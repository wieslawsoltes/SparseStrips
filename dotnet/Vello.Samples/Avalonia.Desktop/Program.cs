using System;
using Avalonia;

namespace Vello.Samples.Avalonia.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<Vello.Samples.Avalonia.App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
