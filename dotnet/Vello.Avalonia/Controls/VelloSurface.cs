using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.VisualTree;
using Vello;
using Vello.Avalonia.Rendering;

namespace Vello.Avalonia.Controls;

/// <summary>
/// Avalonia control that hosts a Vello render context and blits frames into a <see cref="WriteableBitmap"/>.
/// </summary>
public sealed class VelloSurface : Control
{
    public static readonly StyledProperty<IVelloRenderer?> RendererProperty =
        AvaloniaProperty.Register<VelloSurface, IVelloRenderer?>(
            nameof(Renderer));

    public static readonly StyledProperty<bool> UseMultithreadedRenderingProperty =
        AvaloniaProperty.Register<VelloSurface, bool>(
            nameof(UseMultithreadedRendering),
            true);

    private WriteableBitmap? _bitmap;
    private RenderContext? _context;
    private byte[]? _scratchBuffer;
    private Vector _bitmapDpi = new(96, 96);
    private bool _frameRequested;
    private bool _isAttached;
    private TimeSpan? _lastFrameTimestamp;
    private double _statsAccumulatorMs;
    private int _statsFrameCount;
    private bool _renderFailed;
    private string? _lastRenderError;
    private IVelloRenderer? _renderer;
    private bool _useMultithreadedRendering = !OperatingSystem.IsBrowser();

    public VelloSurface()
    {
        ClipToBounds = true;
        UseMultithreadedRendering = !OperatingSystem.IsBrowser();
    }

    /// <summary>
    /// Gets or sets the renderer that produces pixel output for this surface.
    /// </summary>
    public IVelloRenderer? Renderer
    {
        get => GetValue(RendererProperty);
        set => SetValue(RendererProperty, value);
    }

    /// <summary>
    /// Gets or sets whether Vello should use multithreaded rendering.
    /// </summary>
    public bool UseMultithreadedRendering
    {
        get => GetValue(UseMultithreadedRenderingProperty);
        set => SetValue(UseMultithreadedRenderingProperty, value);
    }

