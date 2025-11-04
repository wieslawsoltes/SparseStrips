using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Vello.Samples.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
