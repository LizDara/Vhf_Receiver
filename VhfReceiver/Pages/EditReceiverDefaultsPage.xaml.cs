using System;
using System.Collections.Generic;
using VhfReceiver.Utils;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class EditReceiverDefaultsPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;

        public EditReceiverDefaultsPage(DeviceInformation device)
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

        private async void EditMobileDefaults_Tapped(object sender, EventArgs e)
        {
            var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
            if (service != null)
            {
                var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
                if (characteristic != null)
                {
                    var bytes = await characteristic.ReadAsync();

                    await Navigation.PushModalAsync(new MobileDefaultsPage(ConnectedDevice, bytes));
                }
            }
        }

        private async void EditStationaryDefaults_Tapped(object sender, EventArgs e)
        {
            var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
            if (service != null)
            {
                var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY);
                if (characteristic != null)
                {
                    var bytes = await characteristic.ReadAsync();

                    await Navigation.PushModalAsync(new StationaryDefaultsPage(ConnectedDevice, bytes));
                }
            }
        }
    }
}
