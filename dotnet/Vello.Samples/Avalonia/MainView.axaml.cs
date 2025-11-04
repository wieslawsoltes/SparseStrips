using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using Vello.Avalonia.Controls;
using Vello.Avalonia.Rendering;

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

    private void OnFrameStatsUpdated(object? sender, VelloFrameStats stats)
        => _viewModel.OnFrameStats(stats);

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
