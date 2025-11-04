using System;
using Vello;
using Vello.Avalonia.Rendering;

namespace Vello.Samples.Avalonia.Rendering;

internal sealed class MotionMarkRenderer : IVelloRenderer, IDisposable
{
    private readonly MotionMarkScene _scene = new();
    private int _complexity = 8;
    private bool _disposed;

    public int Complexity
    {
        get => _complexity;
        set
        {
            value = Math.Clamp(value, 0, 24);
            if (_complexity == value)
                return;

            _complexity = value;
            _scene.SetComplexity(value);
        }
    }

    public int ElementCount => _scene.ElementCount;

    public void Render(RenderContext context, int pixelWidth, int pixelHeight)
    {
        ThrowIfDisposed();
        _scene.Render(context, pixelWidth, pixelHeight);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _scene.Dispose();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MotionMarkRenderer));
    }
}
