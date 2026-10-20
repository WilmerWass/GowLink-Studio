using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media;

using WASSLink.Desktop.ViewModels;

namespace WASSLink.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
        UpdateAccentResources(viewModel.AccentColor);
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Closed += (_, _) => viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (sender is MainViewModel viewModel &&
            args.PropertyName == nameof(MainViewModel.AccentColor))
        {
            UpdateAccentResources(viewModel.AccentColor);
        }
    }

    private void UpdateAccentResources(string value)
    {
        var color = Color.Parse(value);
        Resources["AccentBrush"] = new SolidColorBrush(color);
        Resources["AccentHoverBrush"] = new SolidColorBrush(BlendWithWhite(color, 0.08));
        Resources["AccentLightBrush"] = new SolidColorBrush(BlendWithWhite(color, 0.30));
        Resources["AccentPaleBrush"] = new SolidColorBrush(BlendWithWhite(color, 0.56));
    }

    private static Color BlendWithWhite(Color color, double amount) =>
        Color.FromRgb(
            BlendChannel(color.R, amount),
            BlendChannel(color.G, amount),
            BlendChannel(color.B, amount));

    private static byte BlendChannel(byte value, double amount) =>
        (byte)Math.Round(value + (byte.MaxValue - value) * amount);
}
