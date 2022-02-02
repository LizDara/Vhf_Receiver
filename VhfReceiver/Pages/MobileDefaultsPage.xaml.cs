using System;
using System.Collections.Generic;
using VhfReceiver.Utils;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class MobileDefaultsPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;
        private int[] DefaultData;
        private byte[] Bytes;

        public MobileDefaultsPage(DeviceInformation device, byte[] bytes)
        {
            InitializeComponent();

            ConnectedDevice = device;

            Name.Text = ConnectedDevice.Name;
            Range.Text = ConnectedDevice.Range;
            Battery.Text = ConnectedDevice.Battery;

            Bytes = bytes;
            SetData(bytes);

            MessagingCenter.Subscribe<int[]>(this, "Value", (value) => {
                switch (value[0])
                {
                    case ValueDefaultsPage.FREQUENCY_TABLE_NUMBER:
                        FrequencyTableNumber.Text = value[1].ToString();
                        break;
                    case ValueDefaultsPage.SCAN_TIME_SECONDS:
                        double scanTime = (double)(value[1] * 0.1);
                        ScanTime.Text = (scanTime.ToString().Contains(".")) ? scanTime.ToString() : scanTime.ToString() + ".0";
                        break;
                }
            });
        }

        private void SetData(byte[] bytes)
        {
            FrequencyTableNumber.Text = (bytes[1] == 0) ? "None" : bytes[1].ToString();

            int gps = bytes[2] >> 7 & 1;
            GPS.IsToggled = gps == 1;

            int autoRecord = bytes[2] >> 6 & 1;
            AutoRecord.IsToggled = autoRecord == 1;

            double scanTime = (double)(bytes[3] * 0.1);
            ScanTime.Text = (scanTime.ToString().Contains(".")) ? scanTime.ToString() : scanTime.ToString() + ".0";

            DefaultData = new int[] { bytes[1], (gps == 1) ? 1: 0, (autoRecord == 1) ? 1 : 0, (int) (scanTime * 10) };
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (ExistChanges())
            {
                var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
                    if (characteristic != null)
                    {
                        int info = (GPS.IsToggled ? 1 : 0) << 7;
                        info = info | ((AutoRecord.IsToggled ? 1 : 0) << 6);
                        float scanTime = float.Parse(ScanTime.Text);
                        int frequencyTableNumber = (FrequencyTableNumber.Text.Equals("None") ? 0 : int.Parse(FrequencyTableNumber.Text));

                        byte[] b = new byte[] { 0x4D, (byte) frequencyTableNumber, (byte) info, (byte) (scanTime * 10), 0, 0, 0, 0 };

                        await characteristic.WriteAsync(b);
                    }
                }
            }

            await Navigation.PopModalAsync();
        }

        private async void FrequencyTableNumber_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(ConnectedDevice, Bytes, ValueDefaultsPage.FREQUENCY_TABLE_NUMBER));
        }

        private async void ScanTime_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(ConnectedDevice, Bytes, ValueDefaultsPage.SCAN_TIME_SECONDS));
        }

        private bool ExistChanges()
        {
            int frequencyTableNumber = (FrequencyTableNumber.Text.Equals("None")) ? 0 : int.Parse(FrequencyTableNumber.Text);
            int gps = GPS.IsToggled ? 1 : 0;
            int autoRecord = AutoRecord.IsToggled ? 1 : 0;
            int scanTime = (int)(float.Parse(ScanTime.Text) * 10);

            return (DefaultData[0] != frequencyTableNumber || DefaultData[1] != gps || DefaultData[2] != autoRecord || DefaultData[3] != scanTime);
        }
    }
}
