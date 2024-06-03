using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MixerStudio.ViewModels;
using System;

namespace MixerStudio.Views
{
    public partial class AppearanceView : UserControl
    {
        public AppearanceView()
        {
            InitializeComponent();

            DCKPNL_SuperPanel.Initialized += Dckpnl_SuperPanel_OnInitialized;
        }

        private void Dckpnl_SuperPanel_OnInitialized(object? _Sender, EventArgs e)
        {
            if (_Sender is null)
            { return; }
            (this.DataContext as AppearanceViewModel).InitControls(_Sender as Panel);
            
        }
    }
}