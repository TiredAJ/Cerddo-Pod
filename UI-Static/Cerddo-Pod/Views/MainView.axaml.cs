using Avalonia.Controls;

namespace Cerddo_Pod.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        
        //sldr_Progress.AddHandler(RoutedEvent.Register<RangeBaseValueChangedEventArgs>("Sldr_ValChanged", RoutingStrategies.Tunnel, this.GetType()), Handler);
        
    }
}