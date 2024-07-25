using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using VhfReceiver.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class StationaryScanningPage : ContentPage
    {
        private ReceiverInformation ReceiverInformation;
        private byte[] StationaryBytes;
        private bool IsScanning;
        private int BaseFrequency;
        private int SelectedTableNumber;
        private int NumberOfAntennas;
        private int CodeNumber;
        private int DetectionsNumber;
        private int MortNumber;
        private byte DetectionType;

        public StationaryScanningPage(byte[] bytes)
        {
            Initialize();

            IsScanning = false;

            SetData(bytes);
        }

        public StationaryScanningPage(ICharacteristic characteristic, byte[] bytes, int currentFrequency, int currentIndex, int maxIndex, byte detetcionType)
        {
            Initialize();

            IsScanning = true;
            DetectionType = detetcionType;
            FrequencyScan.Text = (currentFrequency + BaseFrequency).ToString();
            TableIndex.Text = currentIndex.ToString();
            MaxIndex.Text = maxIndex.ToString();
            SetVisibility("scanning");
            UpdateVisibility();

            LogScan(characteristic);

            SetData(bytes);
        }

        public StationaryScanningPage(string tableNumber, string scanTime, string scanTimeout, string antennasNumber, string storeRate, byte[] bytes)
        {
            Initialize();

            IsScanning = false;

            SetData(tableNumber, scanTime, scanTimeout, antennasNumber, storeRate, bytes);
        }

        private void Initialize()
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();

            BaseFrequency = Preferences.Get("BaseFrequency", 0) * 1000;
        }

        private void SetData(byte[] bytes)
        {
            if (Converters.GetHexValue(bytes[0]).Equals("6C"))
            {
                SelectedTableNumber = bytes[1] / 16;
                if (SelectedTableNumber == 0)
                {
                    SelectedFrequencyTable.Text = "None";
                    StartStationaryScan.IsEnabled = false;
                    StartStationaryScan.Opacity = 0.6;
                }
                else
                {
                    SelectedFrequencyTable.Text = SelectedTableNumber.ToString();
                    StartStationaryScan.IsEnabled = true;
                    StartStationaryScan.Opacity = 1;
                }
                NumberOfAntennas = bytes[1] % 16;
                NumberAntennas.Text = (NumberOfAntennas == 0) ? "None" : NumberOfAntennas.ToString();
                ScanTime.Text = bytes[3].ToString();
                Timeout.Text = bytes[4].ToString();
                if (Converters.GetHexValue(bytes[5]).Equals("FF"))
                    StoreRate.Text = "Continuous Store";
                else
                    StoreRate.Text = bytes[5] == 0 ? "No Store Rate" : bytes[5].ToString();
                StationaryBytes = bytes;
            }
        }

        private void SetData(string tableNumber, string scanTime, string scanTimeout, string antennasNumber, string storeRate, byte[] bytes)
        {
            SelectedFrequencyTable.Text = tableNumber;
            StartStationaryScan.IsEnabled = true;
            StartStationaryScan.Opacity = 1;

            NumberAntennas.Text = antennasNumber;
            ScanTime.Text = scanTime;
            Timeout.Text = scanTimeout;
            StoreRate.Text = storeRate;

            StationaryBytes = bytes;
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (IsScanning)
            {
                bool result = await SetStopScan();
                if (result)
                {
                    SetVisibility("overview");
                    Clear();
                }
            }
            else
            {
                await Navigation.PushModalAsync(new StationarySettingsPage(), false);
            }
        }

        private async void EditSettings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new StationaryDefaultsPage(StationaryBytes), false);
        }

        private async void StartStationaryScan_Clicked(object sender, EventArgs e)
        {
            bool result = await LogScan();
            if (result)
            {
                result = await SetStartScan();
                if (result)
                    SetVisibility("scanning");
            }
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "overview":
                    CurrentSettings.IsVisible = true;
                    StationaryScanning.IsVisible = false;
                    TitleToolbar.Text = "Stationary Scanning";
                    Back.Source = "Back";
                    break;
                case "scanning":
                    CurrentSettings.IsVisible = true;
                    StationaryScanning.IsVisible = false;
                    TitleToolbar.Text = "Stationary Scanning ...";
                    Back.Source = "Exit";
                    break;
            }
        }

        private void UpdateVisibility()
        {
            bool visibility = Converters.GetHexValue(DetectionType).Equals("09");
            //AudioOptions.IsVisible = visibility;
            Code.IsVisible = visibility;
            Mortality.IsVisible = visibility;
            Period.IsVisible = !visibility;
            PulseRate.IsVisible = !visibility;
        }

        private async Task<bool> LogScan()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCREEN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SEND_LOG);
                    if (characteristic != null)
                    {
                        characteristic.ValueUpdated += (o, args) =>
                        {
                            SetCurrentLog(args.Characteristic.Value);
                        };
                        await characteristic.StartUpdatesAsync();

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return false;
        }

        private void LogScan(ICharacteristic characteristic)
        {
            try
            {
                characteristic.ValueUpdated += (o, args) =>
                {
                    SetCurrentLog(args.Characteristic.Value);
                };
                characteristic.StartUpdatesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
        }

        private async Task<bool> SetStartScan()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY);
                    if (characteristic != null)
                    {
                        DateTime dateTime = DateTime.Now;

                        byte[] b = new byte[] { 0x83, (byte) (dateTime.Year % 100), (byte) dateTime.Month,
                        (byte) dateTime.Day, (byte) dateTime.Hour, (byte) dateTime.Minute,
                        (byte) dateTime.Second, (byte) SelectedTableNumber };

                        bool result = await characteristic.WriteAsync(b);
                        if (result)
                            IsScanning = true;
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

        private async Task<bool> SetStopScan()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY);
                    if (characteristic != null)
                    {
                        bool result = await characteristic.WriteAsync(new byte[] { 0x87 });
                        if (result)
                            IsScanning = false;
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

        private void SetCurrentLog(byte[] value)
        {
            switch (Converters.GetHexValue(value[0]))
            {
                case "50":
                    int maxIndex = (value[5] * 256) + value[6];
                    MaxIndex.Text = "Table Index (" + maxIndex + " Total)";
                    DetectionType = value[18];
                    UpdateVisibility();
                    break;
                case "F0":
                    LogScanHeader(value);
                    break;
                case "F1":
                    LogScanFix(value);
                    break;
                case "F2":
                    LogScanFixConsolidated(value);
                    break;
                default: //E1 and E2
                    LogScanData(value);
                    break;
            }
        }

        private void LogScanHeader(byte[] value)
        {
            Clear();
            int frequency = (value[1] * 256) + value[2] + BaseFrequency;
            FrequencyScan.Text = GetFrequency(frequency);
            TableIndex.Text = value[3].ToString();
            CurrentAntenna.Text = (NumberOfAntennas == 0) ? "All" : NumberOfAntennas.ToString();
        }

        private void LogScanFix(byte[] value)
        {
            int position;
            CodeNumber = value[3];
            int signalStrength = value[4];
            DetectionsNumber = value[7];
            int mort = value[5];

            if (ScanDetails.Children.Count > 2 && IsEqualFirstCode(CodeNumber))
            {
                RefreshFirstCode(signalStrength, mort > 0);
            }
            else if ((position = PositionCode(CodeNumber)) != 0)
            {
                RefreshPosition(position, signalStrength, mort > 0);
            }
            else
            {
                DetectionsNumber = 1;
                CreateCodeDetail(CodeNumber, signalStrength, DetectionsNumber, mort > 0);
            }
        }

        private void LogScanFixConsolidated(byte[] value)
        {
            int position;
            CodeNumber = value[3];
            int signalStrength = value[4];
            DetectionsNumber = value[7];
            int mort = value[5];

            if (ScanDetails.Children.Count > 2 && IsEqualFirstCode(CodeNumber))
            {
                RefreshFirstCode(signalStrength, mort > 0);
            }
            else if ((position = PositionCode(CodeNumber)) != 0)
            {
                RefreshPosition(position, signalStrength, mort > 0);
            }
            else
            {
                DetectionsNumber = 1;
                CreateCodeDetail(CodeNumber, signalStrength, DetectionsNumber, mort > 0);
            }
        }

        private void LogScanData(byte[] value)
        {
            int period = (value[4] * 256) + value[5];
            double pulseRate = (double)60000 / period;
            int signalStrength = value[4];

            RefreshNonCoded(period, pulseRate, signalStrength, DetectionsNumber);
        }

        private bool IsEqualFirstCode(int codeNumber)
        {
            StackLayout stackLayout = (StackLayout)ScanDetails.Children[2];
            Label codeLabel = (Label)stackLayout.Children[0];

            return int.Parse(codeLabel.Text) == codeNumber;
        }

        private void RefreshFirstCode(int signalStrength, bool isMort)
        {
            StackLayout stackLayout = (StackLayout)ScanDetails.Children[2];
            Label detectionsLabel = (Label)stackLayout.Children[1];
            Label mortalityLabel = (Label)stackLayout.Children[2];
            Label signalStrengthLabel = (Label)stackLayout.Children[3];
            Label mortLabel = (Label)stackLayout.Children[4];

            mortalityLabel.Text = isMort ? "M" : "-";
            signalStrengthLabel.Text = signalStrength.ToString();
            detectionsLabel.Text = (int.Parse(detectionsLabel.Text) + 1).ToString();
            if (isMort) mortLabel.Text = (int.Parse(mortLabel.Text) + 1).ToString();
            DetectionsNumber = int.Parse(detectionsLabel.Text);
            MortNumber = int.Parse(mortLabel.Text);
        }

        private int PositionCode(int codeNumber)
        {
            int position = 0;
            for (int i = 4; i < ScanDetails.Children.Count - 1; i += 2)
            {
                StackLayout stackLayout = (StackLayout)ScanDetails.Children[i];
                Label label = (Label)stackLayout.Children[0];

                if (int.Parse(label.Text) == codeNumber) position = i;
            }
            return position;
        }

        private void RefreshPosition(int position, int signalStrength, bool isMort)
        {
            StackLayout stackLayout = (StackLayout)ScanDetails.Children[position];
            Label codeLabel = (Label)stackLayout.Children[0];
            Label detectionsLabel = (Label)stackLayout.Children[1];
            Label mortLabel = (Label)stackLayout.Children[4];

            int code = int.Parse(codeLabel.Text);
            int detections = int.Parse(detectionsLabel.Text);
            int mort = int.Parse(mortLabel.Text);

            RefreshCode(position, code, signalStrength, detections + 1, isMort, mort);
        }

        private void RefreshCode(int finalPosition, int code, int signalStrength, int detections, bool isMort, int mort)
        {
            for (int i = finalPosition; i > 3; i -= 2)
            {
                StackLayout lastStackLayout = (StackLayout)ScanDetails.Children[i];
                StackLayout penultimateStackLayout = (StackLayout)ScanDetails.Children[i - 2];

                Label lastCodeLabel = (Label)lastStackLayout.Children[0];
                Label penultimateCodeLabel = (Label)penultimateStackLayout.Children[0];
                lastCodeLabel.Text = penultimateCodeLabel.Text;

                Label lastDetectionsLabel = (Label)lastStackLayout.Children[1];
                Label penultimateDetectionsLabel = (Label)penultimateStackLayout.Children[1];
                lastDetectionsLabel.Text = penultimateDetectionsLabel.Text;

                Label lastMortalityLabel = (Label)lastStackLayout.Children[2];
                Label penultimateMortalityLabel = (Label)penultimateStackLayout.Children[2];
                lastMortalityLabel.Text = penultimateMortalityLabel.Text;

                Label lastSignalStrengthLabel = (Label)lastStackLayout.Children[3];
                Label penultimateSignalStrengthLabel = (Label)penultimateStackLayout.Children[3];
                lastSignalStrengthLabel.Text = penultimateSignalStrengthLabel.Text;

                Label lastMortLabel = (Label)lastStackLayout.Children[4];
                Label penultimateMortLabel = (Label)penultimateStackLayout.Children[4];
                lastMortLabel.Text = penultimateMortLabel.Text;
            }

            StackLayout stackLayout = (StackLayout)ScanDetails.Children[2];
            Label newCodeLabel = (Label)stackLayout.Children[0];
            Label newDetectionsLabel = (Label)stackLayout.Children[1];
            Label newMortalityLabel = (Label)stackLayout.Children[2];
            Label newSignalStrengthLabel = (Label)stackLayout.Children[3];
            Label newMortLabel = (Label)stackLayout.Children[4];

            newCodeLabel.Text = code.ToString();
            newDetectionsLabel.Text = detections.ToString();
            newMortalityLabel.Text = isMort ? "M" : "-";
            newSignalStrengthLabel.Text = signalStrength.ToString();
            newMortLabel.Text = isMort ? (mort + 1).ToString() : mort.ToString();

            DetectionsNumber = detections;
            MortNumber = int.Parse(newMortLabel.Text);
        }

        private void CreateCodeDetail(int codeNumber, int signalStrength, int detectionsNumber, bool isMort)
        {
            StackLayout newStackLayout = new StackLayout();
            newStackLayout.Orientation = StackOrientation.Horizontal;
            newStackLayout.Padding = new Thickness(0, 8, 0, 0);

            Label codeLabel = new Label();
            codeLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            codeLabel.HorizontalTextAlignment = TextAlignment.Center;
            codeLabel.TextColor = Color.FromHex("1F2933");
            codeLabel.FontSize = 16;

            Label detectionsLabel = new Label();
            detectionsLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            detectionsLabel.HorizontalTextAlignment = TextAlignment.Center;
            detectionsLabel.TextColor = Color.FromHex("1F2933");
            detectionsLabel.FontSize = 16;

            Label mortalityLabel = new Label();
            mortalityLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            mortalityLabel.HorizontalTextAlignment = TextAlignment.Center;
            mortalityLabel.TextColor = Color.FromHex("1F2933");
            mortalityLabel.FontSize = 16;

            Label signalStrengthLabel = new Label();
            signalStrengthLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            signalStrengthLabel.HorizontalTextAlignment = TextAlignment.Center;
            signalStrengthLabel.TextColor = Color.FromHex("1F2933");
            signalStrengthLabel.FontSize = 16;

            Label mortLabel = new Label();
            mortLabel.IsVisible = false;

            newStackLayout.Children.Add(codeLabel);
            newStackLayout.Children.Add(detectionsLabel);
            newStackLayout.Children.Add(mortalityLabel);
            newStackLayout.Children.Add(signalStrengthLabel);
            newStackLayout.Children.Add(mortLabel);

            StackLayout line = new StackLayout();
            line.BackgroundColor = Color.FromHex("E5E5E5");
            line.HorizontalOptions = LayoutOptions.Fill;
            line.HeightRequest = 1;

            ScanDetails.Children.Add(newStackLayout);
            ScanDetails.Children.Add(line);

            RefreshCode(ScanDetails.Children.Count - 2, codeNumber, signalStrength, detectionsNumber, isMort, 0);
        }

        private void RefreshNonCoded(int period, double pulseRate, int signalStrength, int detectionsNumber)
        {
            StackLayout newStackLayout = new StackLayout();
            newStackLayout.Orientation = StackOrientation.Horizontal;
            newStackLayout.Padding = new Thickness(0, 8, 0, 8);

            Label periodLabel = new Label();
            periodLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            periodLabel.HorizontalTextAlignment = TextAlignment.Center;
            periodLabel.TextColor = Color.FromHex("1F2933");
            periodLabel.FontSize = 16;

            Label detectionsLabel = new Label();
            detectionsLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            detectionsLabel.HorizontalTextAlignment = TextAlignment.Center;
            detectionsLabel.TextColor = Color.FromHex("1F2933");
            detectionsLabel.FontSize = 16;

            Label pulseRateLabel = new Label();
            pulseRateLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            pulseRateLabel.HorizontalTextAlignment = TextAlignment.Center;
            pulseRateLabel.TextColor = Color.FromHex("1F2933");
            pulseRateLabel.FontSize = 16;

            Label signalStrengthLabel = new Label();
            signalStrengthLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            signalStrengthLabel.HorizontalTextAlignment = TextAlignment.Center;
            signalStrengthLabel.TextColor = Color.FromHex("1F2933");
            signalStrengthLabel.FontSize = 16;

            newStackLayout.Children.Add(periodLabel);
            newStackLayout.Children.Add(detectionsLabel);
            newStackLayout.Children.Add(pulseRateLabel);
            newStackLayout.Children.Add(signalStrengthLabel);

            StackLayout line = new StackLayout();
            line.BackgroundColor = Color.FromHex("E5E5E5");
            line.HorizontalOptions = LayoutOptions.Fill;
            line.HeightRequest = 1;

            ScanDetails.Children.Add(newStackLayout);
            ScanDetails.Children.Add(line);

            for (int i = ScanDetails.Children.Count - 2; i > 3; i -= 2)
            {
                StackLayout lastStackLayout = (StackLayout)ScanDetails.Children[i];
                StackLayout penultimateStackLayout = (StackLayout)ScanDetails.Children[i - 2];

                Label lastPeriodLabel = (Label)lastStackLayout.Children[0];
                Label penultimatePeriodLabel = (Label)penultimateStackLayout.Children[0];
                lastPeriodLabel.Text = penultimatePeriodLabel.Text;

                Label lastDetectionsLabel = (Label)lastStackLayout.Children[1];
                Label penultimateDetectionsLabel = (Label)penultimateStackLayout.Children[1];
                lastDetectionsLabel.Text = penultimateDetectionsLabel.Text;

                Label lastPulseRateLabel = (Label)lastStackLayout.Children[2];
                Label penultimatePulseRateLabel = (Label)penultimateStackLayout.Children[2];
                lastPulseRateLabel.Text = penultimatePulseRateLabel.Text;

                Label lastSignalStrengthLabel = (Label)lastStackLayout.Children[3];
                Label penultimateSignalStrengthLabel = (Label)penultimateStackLayout.Children[3];
                lastSignalStrengthLabel.Text = penultimateSignalStrengthLabel.Text;
            }

            StackLayout stackLayout = (StackLayout)ScanDetails.Children[2];
            Label newPeriodLabel = (Label)stackLayout.Children[0];
            Label newDetectionsLabel = (Label)stackLayout.Children[1];
            Label newPulseRateLabel = (Label)stackLayout.Children[2];
            Label newSignalStrengthLabel = (Label)stackLayout.Children[3];

            newPeriodLabel.Text = period.ToString();
            newDetectionsLabel.Text = detectionsNumber.ToString();
            newPulseRateLabel.Text = string.Format("{0:f2}", pulseRate);
            newSignalStrengthLabel.Text = signalStrength.ToString();
        }

        private void Clear()
        {
            FrequencyScan.Text = "";
            TableIndex.Text = "";
            int count = ScanDetails.Children.Count;
            while (count > 2)
            {
                ScanDetails.Children.RemoveAt(2);
                count--;
            }
        }

        private string GetFrequency(int frequency)
        {
            return frequency.ToString().Substring(0, 3) + "." + frequency.ToString().Substring(3);
        }
    }
}
