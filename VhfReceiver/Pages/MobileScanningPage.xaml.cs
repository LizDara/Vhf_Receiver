using System;
using System.Collections.Generic;
using VhfReceiver.Utils;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class MobileScanningPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;

        public MobileScanningPage(DeviceInformation device)
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

        private async void ScanWithDefaultSettings_Clicked(object sender, EventArgs e)
        {

        }

        private async void ScanWithTemporarySettings_Clicked(object sender, EventArgs e)
        {

        }

        private async void EditSettings_Clicked(object sender, EventArgs e)
        {

        }

        private async void StartMobileScan_Clicked(object sender, EventArgs e)
        {

        }
    }
}
