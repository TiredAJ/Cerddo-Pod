using Avalonia.Controls;
using Cerddo_Pod.ViewModels;
using System;
using System.Diagnostics;

namespace Cerddo_Pod.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    { InitializeComponent(); }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (this.Content is MainView MV && MV.DataContext is MainViewModel MVM)
        { MVM.Closing(); }        
    }
}