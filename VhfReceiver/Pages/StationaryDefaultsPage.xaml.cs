using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Essentials;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class StationaryDefaultsPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;
        private int[] DefaultData;
        private byte[] Bytes;

        public StationaryDefaultsPage(DeviceInformation device, byte[] bytes)
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
                    case ValueDefaultsPage.NUMBER_OF_ANTENNAS:
                        NumberOfAntennas.Text = value[1].ToString();
                        break;
                    case ValueDefaultsPage.SCAN_TIME_SECONDS:
                        ScanTime.Text = value[1].ToString();
                        break;
                    case ValueDefaultsPage.SCAN_TIMEOUT_SECONDS:
                        ScanTimeout.Text = value[1].ToString();
                        break;
                    case ValueDefaultsPage.STORE_RATE:
                        StoreRate.Text = (value[1] == 0) ? "No Store Rate" : value[1].ToString();
                        break;
                    case ValueDefaultsPage.REFERENCE_FREQUENCY_STORE_RATE:
                        ReferenceFrequencyStoreRate.Text = value[1].ToString();
                        break;
                }
            });
        }

        private void SetData(byte[] bytes)
        {
            int tableNumber = bytes[1] / 16;
            FrequencyTableNumber.Text = (tableNumber == 0) ? "None" : tableNumber.ToString();

            int antennaNumber = bytes[1] % 16;
            NumberOfAntennas.Text = (antennaNumber == 0) ? "None" : antennaNumber.ToString();

            ScanTime.Text = bytes[3].ToString();

            ScanTimeout.Text = bytes[4].ToString();

            if (Converters.GetHexValue(bytes[5]).Equals("FF"))
            {
                StoreRate.Text = "Continuous Store";
                Next.IsVisible = false;
                StoreRateLayout.IsEnabled = false;
            }
            else
            {
                StoreRate.Text = (bytes[5] == 0) ? "No Store Rate" : bytes[5].ToString();
                Next.IsVisible = true;
                StoreRateLayout.IsEnabled = true;
            }

            int frequency;
            if (bytes[6] == 0 && bytes[7] == 0)
            {
                frequency = 0;
            }
            else
            {
                int baseFrequency = Preferences.Get("BaseFrequency", 0);
                frequency = (bytes[6] * 256) + bytes[7] + (baseFrequency * 1000);
            }
            ReferenceFrequency.Text = (frequency == 0) ? "No Reference Frequency" : frequency.ToString().Substring(0, 3) + "." + frequency.ToString().Substring(3);

            ReferenceFrequencyStoreRate.Text = bytes[8].ToString();

            DefaultData = new int[] { tableNumber, antennaNumber, bytes[3], bytes[4], bytes[5], frequency, bytes[8] };
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (ExistChanges())
            {
                if (IsDataCorrect())
                {
                    var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                    if (service != null)
                    {
                        var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY);
                        if (characteristic != null)
                        {
                            int info = (FrequencyTableNumber.Text.Equals("None") ? 0 : int.Parse(FrequencyTableNumber.Text) * 16) +
                                (NumberOfAntennas.Text.Equals("None") ? 0 : int.Parse(NumberOfAntennas.Text));
                            int scanTime = int.Parse(ScanTime.Text);
                            int scanTimeout = int.Parse(ScanTimeout.Text);
                            int storeRate;
                            switch (StoreRate.Text)
                            {
                                case "No Store":
                                    storeRate = 0;
                                    break;
                                case "Continuous Store":
                                    storeRate = 255;
                                    break;
                                default:
                                    storeRate = int.Parse(StoreRate.Text);
                                    break;
                            }
                            int baseFrequency = Preferences.Get("BaseFrequency", 0);
                            int referenceFrequency = (ReferenceFrequency.Text.Equals("No Reference Frequency")) ? 0 :
                                int.Parse(ReferenceFrequency.Text.Replace(".", "")) - (baseFrequency * 1000);
                            int referenceFrequencyStoreRate = int.Parse(ReferenceFrequencyStoreRate.Text);

                            byte[] b = new byte[] { 0x4C, (byte) info, 0x0, (byte) scanTime, (byte) scanTimeout, (byte) storeRate, (byte) (referenceFrequency / 256), (byte) (referenceFrequency % 256), (byte) referenceFrequencyStoreRate, 0x0, 0x0 };

                            await characteristic.WriteAsync(b);
                        }
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

        private async void ScanTimeout_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(ConnectedDevice, Bytes, ValueDefaultsPage.SCAN_TIMEOUT_SECONDS));
        }

        private async void NumberOfAntennas_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(ConnectedDevice, Bytes, ValueDefaultsPage.NUMBER_OF_ANTENNAS));
        }

        private async void StoreRate_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(ConnectedDevice, Bytes, ValueDefaultsPage.STORE_RATE));
        }

        private async void ReferenceFrequency_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(ConnectedDevice, Bytes, ValueDefaultsPage.REFERENCE_FREQUENCY));
        }

        private async void ReferenceFrequencyStoreRate_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(ConnectedDevice, Bytes, ValueDefaultsPage.REFERENCE_FREQUENCY_STORE_RATE));
        }

        private bool ExistChanges()
        {
            int frequencyTableNumber = (FrequencyTableNumber.Text.Equals("None")) ? 0 : int.Parse(FrequencyTableNumber.Text);
            int antennaNumber = (NumberOfAntennas.Text.Equals("None")) ? 0 : int.Parse(NumberOfAntennas.Text);
            int scanTime = int.Parse(ScanTime.Text);
            int scanTimeout = int.Parse(ScanTimeout.Text);
            int storeRate;
            switch (StoreRate.Text)
            {
                case "No Store Rate":
                    storeRate = 0;
                    break;
                case "Continuous Store":
                    storeRate = 255;
                    break;
                default:
                    storeRate = int.Parse(StoreRate.Text);
                    break;
            }
            int referenceFrequency = (ReferenceFrequency.Text.Equals("No Reference Frequency")) ? 0 : int.Parse(ReferenceFrequency.Text.Replace(".", ""));
            int referenceFrequencyStoreRate = int.Parse(ReferenceFrequencyStoreRate.Text);

            return (DefaultData[0] != frequencyTableNumber || DefaultData[1] != antennaNumber || DefaultData[2] != scanTime || DefaultData[3] != scanTimeout
                || DefaultData[4] != storeRate || DefaultData[5] != referenceFrequency || DefaultData[6] != referenceFrequencyStoreRate);
        }

        private bool IsDataCorrect()
        {
            return (int.Parse(ScanTimeout.Text) < int.Parse(ScanTime.Text));
        }
    }
}
