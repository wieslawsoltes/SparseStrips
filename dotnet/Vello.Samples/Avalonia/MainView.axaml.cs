using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using Vello.Samples.Avalonia.Controls;
using Vello.Samples.Avalonia.Rendering;

namespace Vello.Samples.Avalonia;

public partial class MainView : UserControl
{
    private readonly MainWindowViewModel _viewModel = new();
    private VelloSurface? _surface;
    private bool _frameStatsSubscribed;

    public MainView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnFrameStatsUpdated(object? sender, FrameStats stats)
    {
        _viewModel.Complexity = stats.Complexity;
        _viewModel.ElementCount = stats.ElementCount;
        _viewModel.FrameTimeMilliseconds = stats.FrameTimeMilliseconds;
        _viewModel.FramesPerSecond = stats.FramesPerSecond;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _surface ??= this.FindControl<VelloSurface>("Surface");
        if (_surface is not null && !_frameStatsSubscribed)
        {
            _surface.FrameStatsUpdated += OnFrameStatsUpdated;
            _frameStatsSubscribed = true;
        }

        if (e.Root is TopLevel topLevel)
        {
            topLevel.RendererDiagnostics.DebugOverlays =
                RendererDebugOverlays.Fps |
                RendererDebugOverlays.LayoutTimeGraph |
                RendererDebugOverlays.RenderTimeGraph;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_surface is not null && _frameStatsSubscribed)
        {
            _surface.FrameStatsUpdated -= OnFrameStatsUpdated;
            _frameStatsSubscribed = false;
        }

        base.OnDetachedFromVisualTree(e);
    }
}
