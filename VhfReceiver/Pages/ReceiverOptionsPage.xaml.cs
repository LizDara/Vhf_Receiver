using System;
using System.Collections.Generic;
using Plugin.BLE.Abstractions.Contracts;
using VhfReceiver.Utils;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class ReceiverOptionsPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;

        public ReceiverOptionsPage(DeviceInformation device)
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

        private async void ReceiverConfiguration_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ReceiverConfigurationPage(ConnectedDevice));
        }

        private async void ManageReceiverData_Tapped(object sender, EventArgs e)
        {
            byte[] bytes = new byte[1];
            var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_DIAGNOSTIC);
            if (service != null)
            {
                var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_DIAGNOSTIC_INFO);
                if (characteristic != null)
                {
                    bytes = await characteristic.ReadAsync();
                }
            }

            await Navigation.PushModalAsync(new ManageReceiverDataPage(ConnectedDevice, bytes));
        }

        private async void TestReceiver_Tapped(object sender, EventArgs e)
        {
            byte[] bytes = new byte[1];
            var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_DIAGNOSTIC);
            if (service != null)
            {
                var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_DIAGNOSTIC_INFO);
                if (characteristic != null)
                {
                    bytes = await characteristic.ReadAsync();
                }
            }

            await Navigation.PushModalAsync(new TestReceiverPage(ConnectedDevice, bytes));
        }

        private async void CheckForUpdates_Tapped(object sender, EventArgs e)
        {

        }
    }
}
