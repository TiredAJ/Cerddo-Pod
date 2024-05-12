using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace Cerddo_Pod.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        
        //sldr_Progress.AddHandler(RoutedEvent.Register<RangeBaseValueChangedEventArgs>("Sldr_ValChanged", RoutingStrategies.Tunnel, this.GetType()), Handler);
        
    }
}