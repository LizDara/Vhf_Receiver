using System;
using System.Collections.Generic;
using VhfReceiver.Utils;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class ReceiverConfigurationPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;
        public ReceiverConfigurationPage(DeviceInformation device)
        {
            InitializeComponent();

            ConnectedDevice = device;

            Name.Text = ConnectedDevice.Name;
            Range.Text = ConnectedDevice.Range;
            Battery.Text = ConnectedDevice.Battery;

            Console.WriteLine("Initialize Component");
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void EditFrequencyTables_Tapped(object sender, EventArgs e)
        {
            byte[] bytes = new byte[1];
            var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
            if (service != null)
            {
                var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_FREQ_TABLE);
                if (characteristic != null)
                {
                    bytes = await characteristic.ReadAsync();
                }
            }

            await Navigation.PushModalAsync(new TablesPage(ConnectedDevice, bytes));
        }

        private async void EditReceiverDetails_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new EditReceiverDefaultsPage(ConnectedDevice));
        }

        private async void SetDetectionFilter_Tapped(object sender, EventArgs e)
        {

        }

        private async void CloneFromOtherReceiver_Tapped(object sender, EventArgs e)
        {

        }
    }
}
