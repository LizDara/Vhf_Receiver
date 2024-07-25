using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VhfReceiver.Utils;
using Xamarin.Essentials;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class StationaryDefaultsPage : ContentPage
    {
        private readonly ReceiverInformation ReceiverInformation;
        private Dictionary<string, object> OriginalData;
        private readonly byte[] StationaryBytes;

        private readonly int BaseFrequency;

        public StationaryDefaultsPage(byte[] bytes)
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();

            StationaryBytes = bytes;
            BaseFrequency = Preferences.Get("BaseFrequency", 0) * 1000;

            SetData(bytes);

            MessagingCenter.Subscribe<int[]>(this, ValueCodes.VALUE, (value) => {
                switch (value[0])
                {
                    case ValueCodes.TABLES_NUMBER:
                        string tables = "";
                        for (int i = 1; i < value.Length; i++)
                            tables += value[i] + ", ";
                        FrequencyTableNumber.Text = tables.Substring(0, tables.Length - 2);
                        break;
                    case ValueCodes.NUMBER_OF_ANTENNAS:
                        NumberOfAntennas.Text = value[1].ToString();
                        break;
                    case ValueCodes.SCAN_RATE_SECONDS:
                        ScanTime.Text = value[1].ToString();
                        break;
                    case ValueCodes.SCAN_TIMEOUT_SECONDS:
                        ScanTimeout.Text = value[1].ToString();
                        break;
                    case ValueCodes.STORE_RATE:
                        StoreRate.Text = (value[1] == 0) ? "Continuous Store" : value[1].ToString();
                        break;
                    case ValueCodes.REFERENCE_FREQUENCY:
                        ReferenceFrequency.Text = GetFrequency(value[1]);
                        break;
                    case ValueCodes.REFERENCE_FREQUENCY_STORE_RATE:
                        ReferenceFrequencyStoreRate.Text = value[1].ToString();
                        break;
                }
            });
        }

        private void SetData(byte[] bytes)
        {
            string tables = "";
            for (int i = 9; i < bytes.Length; i++)
            {
                if (bytes[i] != 0)
                    tables += bytes[i] + ", ";
            }
            FrequencyTableNumber.Text = tables.Equals("") ? "None" : tables.Substring(0, tables.Length - 2);

            int antennaNumber = bytes[1];
            NumberOfAntennas.Text = (antennaNumber == 0) ? "None" : antennaNumber.ToString();

            ExternalDataTransfer.IsToggled = bytes[2] != 0;

            ScanTime.Text = bytes[3].ToString();

            ScanTimeout.Text = bytes[4].ToString();

            if (Converters.GetHexValue(bytes[5]).Equals("00"))
                StoreRate.Text = "Continuous Store";
            else
                StoreRate.Text = bytes[5].ToString();

            int frequency = 0;
            if (bytes[6] != 0 && bytes[7] != 0)
                frequency = (bytes[6] * 256) + bytes[7] + BaseFrequency;
            ReferenceFrequencyStoreRate.Text = bytes[8].ToString();

            OriginalData = new Dictionary<string, object>
            {
                { "FirstTableNumber", (int)bytes[9] },
                { "SecondTableNumber", (int)bytes[10] },
                { "ThirdTableNumber", (int)bytes[11] },
                { "AntennaNumber", antennaNumber },
                { "ScanTime", (int)bytes[3] },
                { "ExternalDataTransfer", (int)bytes[2] },
                { "ScanTimeout", (int)bytes[4] },
                { "StoreRate", (int)bytes[5] },
                { "ReferenceFrequency", frequency },
                { "ReferenceFrequencyStoreRate", (int)bytes[8] }
            };
            ReferenceFrequencySwitch.IsToggled = frequency != 0;
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (ExistChanges())
            {
                if (IsDataCorrect())
                {
                    bool result = await SetStationaryDefaults();
                    if (!result)
                        return;
                }
            }
            await Navigation.PopModalAsync(false);
        }

        private async void FrequencyTableNumber_Tapped(object sender, EventArgs e)
        {
            byte[] bytes = await GetTables();
            if (bytes != null)
                await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, bytes, ValueCodes.TABLES_NUMBER), false);
        }

        private async void ScanTime_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.SCAN_RATE_SECONDS), false);
        }

        private async void ScanTimeout_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.SCAN_TIMEOUT_SECONDS), false);
        }

        private async void NumberOfAntennas_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.NUMBER_OF_ANTENNAS), false);
        }

        private async void StoreRate_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.STORE_RATE), false);
        }

        private async void ReferenceFrequency_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.REFERENCE_FREQUENCY), false);
        }

        private async void ReferenceFrequencyStoreRate_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.REFERENCE_FREQUENCY_STORE_RATE), false);
        }

        private void ReferenceFrequency_Toggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                ReferenceFrequencyValue.IsEnabled = true;
                int frequency = (int)OriginalData["ReferenceFrequency"];
                ReferenceFrequency.Text = frequency != 0 ? GetFrequency(frequency) : "0";
                ReferenceFrequencyStoreRateValue.IsEnabled = true;
            }
            else
            {
                ReferenceFrequencyValue.IsEnabled = false;
                ReferenceFrequency.Text = "No Reference Frequency";
                ReferenceFrequencyStoreRateValue.IsEnabled = false;
                ReferenceFrequencyStoreRate.Text = "0";
            }
        }

        private async Task<bool> SetStationaryDefaults()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY);
                    if (characteristic != null)
                    {
                        string[] tables = FrequencyTableNumber.Text.Split(", ".ToCharArray());
                        int firstTable = (tables.Length > 0) ? int.Parse(tables[0]) : 0;
                        int secondTable = (tables.Length > 1) ? int.Parse(tables[1]) : 0;
                        int thirdTable = (tables.Length > 2) ? int.Parse(tables[2]) : 0;
                        int antennasNumber = int.Parse(NumberOfAntennas.Text);
                        int scanTime = int.Parse(ScanTime.Text);
                        int scanTimeout = int.Parse(ScanTimeout.Text);
                        int externalData = ExternalDataTransfer.IsToggled ? 1 : 0;
                        int storeRate;
                        if (StoreRate.Text.Equals("Continuous Store"))
                            storeRate = 0;
                        else
                            storeRate = int.Parse(StoreRate.Text);
                        int referenceFrequency = ReferenceFrequencySwitch.IsToggled ?
                            GetFrequencyNumber(ReferenceFrequency.Text) - BaseFrequency : 0;
                        int referenceFrequencyStoreRate = int.Parse(ReferenceFrequencyStoreRate.Text);

                        byte[] b = new byte[] { 0x4C, (byte)antennasNumber, (byte)externalData, (byte)scanTime, (byte)scanTimeout,
                        (byte)storeRate, (byte)(referenceFrequency / 256), (byte)(referenceFrequency % 256),
                        (byte)referenceFrequencyStoreRate, (byte)firstTable, (byte)secondTable, (byte)thirdTable };

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

        private string GetFrequency(int frequency)
        {
            return frequency.ToString().Substring(0, 3) + "." + frequency.ToString().Substring(3);
        }

        private int GetFrequencyNumber(string frequency)
        {
            return int.Parse(frequency.Replace(".", ""));
        }

        private bool ExistChanges()
        {
            string[] tables = FrequencyTableNumber.Text.Split(", ".ToCharArray());
            int firstTable = (tables.Length > 0) ? int.Parse(tables[0]) : 0;
            int secondTable = (tables.Length > 1) ? int.Parse(tables[1]) : 0;
            int thirdTable = (tables.Length > 2) ? int.Parse(tables[2]) : 0;
            int antennaNumber = (NumberOfAntennas.Text.Equals("None")) ? 0 : int.Parse(NumberOfAntennas.Text);
            int scanTime = int.Parse(ScanTime.Text);
            int scanTimeout = int.Parse(ScanTimeout.Text);
            int externalData = ExternalDataTransfer.IsToggled ? 1 : 0;
            int storeRate;
            if (StoreRate.Text.Equals("Continuous Store"))
                storeRate = 0;
            else
                storeRate = int.Parse(StoreRate.Text);
            int referenceFrequency = ReferenceFrequencySwitch.IsToggled ? GetFrequencyNumber(ReferenceFrequency.Text) : 0;
            int referenceFrequencyStoreRate = int.Parse(ReferenceFrequencyStoreRate.Text);

            return (int)OriginalData["FirstTableNumber"] != firstTable || (int)OriginalData["SecondTableNumber"] != secondTable
                || (int)OriginalData["ThirdTableNumber"] != thirdTable || (int)OriginalData["AntennaNumber"] != antennaNumber
                || (int)OriginalData["ScanTime"] != scanTime || (int)OriginalData["ScanTimeout"] != scanTimeout
                || (int)OriginalData["StoreRate"] != storeRate || (int)OriginalData["ReferenceFrequency"] != referenceFrequency
                || (int)OriginalData["ReferenceFrequencyStoreRate"] != referenceFrequencyStoreRate
                || (int)OriginalData["ExternalDataTransfer"] != externalData;
        }

        private bool IsDataCorrect()
        {
            return (int.Parse(ScanTimeout.Text) < int.Parse(ScanTime.Text));
        }
    }
}