    /// <summary>
    /// Raised periodically with averaged frame timing statistics.
    /// </summary>
    public event EventHandler<VelloFrameStats>? FrameStatsUpdated;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RendererProperty)
        {
            UpdateRenderer((IVelloRenderer?)change.NewValue);
        }
        else if (change.Property == UseMultithreadedRenderingProperty)
        {
            UpdateUseMultithreadedRendering(change.GetNewValue<bool>());
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        RequestNextFrame();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isAttached = false;
        _frameRequested = false;
        _lastFrameTimestamp = null;
        _statsAccumulatorMs = 0;
        _statsFrameCount = 0;
        DisposeResources();
    }

    public override void Render(DrawingContext context)
    {
        try
        {
            base.Render(context);

            var renderer = Volatile.Read(ref _renderer);
            if (renderer is null)
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
                return;

            double scaling = topLevel.RenderScaling;
            PixelSize pixelSize = PixelSize.FromSize(Bounds.Size, scaling);

            if (pixelSize.Width <= 0 || pixelSize.Height <= 0)
                return;

            if (pixelSize.Width > ushort.MaxValue || pixelSize.Height > ushort.MaxValue)
                return;

            var destRect = new Rect(Bounds.Size);
            context.Custom(new VelloDrawOperation(this, pixelSize, scaling, destRect));
        }
        catch (Exception ex)
        {
            LogRenderError("rendering", ex);
            _renderFailed = true;
        }
    }

    private void RenderFrame(ImmediateDrawingContext drawingContext, PixelSize pixelSize, double scaling, Rect destRect)
    {
        try
        {
            EnsureResources(pixelSize, scaling);

            if (_context is null || _bitmap is null)
            {
                _renderFailed = true;
                return;
            }

            var renderer = Volatile.Read(ref _renderer);
            if (renderer is null)
                return;

            renderer.Render(_context, pixelSize.Width, pixelSize.Height);

            VelloBitmapBlitter.Blit(_context, _bitmap, ref _scratchBuffer);

            var sourceRect = new Rect(0, 0, _bitmap.PixelSize.Width, _bitmap.PixelSize.Height);
            drawingContext.DrawBitmap(_bitmap, sourceRect, destRect);

            _renderFailed = false;
        }
        catch (Exception ex)
        {
            LogRenderError("rendering frame", ex);
            _renderFailed = true;
            DisposeResources();
        }
    }

    private void EnsureResources(PixelSize pixelSize, double scaling)
    {
        bool resized = false;
        Vector requestedDpi = new Vector(96, 96) * scaling;

        if (_bitmap is null || _bitmap.PixelSize != pixelSize || _bitmapDpi != requestedDpi)
        {
            _bitmap?.Dispose();
            _bitmap = new WriteableBitmap(
                pixelSize,
                requestedDpi,
                PixelFormats.Rgba8888,
                AlphaFormat.Premul);
            _bitmapDpi = requestedDpi;
            resized = true;
        }

        if (_context is null ||
            _context.Width != (ushort)pixelSize.Width ||
            _context.Height != (ushort)pixelSize.Height)
        {
            _context?.Dispose();
            try
            {
                _context = CreateRenderContext((ushort)pixelSize.Width, (ushort)pixelSize.Height);
                _renderFailed = false;
                resized = true;
            }
            catch (Exception ex)
            {
                _context = null;
                LogRenderError("creating render context", ex);
                _renderFailed = true;
            }
        }

        if (resized)
        {
            _scratchBuffer = null;
        }
    }

    private void DisposeResources()
    {
        _context?.Dispose();
        _context = null;

        _bitmap?.Dispose();
        _bitmap = null;

        _scratchBuffer = null;
        _statsAccumulatorMs = 0;
        _statsFrameCount = 0;
        _renderFailed = false;
        _lastRenderError = null;
    }

    private void RequestNextFrame()
    {
        if (!_isAttached || _frameRequested)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return;

        _frameRequested = true;
        topLevel.RequestAnimationFrame(OnAnimationFrame);
    }

    private void OnAnimationFrame(TimeSpan timestamp)
    {
        _frameRequested = false;

        if (!_isAttached)
            return;

        if (_lastFrameTimestamp is TimeSpan last)
        {
            double deltaMs = (timestamp - last).TotalMilliseconds;
            if (deltaMs > 0 && deltaMs < 250)
            {
                _statsAccumulatorMs += deltaMs;
                _statsFrameCount++;

                const double statsWindowMs = 500.0;
                if (_statsAccumulatorMs >= statsWindowMs)
                {
                    double averageFrameMs = _statsAccumulatorMs / _statsFrameCount;
                    double fps = averageFrameMs > 0 ? 1000.0 / averageFrameMs : 0;
                    int width = _bitmap?.PixelSize.Width ?? 0;
                    int height = _bitmap?.PixelSize.Height ?? 0;
                    var stats = new VelloFrameStats(averageFrameMs, fps, width, height);
                    FrameStatsUpdated?.Invoke(this, stats);
                    _statsAccumulatorMs = 0;
                    _statsFrameCount = 0;
                }
            }
        }

        _lastFrameTimestamp = timestamp;
        InvalidateVisual();
        if (!_renderFailed && Volatile.Read(ref _renderer) is not null)
        {
            RequestNextFrame();
        }
    }

    private RenderContext CreateRenderContext(ushort width, ushort height)
        => Volatile.Read(ref _useMultithreadedRendering)
            ? new RenderContext(width, height)
            : new RenderContext(width, height, RenderSettings.SingleThreaded);

    private void LogRenderError(string stage, Exception exception)
    {
        Exception root = exception.GetBaseException();
        string detail = root == exception ? root.ToString() : $"{root}{Environment.NewLine}{exception}";

        if (root is VelloException velloEx)
        {
            detail = $"{velloEx.Message} (code {velloEx.ErrorCode}){Environment.NewLine}{detail}";
        }

        string message = $"[VelloSurface] Error while {stage}: {detail}";
        if (message == _lastRenderError)
            return;

        _lastRenderError = message;
        Console.Error.WriteLine(message);
    }

    private void UpdateRenderer(IVelloRenderer? renderer)
    {
        Volatile.Write(ref _renderer, renderer);

        if (renderer is null)
        {
            DisposeResources();
            return;
        }

        _renderFailed = false;
        _lastRenderError = null;
        RequestNextFrame();
    }

    private void UpdateUseMultithreadedRendering(bool useMultithreaded)
    {
        Volatile.Write(ref _useMultithreadedRendering, useMultithreaded);

        DisposeResources();
        _renderFailed = false;
        _lastRenderError = null;
        RequestNextFrame();
    }

    private sealed class VelloDrawOperation : ICustomDrawOperation
    {
        private readonly VelloSurface _owner;
        private readonly PixelSize _pixelSize;
        private readonly double _scaling;
        private readonly Rect _destRect;

        public VelloDrawOperation(VelloSurface owner, PixelSize pixelSize, double scaling, Rect destRect)
        {
            _owner = owner;
            _pixelSize = pixelSize;
            _scaling = scaling;
            _destRect = destRect;
        }

        public Rect Bounds => _destRect;

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => _destRect.Contains(p);

        public void Render(ImmediateDrawingContext context)
        {
            _owner.RenderFrame(context, _pixelSize, _scaling, _destRect);
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is VelloDrawOperation op &&
                   ReferenceEquals(op._owner, _owner) &&
                   op._pixelSize == _pixelSize &&
                   op._destRect == _destRect &&
                   Math.Abs(op._scaling - _scaling) < double.Epsilon;
        }
    }
}
