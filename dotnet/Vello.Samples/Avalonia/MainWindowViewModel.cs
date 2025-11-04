using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vello.Samples.Avalonia;

internal sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private int _complexity = 8;
    private int _elementCount;
    private double _frameTimeMs;
    private double _fps;
    private bool _useMultithreaded = !OperatingSystem.IsBrowser();

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Complexity
    {
        get => _complexity;
        set
        {
            if (_complexity != value)
            {
                _complexity = value;
                OnPropertyChanged();
            }
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
            if (_useMultithreaded != value)
            {
                _useMultithreaded = value;
                OnPropertyChanged();
            }
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
