using System;
using System.Collections.Generic;
using Plugin.BLE.Abstractions.Contracts;
using VhfReceiver.Utils;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class StartScanningPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;

        public StartScanningPage(DeviceInformation device)
        {
            InitializeComponent();

            ConnectedDevice = device;

            Name.Text = ConnectedDevice.Name;
            Range.Text = ConnectedDevice.Range;
            Battery.Text = ConnectedDevice.Battery;
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void ManualScan_Tapped(object sender, EventArgs e)
        {
            
        }
        
        private async void MobileScan_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new MobileSettingsPage(ConnectedDevice));
        }

        private async void StationaryScan_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new StationarySettingsPage(ConnectedDevice));
        }
    }
}
