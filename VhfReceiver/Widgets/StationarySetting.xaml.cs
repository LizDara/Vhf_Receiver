using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VhfReceiver.Pages;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class StationarySetting : ContentView
    {
        private readonly ReceiverInformation ReceiverInformation;
        public Dictionary<string, object> OriginalData;
        public byte[] StationaryBytes;
        public int firstTable = 255;
        public int secondTable = 255;
        public int thirdTable = 255;
        public int antennas = 255;
        public int scanTime = 255;
        public int scanTimeout = 255;
        public bool externalDataTransfer = true;
        public int storeRate = 255;
        public bool isReferenceFrequency = false;
        public int referenceFrequency = 255;
        public int referenceFrequencyStoreRate = 255;

        public StationarySetting()
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetInstance();

            MessagingCenter.Subscribe<int[]>(this, ValueCodes.VALUE, async (value) => {
                switch (value[0])
                {
                    case ValueCodes.TABLES_NUMBER_CODE:
                        string tables = "";
                        for (int i = 1; i < value.Length; i++)
                            tables += value[i] + ", ";
                        FrequencyTableNumber.Text = tables.Substring(0, tables.Length - 2);
                        firstTable = value[1];
                        secondTable = value.Length > 2 ? value[2] : 0;
                        thirdTable = value.Length > 3 ? value[3] : 0;
                        break;
                    case ValueCodes.NUMBER_OF_ANTENNAS_CODE:
                        NumberOfAntennas.Text = value[1].ToString();
                        antennas = value[1];
                        break;
                    case ValueCodes.SCAN_RATE_SECONDS_CODE:
                        ScanTime.Text = value[1].ToString();
                        scanTime = value[1];
                        break;
                    case ValueCodes.SCAN_TIMEOUT_SECONDS_CODE:
                        ScanTimeout.Text = value[1].ToString();
                        scanTimeout = value[1];
                        break;
                    case ValueCodes.STORE_RATE_CODE:
                        StoreRate.Text = (value[1] == 0) ? "Continuous Store" : value[1].ToString();
                        storeRate = value[1];
                        break;
                    case ValueCodes.REFERENCE_FREQUENCY_CODE:
                        ReferenceFrequency.Text = Converters.GetFrequency(value[1]);
                        referenceFrequency = value[1];
                        break;
                    case ValueCodes.REFERENCE_FREQUENCY_STORE_RATE_CODE:
                        ReferenceFrequencyStoreRate.Text = value[1].ToString();
                        referenceFrequencyStoreRate = value[1];
                        break;
                }
            });
        }

        public void SetData(byte[] bytes, int baseFrequency, bool isEnabled)
        {
            StationaryBytes = bytes;
            if (!Converters.IsDefaultEmpty(bytes))
            {
                string tables = "";
                for (int i = 9; i < bytes.Length; i++)
                {
                    if (bytes[i] != 0)
                        tables += bytes[i] + ", ";
                }
                FrequencyTableNumber.Text = tables.Equals("") ? "None" : tables.Substring(0, tables.Length - 2);
                firstTable = bytes[9];
                secondTable = bytes[10];
                thirdTable = bytes[11];

                antennas = bytes[1];
                NumberOfAntennas.Text = (antennas == 0) ? "None" : antennas.ToString();

                externalDataTransfer = bytes[2] != 0;
                ExternalDataTransfer.IsToggled = externalDataTransfer;

                scanTime = bytes[3];
                ScanTime.Text = bytes[3].ToString();

                scanTimeout = bytes[4];
                ScanTimeout.Text = bytes[4].ToString();

                storeRate = bytes[5];
                if (storeRate == 0)
                    StoreRate.Text = "Continuous Store";
                else
                    StoreRate.Text = storeRate.ToString();

                referenceFrequency = 0;
                if (bytes[6] != 0 && bytes[7] != 0)
                    referenceFrequency = (bytes[6] * 256) + bytes[7] + baseFrequency;
                ReferenceFrequency.Text = referenceFrequency != 0 ? referenceFrequency.ToString() : "Not Set";
                referenceFrequencyStoreRate = bytes[8];
                ReferenceFrequencyStoreRate.Text = bytes[8] != 255 ? bytes[8].ToString() : "Not Set";
            }
            else
            {
                FrequencyTableNumber.Text = "Not Set";
                NumberOfAntennas.Text = "Not Set";
                ExternalDataTransfer.IsToggled = true;
                ScanTime.Text = "Not Set";
                ScanTimeout.Text = "Not Set";
                StoreRate.Text = "Not Set";
                ReferenceFrequency.Text = "Not Set";
                ReferenceFrequencyStoreRate.Text = "Not Set";

            }

            OriginalData = new Dictionary<string, object>
            {
                { ValueCodes.FIRST_TABLE_NUMBER, firstTable },
                { ValueCodes.SECOND_TABLE_NUMBER, secondTable },
                { ValueCodes.THIRD_TABLE_NUMBER, thirdTable },
                { ValueCodes.ANTENNA_NUMBER, antennas },
                { ValueCodes.SCAN_RATE, scanTime },
                { ValueCodes.EXTERNAL_DATA_TRANSFER, externalDataTransfer },
                { ValueCodes.SCAN_TIMEOUT, scanTimeout },
                { ValueCodes.STORE_RATE, storeRate },
                { ValueCodes.REFERENCE_FREQUENCY, referenceFrequency },
                { ValueCodes.REFERENCE_FREQUENCY_STORE_RATE, referenceFrequencyStoreRate }
            };
            ReferenceFrequencySwitch.IsToggled = referenceFrequency != 0;

            if (!isEnabled)
            {
                ScanTimeoutTap.IsEnabled = false;
                ScanTimeTap.IsEnabled = false;
                StoreRateTap.IsEnabled = false;
                FrequencyTableNumberTap.IsEnabled = false;
                NumberOfAntennasTap.IsEnabled = false;
                ExternalDataTransfer.IsEnabled = false;
                ReferenceFrequencySwitch.IsEnabled = false;
                ReferenceFrequencyTap.IsEnabled = false;
                ReferenceFrequencyStoreRateTap.IsEnabled = false;
            }
        }

        private async void FrequencyTableNumber_Tapped(object sender, EventArgs e)
        {
            byte[] bytes = await TransferBLEData.ReadTables();
            if (bytes != null)
                await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, bytes, ValueCodes.TABLES_NUMBER_CODE), false);
        }

        private async void ScanTime_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.SCAN_RATE_SECONDS_CODE), false);
        }

        private async void ScanTimeout_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.SCAN_TIMEOUT_SECONDS_CODE), false);
        }

        private async void NumberOfAntennas_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.NUMBER_OF_ANTENNAS_CODE), false);
        }

        private async void StoreRate_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.STORE_RATE_CODE), false);
        }

        private async void ReferenceFrequency_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.REFERENCE_FREQUENCY_CODE), false);
        }

        private async void ReferenceFrequencyStoreRate_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(StationaryBytes, ValueCodes.REFERENCE_FREQUENCY_STORE_RATE_CODE), false);
        }

        private void ReferenceFrequency_Toggled(object sender, ToggledEventArgs e)
        {
            Console.WriteLine("TOGGLED REFERENCE FREQUENCY: " + e.Value);
            isReferenceFrequency = e.Value;
            ReferenceFrequencyTap.IsEnabled = e.Value;
            ReferenceFrequencyStoreRateTap.IsEnabled = e.Value;
            if (e.Value)
            {
                referenceFrequency = (int)OriginalData[ValueCodes.REFERENCE_FREQUENCY];
                ReferenceFrequency.Text = referenceFrequency != 0 && referenceFrequency != 255 ? Converters.GetFrequency(referenceFrequency) : "Not Set";
                referenceFrequencyStoreRate = (int)OriginalData[ValueCodes.REFERENCE_FREQUENCY_STORE_RATE];
                ReferenceFrequencyStoreRate.Text = referenceFrequencyStoreRate != 255 ? referenceFrequencyStoreRate.ToString() : "Not Set";
            }
            else
            {
                ReferenceFrequency.Text = "No Reference Frequency";
                ReferenceFrequencyStoreRate.Text = "No Reference Frequency";
                referenceFrequency = 0;
                referenceFrequencyStoreRate = 255;
            }
        }

        private void ExternalDataTransfer_Toggled(object sender, ToggledEventArgs e)
        {
            externalDataTransfer = e.Value;
        }
    }
}
