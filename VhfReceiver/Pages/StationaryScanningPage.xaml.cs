using System;
using System.Collections.Generic;
using VhfReceiver.Utils;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class StationaryScanningPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;

        public StationaryScanningPage(DeviceInformation device)
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

        private async void ScanWithDefaultSettings_Tapped(object sender, EventArgs e)
        {

        }

        private async void ScanWithTemporarySettings_Tapped(object sender, EventArgs e)
        {

        }
    }
}
