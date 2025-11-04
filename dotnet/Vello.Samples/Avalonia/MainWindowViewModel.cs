using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vello.Avalonia.Rendering;
using Vello.Samples.Avalonia.Rendering;

namespace Vello.Samples.Avalonia;

internal sealed class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly MotionMarkRenderer _renderer = new();
    private int _elementCount;
    private double _frameTimeMs;
    private double _fps;
    private bool _useMultithreaded = !OperatingSystem.IsBrowser();
    private bool _disposed;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Complexity
    {
        get => _renderer.Complexity;
        set
        {
            ThrowIfDisposed();
            if (_renderer.Complexity == value)
                return;

            _renderer.Complexity = value;
            OnPropertyChanged();
            ElementCount = _renderer.ElementCount;
        }
    }

    public bool IsMultithreadedToggleEnabled => !OperatingSystem.IsBrowser();

    public int ElementCount
    {
        get => _elementCount;
        set
        {
            if (_elementCount != value)
            {
                _elementCount = value;
                OnPropertyChanged();
            }
        }
    }

    public double FrameTimeMilliseconds
    {
        get => _frameTimeMs;
        set
        {
            if (Math.Abs(_frameTimeMs - value) > 0.0001)
            {
                _frameTimeMs = value;
                OnPropertyChanged();
            }
        }
    }

    public double FramesPerSecond
    {
        get => _fps;
        set
        {
            if (Math.Abs(_fps - value) > 0.0001)
            {
                _fps = value;
                OnPropertyChanged();
            }
        }
    }

    public bool UseMultithreadedRendering
    {
        get => _useMultithreaded;
        set
        {
            if (_useMultithreaded == value)
                return;

            _useMultithreaded = value;
            OnPropertyChanged();
        }
    }

    public IVelloRenderer Renderer
    {
        get
        {
            ThrowIfDisposed();
            return _renderer;
        }
    }

    public void OnFrameStats(VelloFrameStats stats)
    {
        ThrowIfDisposed();
        FrameTimeMilliseconds = stats.FrameTimeMilliseconds;
        FramesPerSecond = stats.FramesPerSecond;
        ElementCount = _renderer.ElementCount;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _renderer.Dispose();
        _disposed = true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MainWindowViewModel));
    }
}
