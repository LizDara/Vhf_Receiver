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
    public partial class ManualScanningPage : ContentPage
    {
        private bool IsScanning;
        private int FrequencyNumber;
        private int BaseFrequency;
        private readonly int FrequencyRange;
        private byte DetectionType;

        public ManualScanningPage() //Coming from select scan
        {
            Initialize();

            IsScanning = false;
            _ = TransferBLEData.NotificationLog(ValueUpdateScan); // Log sd card state and battery
        }

        public ManualScanningPage(ICharacteristic characteristic, byte[] value) //Coming from connect ble
        {
            Initialize();

            IsScanning = true;
            GpsOption.SetGps((value[15] >> 7 & 1) == 1);
            GpsScan.SetGps(GpsScan.IsGpsOn());
            if (GpsOption.IsGpsOn()) SetGpsSearching(); else SetGpsOff();
            ScanState(value);
            SetVisibility("scanning");

            LogScan(characteristic);
        }

        private void Initialize()
        {
            InitializeComponent();

            int baseFrequency = Preferences.Get("BaseFrequency", 0);
            BaseFrequency = baseFrequency * 1000;
            int baseRange = Preferences.Get("Range", 0);
            Frequency.Text = BaseFrequency.ToString();
            FrequencyNumber = BaseFrequency;

            GpsOption.SetData(false);
            GpsScan.SetData(false);
            Toolbar.SetData("Manual Scanning", true, Back_Clicked);
            EnterFrequency.SetData(baseFrequency, baseRange, SaveFrequency);
            GpsRecordOptions.SetData(false);
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (IsScanning)
            {
                bool result = await TransferBLEData.WriteStopScan("");
                if (result)
                {
                    IsScanning = false;
                    SetVisibility("overview");
                    Scanning.Clear();
                    GpsScan.SetData(false);
                }
            }
            else
            {
                await Navigation.PushModalAsync(new StartScanningPage(), false);
            }
        }

        private void EnterNewFrequency_Clicked(object sender, EventArgs e)
        {
            SetVisibility("change");
            EnterFrequency.Initialize();
        }

        private void StartManualScan_Clicked(object sender, EventArgs e)
        {
            StartManualScan();
        }

        private void SaveFrequency_Clicked(object sender, EventArgs e)
        {
            FrequencyNumber = EnterFrequency.newFrequency;
            StartManualScan();
        }

        private async void TuneDown_Clicked(object sender, EventArgs e)
        {
            bool result = await TransferBLEData.WriteDecreaseIncrease(true);
            if (result)
            {
                FrequencyNumber = Converters.GetFrequencyNumber(FrequencyScan.Text) - 1;
                if (FrequencyNumber == BaseFrequency)
                {
                    TuneDown.Source = "TuneDownLight";
                    TuneDown.BorderColor = Color.FromRgba(203, 210, 217, 1);
                    TuneDown.IsEnabled = false;
                }
                else if (FrequencyNumber == FrequencyRange - 1)
                {
                    TuneUp.Source = "TuneUp";
                    TuneUp.BorderColor = Color.FromRgba(31, 41, 51, 1);
                    TuneUp.IsEnabled = true;
                }
            }
        }

        private async void TuneUp_Clicked(object sender, EventArgs e)
        {
            bool result = await TransferBLEData.WriteDecreaseIncrease(false);
            if (result)
            {
                FrequencyNumber = Converters.GetFrequencyNumber(FrequencyScan.Text) + 1;
                if (FrequencyNumber == BaseFrequency + 1)
                {
                    TuneDown.Source = "TuneDown";
                    TuneDown.BorderColor = Color.FromRgba(31, 41, 51, 1);
                    TuneDown.IsEnabled = true;
                }
                else if (FrequencyNumber == FrequencyRange)
                {
                    TuneUp.Source = "TuneUpLight";
                    TuneUp.BorderColor = Color.FromRgba(203, 210, 217, 1);
                    TuneUp.IsEnabled = false;
                }
            }
        }

        private void EditFrequency_Clicked(object sender, EventArgs e)
        {
            SetVisibility("change");
            EnterFrequency.Initialize();
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "overview":
                    CurrentFrequency.IsVisible = true;
                    EditFrequency.IsVisible = false;
                    ManualScanning.IsVisible = false;
                    Toolbar.SetData("Manual Scanning");
                    Toolbar.ChangeButton(true);
                    break;
                case "change":
                    CurrentFrequency.IsVisible = false;
                    EditFrequency.IsVisible = true;
                    ManualScanning.IsVisible = false;
                    Toolbar.SetData("Change Frequency");
                    Toolbar.ChangeButton(true);
                    break;
                case "scanning":
                    CurrentFrequency.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    ManualScanning.IsVisible = true;
                    Toolbar.SetData("Manual Scanning ...");
                    Toolbar.ChangeButton(false);
                    break;
            }
        }

        private async void StartManualScan()
        {
            /*bool result = await TransferBLEData.NotificationLog(ValueUpdateScan);
            if (result)
            {*/
                IsScanning = await SetStartScan();
                if (IsScanning)
                {
                    SetVisibility("scanning");
                    FrequencyScan.Text = Converters.GetFrequency(FrequencyNumber);
                    Frequency.Text = FrequencyNumber.ToString();
                    if (GpsOption.IsGpsOn()) SetGpsSearching(); else SetGpsOff();
                    GpsScan.SetGps(GpsOption.IsGpsOn());
                }
            //}
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
                SetCurrentLog(value);
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

            byte[] b = new byte[] { 0x86, (byte) (dateTime.Year % 100), (byte) dateTime.Month,
            (byte) dateTime.Day, (byte) dateTime.Hour, (byte) dateTime.Minute, (byte) dateTime.Second,
            (byte) ((FrequencyNumber - BaseFrequency) / 256), (byte) ((FrequencyNumber - BaseFrequency) % 256),
            (byte) (GpsOption.IsGpsOn() ? 0x80 : 0x0) };

            bool result = await TransferBLEData.WriteStartScan("", b);
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
                case "51":
                    GpsState(value);
                    break;
                case "A1":
                    LogGps(value);
                    break;
                case "F0":
                    LogScanHeader(value);
                    break;
                case "D0":
                    LogScanCoded(value);
                    break;
                case "E0":
                    int signalStrength = value[3];
                    int period = (value[4] * 256) + value[5];
                    if (Converters.GetHexValue(DetectionType).Equals("08")) //Non Coded Fixed
                        LogScanNonCodedFixed(value[0], period, signalStrength);
                    if (Converters.GetHexValue(DetectionType).Equals("07")) // Non Coded Variable
                        Scanning.LogScanNonCodedVariable(period, signalStrength);
                    break;
            }
        }

        private void ScanState(byte[] value)
        {
            DetectionType = value[18];
            int frequency = BaseFrequency + (value[10] * 256) + value[11];
            FrequencyScan.Text = Converters.GetFrequency(frequency);
            Frequency.Text = Converters.GetFrequency(frequency);
            IDAudio.IsVisible = Converters.GetHexValue(DetectionType).Equals("09");
            Scanning.UpdateVisibility(DetectionType);

            if (!Converters.GetHexValue(DetectionType).Equals("09"))
            {
                Scanning.DetectionFilter = new ViewDetectionFilter(value);
            }
            else
            {
                MessagingCenter.Subscribe<string>(this, ValueCodes.VALUE, (data) => {
                    IDAudio.SetData(data);
                });
            }
            GpsScan.SetData(true);
            MessagingCenter.Subscribe<string>(this, ValueCodes.GPS, (data) =>
            {
                GpsOption.SetGps(data.Equals("ON"));
                if (GpsScan.IsGpsOn()) SetGpsSearching(); else SetGpsOff();
            });
            MessagingCenter.Subscribe<string>(this, ValueCodes.AUTO_RECORD, (data) => {
                Scanning.Clear();
            });
        }

        private void GpsState(byte[] value)
        {
            int state = value[1];
            if (state == 3) SetGpsValid();
            else if (state == 2) SetGpsFailed();
            else if (state == 1) SetGpsSearching();
        }

        private void LogGps(byte[] value)
        {
            Coordinates.SetData(value);
        }

        private void LogScanHeader(byte[] value)
        {
            Scanning.Clear();
            int frequency = BaseFrequency + (value[1] * 256) + value[2];
            FrequencyScan.Text = Converters.GetFrequency(frequency);
            Frequency.Text = Converters.GetFrequency(frequency);
        }

        private void LogScanCoded(byte[] value)
        {
            int signalStrength = value[3];
            int code = value[4];
            int mortality = value[5];
            Scanning.LogScanCoded(signalStrength, code, mortality);
        }

        private void LogScanNonCodedFixed(byte format, int period, int signalStrength)
        {
            int type = int.Parse(Converters.GetHexValue(format).Replace("E", ""));
            Scanning.LogScanNonCodedFixed(period, signalStrength, type);
        }

        private void SetGpsOff()
        {
            GpsRecordOptions.SetGpsOff();
            Coordinates.IsVisible = false;
        }

        private void SetGpsSearching()
        {
            GpsRecordOptions.SetGpsSearching();
            Coordinates.IsVisible = false;
        }

        private void SetGpsFailed()
        {
            GpsRecordOptions.SetGpsFailed();
            Coordinates.IsVisible = false;
        }

        private void SetGpsValid()
        {
            GpsRecordOptions.SetGpsValid();
            Coordinates.IsVisible = true;
        }
    }
}
