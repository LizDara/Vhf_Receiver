using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class StationaryScanningPage : ContentPage
    {
        private bool IsScanning;
        private int BaseFrequency;
        private int NumberOfAntennas;
        private byte DetectionType;

        public StationaryScanningPage(byte[] bytes)
        {
            Initialize(bytes);

            IsScanning = false;
            _ = TransferBLEData.NotificationLog(ValueUpdateScan); // Log sd card state and battery
        }

        public StationaryScanningPage(ICharacteristic characteristic, byte[] bytes, byte[] value)
        {
            Initialize(bytes);

            IsScanning = true;
            int currentFrequency = (value[16] * 256) + value[17];
            int currentIndex = (value[7] * 256) + value[8];
            int currentAntenna = value[9];
            FrequencyScan.Text = (currentFrequency + BaseFrequency).ToString();
            TableIndex.Text = currentIndex.ToString();
            CurrentAntenna.Text = currentAntenna.ToString();
            ScanState(value);
            SetVisibility("scanning");

            LogScan(characteristic);
        }

        private void Initialize(byte[] bytes)
        {
            InitializeComponent();

            BaseFrequency = Preferences.Get("BaseFrequency", 0) * 1000;
            Toolbar.SetData("Stationary Scanning", true, Back_Clicked);
            StationarySetting.SetData(bytes, BaseFrequency, false);
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (IsScanning)
            {
                bool result = await TransferBLEData.WriteStopScan(ValueCodes.STATIONARY);
                if (result)
                {
                    IsScanning = false;
                    SetVisibility("overview");
                    Clear();
                }
            }
            else
            {
                await Navigation.PushModalAsync(new StartScanningPage(), false);
            }
        }

        private async void StartStationaryScan_Clicked(object sender, EventArgs e)
        {
            /*bool result = await TransferBLEData.NotificationLog(ValueUpdateScan);
            if (result)
            {*/
                IsScanning = await SetStartScan();
                if (IsScanning)
                    SetVisibility("scanning");
            //}
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "overview":
                    CurrentSettings.IsVisible = true;
                    StationaryScanning.IsVisible = false;
                    Toolbar.SetData("Stationary Scanning");
                    Toolbar.ChangeButton(true);
                    break;
                case "scanning":
                    CurrentSettings.IsVisible = false;
                    StationaryScanning.IsVisible = true;
                    Toolbar.SetData("Stationary Scanning ...");
                    Toolbar.ChangeButton(false);
                    break;
            }
        }

        private void ValueUpdateScan(object o, CharacteristicUpdatedEventArgs args)
        {
            var value = args.Characteristic.Value;
            if (Converters.GetHexValue(value[0]).Equals("56"))
            {
                ReceiverInformation.GetInstance().ChangeSDCard(Converters.GetHexValue(value[1]).Equals("80"));
                ReceiverStatus.UpdateSDCardState();
            }
            else if (Converters.GetHexValue(value[0]).Equals("88"))
            {
                ReceiverInformation.GetInstance().ChangeDeviceBattery(value[1]);
                ReceiverStatus.UpdateBattery();
            }
            else
            {
                SetCurrentLog(args.Characteristic.Value);
            }
        }

        private void LogScan(ICharacteristic characteristic)
        {
            try
            {
                characteristic.ValueUpdated += ValueUpdateScan;
                characteristic.StartUpdatesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR LOG SCAN: " + e.Message);
            }
        }

        private async Task<bool> SetStartScan()
        {
            DateTime dateTime = DateTime.Now;

            byte[] b = new byte[] { 0x83, (byte) (dateTime.Year % 100), (byte) dateTime.Month, (byte) dateTime.Day,
                (byte) dateTime.Hour, (byte) dateTime.Minute, (byte) dateTime.Second, (byte) StationarySetting.firstTable,
                (byte) StationarySetting.secondTable, (byte) StationarySetting.thirdTable };

            bool result = await TransferBLEData.WriteStartScan(ValueCodes.STATIONARY, b);
            return result;
        }

        private void SetCurrentLog(byte[] value)
        {
            Console.WriteLine(Converters.GetHexValue(value));
            switch (Converters.GetHexValue(value[0]))
            {
                case "50":
                    ScanState(value);
                    break;
                case "F0":
                    LogScanHeader(value);
                    break;
                case "F1":
                case "F2": //Consolidated
                    LogScanCoded(value);
                    break;
                case "E1":
                case "E2":
                case "EA": //Non Coded
                    int signalStrength = value[4];
                    int period = (value[5] * 256) + value[6];
                    if (Converters.GetHexValue(DetectionType).Equals("08")) //Non Coded Fixed
                        LogScanNonCodedFixed(value[0], period, signalStrength);
                    if (Converters.GetHexValue(DetectionType).Equals("07")) // Non Coded Variable
                        Scanning.LogScanNonCodedVariable(period, signalStrength);
                    break;
            }
        }

        private void ScanState(byte[] value)
        {
            int maxIndex = (value[5] * 256) + value[6];
            MaxIndex.Text = "Table Index (" + maxIndex + " Total)";
            DetectionType = value[18];
            Scanning.UpdateVisibility(DetectionType);

            if (!Converters.GetHexValue(DetectionType).Equals("09"))
                Scanning.DetectionFilter = new ViewDetectionFilter(value);
        }

        private void LogScanHeader(byte[] value)
        {
            Clear();
            int frequency = (value[1] * 256) + value[2] + BaseFrequency;
            int index = (((value[1] >> 6) & 1) * 256) + value[3];
            NumberOfAntennas = value[1] >> 7;
            if (NumberOfAntennas == 0)
            {
                NumberOfAntennas = (value[7] >> 6) + 1;
                CurrentAntenna.Text = NumberOfAntennas.ToString();
            }
            else
            {
                CurrentAntenna.Text = "All";
            }
            TableIndex.Text = index.ToString();
            FrequencyScan.Text = Converters.GetFrequency(frequency);
        }

        private void LogScanCoded(byte[] value)
        {
            int code = value[3];
            int signalStrength = value[4];
            int mortality = value[5];
            Scanning.LogScanCoded(signalStrength, code, mortality);
        }

        private void LogScanNonCodedFixed(byte format, int period, int signalStrength)
        {
            int type = int.Parse(Converters.GetHexValue(format).Replace("E", ""));
            Scanning.LogScanNonCodedFixed(period, signalStrength, type);
        }

        public void Clear()
        {
            FrequencyScan.Text = "";
            TableIndex.Text = "";
            CurrentAntenna.Text = "";
            Scanning.Clear();
        }
    }
}
