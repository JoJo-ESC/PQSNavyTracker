using Avalonia.Controls;
using PqsTracker.Desktop.ViewModels;

namespace PqsTracker.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Async work doesn't belong in a constructor (nothing could await
        // it, and exceptions there are awkward to surface) — Loaded is the
        // first point where it's safe to kick off the initial API calls.
        Loaded += async (_, _) =>
        {
            if (DataContext is MainViewModel vm)
                await vm.InitializeAsync();
        };
    }
}