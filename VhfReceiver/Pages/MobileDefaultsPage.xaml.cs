using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class MobileDefaultsPage : ContentPage
    {
        private readonly ReceiverInformation ReceiverInformation;
        private Dictionary<string, object> OriginalData;
        private readonly byte[] MobileBytes;

        public MobileDefaultsPage(byte[] bytes)
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();

            MobileBytes = bytes;
            SetData(bytes);

            MessagingCenter.Subscribe<int[]>(this, ValueCodes.VALUE, (value) => {
                switch (value[0])
                {
                    case ValueCodes.FREQUENCY_TABLE_NUMBER:
                        FrequencyTableNumber.Text = value[1].ToString();
                        break;
                    case ValueCodes.SCAN_RATE_SECONDS:
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

            OriginalData = new Dictionary<string, object>
            {
                { "TableNumber", (int)bytes[1] },
                { "Gps", gps == 1 },
                { "AutoRecord", autoRecord == 1 },
                { "ScanTime", scanTime }
            };
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (ExistChanges())
            {
                bool result = await SetMobileDefaults();
                if (!result)
                    return;
            }
            await Navigation.PopModalAsync(false);
        }

        private async void FrequencyTableNumber_Tapped(object sender, EventArgs e)
        {
            byte[] bytes = await GetTables();
            if (bytes != null)
                await Navigation.PushModalAsync(new ValueDefaultsPage(MobileBytes, bytes, ValueCodes.FREQUENCY_TABLE_NUMBER), false);
        }

        private async void ScanTime_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(MobileBytes, ValueCodes.SCAN_RATE_SECONDS), false);
        }

        private async Task<bool> SetMobileDefaults()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
                    if (characteristic != null)
                    {
                        int info = (GPS.IsToggled ? 1 : 0) << 7;
                        info |= ((AutoRecord.IsToggled ? 1 : 0) << 6);
                        float scanTime = float.Parse(ScanTime.Text);
                        int frequencyTableNumber = (FrequencyTableNumber.Text.Equals("None") ? 0 : int.Parse(FrequencyTableNumber.Text));

                        byte[] b = new byte[] { 0x4D, (byte)frequencyTableNumber, (byte)info, (byte)(scanTime * 10), 0, 0, 0, 0 };

                        bool result = await characteristic.WriteAsync(b);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return false;
        }

        private async Task<byte[]> GetTables()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_FREQ_TABLE);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return null;
        }

        private bool ExistChanges()
        {
            int frequencyTableNumber = FrequencyTableNumber.Text.Equals("None") ? 0 : int.Parse(FrequencyTableNumber.Text);
            double scanTime = double.Parse(ScanTime.Text);

            return (int) OriginalData["TableNumber"] != frequencyTableNumber || (bool) OriginalData["Gps"] != GPS.IsToggled ||
                (bool) OriginalData["AutoRecord"] != AutoRecord.IsToggled || (double) OriginalData["ScanTime"] != scanTime;
        }
    }
}
