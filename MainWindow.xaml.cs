using System.ComponentModel;
using System.Windows;
using PishpanTimeTracker.ViewModels;

namespace PishpanTimeTracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.StopTask();
        }
        base.OnClosing(e);
    }
}