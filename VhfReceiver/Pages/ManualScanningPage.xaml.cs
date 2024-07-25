using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class ManualScanningPage : ContentPage
    {
        private ReceiverInformation ReceiverInformation;
        private bool IsScanning;
        private int FrequencyNumber;
        private int BaseFrequency;
        private int FrequencyRange;
        private byte DetectionType;
        private int CodeNumber;
        private int DetectionsNumber;
        private int MortNumber;

        public ManualScanningPage()
        {
            Initialize();

            IsScanning = false;
        }

        public ManualScanningPage(ICharacteristic characteristic, byte detectionType)
        {
            Initialize();

            IsScanning = true;
            DetectionType = detectionType;
            UpdateVisibility();
            SetVisibility("scanning");

            LogScan(characteristic);
        }

        private void Initialize()
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();

            int baseFrequency = Preferences.Get("BaseFrequency", 0);
            BaseFrequency = baseFrequency * 1000;
            int baseRange = Preferences.Get("Range", 0);
            FrequencyRange = ((baseFrequency + baseRange) * 1000) - 1;
            string message = "Frequency range is " + BaseFrequency + " to " + FrequencyRange;
            FrequencyBaseRange.Text = message;
            Frequency.Text = GetFrequency(BaseFrequency);
            FrequencyNumber = BaseFrequency;

            CreateNumberButtons(baseRange);
        }

        private void CreateNumberButtons(int baseRange)
        {
            int baseNumber = BaseFrequency / 1000;
            ButtonsNumber.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonsNumber.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonsNumber.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonsNumber.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < baseRange / 4; i++)
            {
                ButtonsNumber.RowDefinitions.Add(new RowDefinition());
                for (int j = 0; j < 4; j++)
                {
                    Button button = WidgetCreation.NewBaseButton(baseNumber);
                    int finalBaseNumber = baseNumber;
                    button.Clicked += (object sender, EventArgs e) =>
                    {
                        if (NewFrequency.Text.Equals("") || NewFrequency.Text.Length > 6)
                        {
                            NewFrequency.Text = finalBaseNumber.ToString();
                            NewFrequency.TextColor = Color.FromRgb(31, 41, 51);
                        }
                    };
                    ButtonsNumber.Children.Add(button, j, i);
                    baseNumber++;
                }
            }
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
                var bytes = await GetTables();
                if (bytes != null)
                    await Navigation.PushModalAsync(new StartScanningPage(bytes), false);
            }
        }

        private void EnterNewFrequency_Clicked(object sender, EventArgs e)
        {
            SetVisibility("change");
            InitChangeFrequency();
        }

        private async void StartManualScan_Clicked(object sender, EventArgs e)
        {
            bool result = await LogScan();
            if (result)
            {
                IsScanning = await SetStartScan();
                if (result)
                {
                    SetVisibility("scanning");
                    SetGpsOff("Searching ...");
                    State.IsVisible = GPS.IsToggled;
                }
            }
        }

        private void TextListener_Changed(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (NewFrequency.Text.Length == 6)
            {
                int frequency = int.Parse(NewFrequency.Text);
                if (frequency >= BaseFrequency && frequency <= FrequencyRange)
                {
                    ChangeFrequency.IsEnabled = true;
                    ChangeFrequency.Opacity = 1;
                    Line.BackgroundColor = Color.FromHex("#CBD2D9");
                    FrequencyBaseRange.TextColor = Color.FromRgb(123, 135, 148);
                }
            }
            else
            {
                ChangeFrequency.IsEnabled = false;
                ChangeFrequency.Opacity = 0.6;
                Line.BackgroundColor = Color.FromHex("#BA2525");
                FrequencyBaseRange.TextColor = Color.FromRgb(186, 37, 37);
            }
        }

        private void Number_Clicked(object sender, EventArgs e)
        {
            if (NewFrequency.Text.Length >= 3 && NewFrequency.Text.Length < 6)
            {
                Button button = (Button)sender;
                string text = NewFrequency.Text;
                NewFrequency.Text = text + button.Text;
            }
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            if (!NewFrequency.Text.Equals(""))
            {
                string previous = NewFrequency.Text;
                NewFrequency.Text = previous.Substring(0, previous.Length - 1);
            }
        }

        private async void ChangeFrequency_Clicked(object sender, EventArgs e)
        {
            FrequencyNumber = int.Parse(NewFrequency.Text);
            Frequency.Text = GetFrequency(FrequencyNumber);

            bool result = await LogScan();
            if (result)
            {
                result = await SetStartScan();
                if (result)
                    SetVisibility("scanning");
            }
        }

        private async void RecordData_Clicked(object sender, EventArgs e)
        {
            State.IsVisible = true;
            State.Text = "Saving targets ...";
            RecordData.IsEnabled = false;
            RecordData.Opacity = 0.6;

            bool result = await SetRecordData();
            if (result)
            {
                State.IsVisible = false;
                RecordData.IsEnabled = true;
                RecordData.Opacity = 1;
                Clear();
            }
        }

        private async void TuneDown_Clicked(object sender, EventArgs e)
        {
            bool result = await SetFrequency(true);
            if (result)
            {
                FrequencyNumber = GetFrequencyNumber(FrequencyScan.Text) - 1;
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
            bool result = await SetFrequency(false);
            if (result)
            {
                FrequencyNumber = GetFrequencyNumber(FrequencyScan.Text) + 1;
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

        private async void EditFrequency_Clicked(object sender, EventArgs e)
        {
            bool result = await SetStopScan();
            if (result)
            {
                SetVisibility("change");
                InitChangeFrequency();
            }
        }

        private async void EditAudio_Tapped(object sender, EventArgs e)
        {
            var popMessage = new AudioOptions();
            await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "overview":
                    CurrentFrequency.IsVisible = true;
                    EnterFrequency.IsVisible = false;
                    ManualScanning.IsVisible = false;
                    TitleToolbar.Text = "Manual Scanning";
                    Back.Source = "Back";
                    break;
                case "change":
                    CurrentFrequency.IsVisible = false;
                    EnterFrequency.IsVisible = true;
                    ManualScanning.IsVisible = false;
                    TitleToolbar.Text = "Change Frequency";
                    Back.Source = "Back";
                    break;
                case "scanning":
                    CurrentFrequency.IsVisible = false;
                    EnterFrequency.IsVisible = false;
                    ManualScanning.IsVisible = true;
                    TitleToolbar.Text = "Manual Scanning ...";
                    Back.Source = "Exit";
                    break;
            }
        }

        private void UpdateVisibility()
        {
            bool visibility = Converters.GetHexValue(DetectionType).Equals("09");
            AudioOptions.IsVisible = visibility;
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
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_MANUAL);
                    if (characteristic != null)
                    {
                        DateTime dateTime = DateTime.Now;

                        byte[] b = new byte[] { 0x86, (byte) (dateTime.Year % 100), (byte) dateTime.Month,
                        (byte) dateTime.Day, (byte) dateTime.Hour, (byte) dateTime.Minute, (byte) dateTime.Second,
                        (byte) ((FrequencyNumber - BaseFrequency) / 256), (byte) ((FrequencyNumber - BaseFrequency) % 256),
                        (byte) (GPS.IsToggled ? 0x80 : 0x0) };

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

        private async Task<bool> SetStopScan()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_MANUAL);
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

        private async Task<bool> SetRecordData()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_MANUAL);
                    if (characteristic != null)
                    {
                        byte[] b = new byte[] { 0x8C };

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

        private async Task<bool> SetFrequency(bool isDown)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SCAN_TABLE);
                    if (characteristic != null)
                    {
                        byte[] b = new byte[1];
                        if (isDown)
                            b[0] = 0x5E;
                        else
                            b[0] = 0x5F;

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

        private void InitChangeFrequency()
        {
            NewFrequency.Text = "Enter Frequency Digits";
            NewFrequency.TextColor = Color.FromRgb(123, 135, 148);
            Line.BackgroundColor = Color.FromHex("#CBD2D9");
            FrequencyBaseRange.TextColor = Color.FromRgb(123, 135, 148);
            ChangeFrequency.Opacity = 0.6;
            ChangeFrequency.IsEnabled = false;
        }

        private void SetCurrentLog(byte[] value)
        {
            Console.WriteLine(Converters.GetHexValue(value));
            if (Converters.GetHexValue(value[0]).Equals("50"))
            {
                ScanState(value);
            }
            else if (Converters.GetHexValue(value[0]).Equals("51"))
            {
                GpsState(value);
            }
            else if (Converters.GetHexValue(value[0]).Equals("F0"))
            {
                Clear();
                int frequency = BaseFrequency + (value[1] * 256) + value[2];
                FrequencyScan.Text = GetFrequency(frequency);
                Frequency.Text = GetFrequency(frequency);
            }
            else if (Converters.GetHexValue(value[0]).Equals("D0") || Converters.GetHexValue(value[0]).Equals("E0"))
            {
                int signalStrength = value[3];

                if (Converters.GetHexValue(DetectionType).Equals("09")) // Coded
                {
                    CodeNumber = value[4];
                    int mortality = value[5];
                    int position = GetPositionNumber(CodeNumber, 0);
                    if (position > 0)
                    {
                        RefreshCodedPosition(position, signalStrength, mortality > 0);
                    }
                    else if (position < 0)
                    {
                        DetectionsNumber = 1;
                        CreateDetail();
                        AddNewCodedDetailInPosition(-position, CodeNumber, signalStrength, DetectionsNumber, mortality > 0);
                    }
                    else
                    {
                        DetectionsNumber = 1;
                        CreateDetail();
                        AddNewCodedDetail(ScanDetails.Children.Count - 2, CodeNumber, signalStrength, DetectionsNumber, mortality > 0);
                    }
                }
                else if (Converters.GetHexValue(DetectionType).Equals("08")) // Non Coded Fixed
                {
                    int period = (value[4] * 256) + value[5];
                    int pulseRate = 60000 / period;
                    int type = int.Parse(Converters.GetHexValue(value[0]).Replace("E", ""));
                    int position = GetPositionNumber(type, 4);
                    if (position > 0)
                    {
                        RefreshNonCodedPosition(position, signalStrength, period, pulseRate);
                    }
                    else if (position < 0)
                    {
                        DetectionsNumber = 1;
                        CreateDetail();
                        AddNewNonCodedDetailInPosition(-position, pulseRate, signalStrength, DetectionsNumber, period, type);
                    }
                    else
                    {
                        DetectionsNumber = 1;
                        CreateDetail();
                        AddNewNonCodedFixedDetail(ScanDetails.Children.Count - 2, pulseRate, signalStrength, DetectionsNumber, period, type);
                    }
                }
                else if (Converters.GetHexValue(DetectionType).Equals("07")) // Non Coded Variablex
                {
                    int period = (value[4] * 256) + value[5];
                    int pulseRate = 60000 / period;
                    RefreshNonCodedVariable(period, pulseRate, signalStrength);
                }
            }
        }

        private void ScanState(byte[] value)
        {
            DetectionType = value[18];
            int frequency = BaseFrequency + (value[10] * 256) + value[11];
            FrequencyScan.Text = GetFrequency(frequency);
            Frequency.Text = GetFrequency(frequency);
            UpdateVisibility();
        }

        private void GpsState(byte[] value)
        {
            int state = value[1];
            if (state == 3) SetGpsOn();
            else if (state == 2) SetGpsOff("Failed");
            else if (state == 1) SetGpsOff("Searching ...");
        }

        private int GetPositionNumber(int number, int position)
        {
            for (int i = 2; i < ScanDetails.Children.Count - 1; i += 2)
            {
                Grid grid = (Grid)ScanDetails.Children[i];
                Label label = (Label)grid.Children[position];

                if (int.Parse(label.Text) == number) return i;
                else if (number < int.Parse(label.Text)) return -i;
            }
            return 0;
        }

        private void RefreshCodedPosition(int position, int signalStrength, bool isMort)
        {
            Grid grid = (Grid)ScanDetails.Children[position];
            Label detectionsLabel = (Label)grid.Children[1];
            Label mortalityLabel = (Label)grid.Children[2];
            Label signalStrengthLabel = (Label)grid.Children[3];
            Label mortLabel = (Label)grid.Children[4];

            DetectionsNumber = int.Parse(detectionsLabel.Text) + 1;
            MortNumber = isMort ? int.Parse(mortLabel.Text) + 1 : int.Parse(mortLabel.Text);
            detectionsLabel.Text = DetectionsNumber.ToString();
            mortalityLabel.Text = isMort ? "M" : "-";
            signalStrengthLabel.Text = signalStrength.ToString();
            mortLabel.Text = MortNumber.ToString();
        }

        private void RefreshNonCodedPosition(int position, int signalStrength, int period, int pulseRate)
        {
            Grid grid = (Grid)ScanDetails.Children[position];
            Label periodLabel = (Label)grid.Children[0];
            Label detectionsLabel = (Label)grid.Children[1];
            Label pulseRateLabel = (Label)grid.Children[2];
            Label signalStrengthLabel = (Label)grid.Children[3];

            DetectionsNumber = int.Parse(detectionsLabel.Text) + 1;
            periodLabel.Text = period.ToString();
            detectionsLabel.Text = DetectionsNumber.ToString();
            pulseRateLabel.Text = pulseRate.ToString();
            signalStrengthLabel.Text = signalStrength.ToString();
        }

        private void AddNewCodedDetail(int position, int code, int signalStrength, int detections, bool isMort)
        {
            Grid grid = (Grid)ScanDetails.Children[position];
            Label newCodeLabel = (Label)grid.Children[0];
            Label newDetectionsLabel = (Label)grid.Children[1];
            Label newMortalityLabel = (Label)grid.Children[2];
            Label newSignalStrengthLabel = (Label)grid.Children[3];
            Label newMortLabel = (Label)grid.Children[4];

            newCodeLabel.Text = code.ToString();
            newDetectionsLabel.Text = detections.ToString();
            newMortalityLabel.Text = isMort ? "M" : "-";
            newSignalStrengthLabel.Text = signalStrength.ToString();
            newMortLabel.Text = isMort ? "1" : "0";
            MortNumber = isMort ? 1 : 0;
        }

        private void AddNewNonCodedFixedDetail(int position, int pulseRate, int signalStrength, int detections, int period, int type)
        {
            Grid grid = (Grid)ScanDetails.Children[position];
            Label newPeriodLabel = (Label)grid.Children[0];
            Label newDetectionsLabel = (Label)grid.Children[1];
            Label newPulseRateLabel = (Label)grid.Children[2];
            Label newSignalStrengthLabel = (Label)grid.Children[3];
            Label newTypeLabel = (Label)grid.Children[4];

            newPeriodLabel.Text = period.ToString();
            newDetectionsLabel.Text = detections.ToString();
            newPulseRateLabel.Text = pulseRate.ToString();
            newSignalStrengthLabel.Text = signalStrength.ToString();
            newTypeLabel.Text = type.ToString();
        }

        private void AddNewNonCodedVariableDetail(int position, int pulseRate, int signalStrength, int period)
        {
            Grid grid = (Grid)ScanDetails.Children[position];
            Label newPeriodLabel = (Label)grid.Children[0];
            Label newDetectionsLabel = (Label)grid.Children[1];
            Label newPulseRateLabel = (Label)grid.Children[2];
            Label newSignalStrengthLabel = (Label)grid.Children[3];

            newPeriodLabel.Text = period.ToString();
            newDetectionsLabel.Text = "-";
            newPulseRateLabel.Text = pulseRate.ToString();
            newSignalStrengthLabel.Text = signalStrength.ToString();
        }

        private void AddNewCodedDetailInPosition(int position, int code, int signalStrength, int detections, bool isMort)
        {
            for (int i = ScanDetails.Children.Count - 2; i > position; i -= 2)
            {
                Grid lastGrid = (Grid)ScanDetails.Children[i];
                Grid penultimateGrid = (Grid)ScanDetails.Children[i - 2];

                Label lastCodeLabel = (Label)lastGrid.Children[0];
                Label penultimateCodeLabel = (Label)penultimateGrid.Children[0];
                lastCodeLabel.Text = penultimateCodeLabel.Text;

                Label lastDetectionsLabel = (Label)lastGrid.Children[1];
                Label penultimateDetectionsLabel = (Label)penultimateGrid.Children[1];
                lastDetectionsLabel.Text = penultimateDetectionsLabel.Text;

                Label lastMortalityLabel = (Label)lastGrid.Children[2];
                Label penultimateMortalityLabel = (Label)penultimateGrid.Children[2];
                lastMortalityLabel.Text = penultimateMortalityLabel.Text;

                Label lastSignalStrengthLabel = (Label)lastGrid.Children[3];
                Label penultimateSignalStrengthLabel = (Label)penultimateGrid.Children[3];
                lastSignalStrengthLabel.Text = penultimateSignalStrengthLabel.Text;

                Label lastMortLabel = (Label)lastGrid.Children[4];
                Label penultimateMortLabel = (Label)penultimateGrid.Children[4];
                lastMortLabel.Text = penultimateMortLabel.Text;
            }
            AddNewCodedDetail(position, code, signalStrength, detections, isMort);
        }

        private void AddNewNonCodedDetailInPosition(int position, int pulseRate, int signalStrength, int detections, int period, int type)
        {
            for (int i = ScanDetails.Children.Count - 2; i > position; i -= 2)
            {
                Grid lastGrid = (Grid)ScanDetails.Children[i];
                Grid penultimateGrid = (Grid)ScanDetails.Children[i - 2];

                Label lastPeriodLabel = (Label)lastGrid.Children[0];
                Label penultimatePeriodLabel = (Label)penultimateGrid.Children[0];
                lastPeriodLabel.Text = penultimatePeriodLabel.Text;

                Label lastDetectionsLabel = (Label)lastGrid.Children[1];
                Label penultimateDetectionsLabel = (Label)penultimateGrid.Children[1];
                lastDetectionsLabel.Text = penultimateDetectionsLabel.Text;

                Label lastPulseRateLabel = (Label)lastGrid.Children[2];
                Label penultimatePulseRateLabel = (Label)penultimateGrid.Children[2];
                lastPulseRateLabel.Text = penultimatePulseRateLabel.Text;

                Label lastSignalStrengthLabel = (Label)lastGrid.Children[3];
                Label penultimateSignalStrengthLabel = (Label)penultimateGrid.Children[3];
                lastSignalStrengthLabel.Text = penultimateSignalStrengthLabel.Text;

                Label lastTypeLabel = (Label)lastGrid.Children[4];
                Label penultimateTypeLabel = (Label)penultimateGrid.Children[4];
                lastTypeLabel.Text = penultimateTypeLabel.Text;
            }
            AddNewNonCodedFixedDetail(position, pulseRate, signalStrength, detections, period, type);
        }

        private void RefreshNonCodedVariable(int period, int pulseRate, int signalStrength)
        {
            CreateDetail();
            for (int i = ScanDetails.Children.Count - 2; i > 3; i -= 2)
            {
                Grid lastGrid = (Grid)ScanDetails.Children[i];
                Grid penultimateGrid = (Grid)ScanDetails.Children[i - 2];

                Label lastPeriodLabel = (Label)lastGrid.Children[0];
                Label penultimatePeriodLabel = (Label)penultimateGrid.Children[0];
                lastPeriodLabel.Text = penultimatePeriodLabel.Text;

                Label lastDetectionsLabel = (Label)lastGrid.Children[1];
                Label penultimateDetectionsLabel = (Label)penultimateGrid.Children[1];
                lastDetectionsLabel.Text = penultimateDetectionsLabel.Text;

                Label lastPulseRateLabel = (Label)lastGrid.Children[2];
                Label penultimatePulseRateLabel = (Label)penultimateGrid.Children[2];
                lastPulseRateLabel.Text = penultimatePulseRateLabel.Text;

                Label lastSignalStrengthLabel = (Label)lastGrid.Children[3];
                Label penultimateSignalStrengthLabel = (Label)penultimateGrid.Children[3];
                lastSignalStrengthLabel.Text = penultimateSignalStrengthLabel.Text;
            }
            AddNewNonCodedVariableDetail(2, pulseRate, signalStrength, period);
        }

        private void CreateDetail()
        {
            Grid newGrid = new Grid();
            newGrid.ColumnDefinitions.Add(new ColumnDefinition());
            newGrid.ColumnDefinitions.Add(new ColumnDefinition());
            newGrid.ColumnDefinitions.Add(new ColumnDefinition());
            newGrid.ColumnDefinitions.Add(new ColumnDefinition());
            newGrid.Padding = new Thickness(0, 8, 0, 8);

            Label firstLabel = CreateLabel();
            Label detectionsLabel = CreateLabel();
            Label secondLabel = CreateLabel();
            Label signalStrengthLabel = CreateLabel();
            Label extraLabel = new Label();
            extraLabel.IsVisible = false;

            newGrid.Children.Add(firstLabel, 0, 0);
            newGrid.Children.Add(detectionsLabel, 1, 0);
            newGrid.Children.Add(secondLabel, 2, 0);
            newGrid.Children.Add(signalStrengthLabel, 3, 0);
            newGrid.Children.Add(extraLabel, 3, 0);

            StackLayout line = new StackLayout();
            line.BackgroundColor = Color.FromHex("E5E5E5");
            line.HorizontalOptions = LayoutOptions.Fill;
            line.HeightRequest = 1;

            ScanDetails.Children.Add(newGrid);
            ScanDetails.Children.Add(line);
        }

        private Label CreateLabel()
        {
            Label label = new Label();
            label.HorizontalTextAlignment = TextAlignment.Center;
            label.VerticalTextAlignment = TextAlignment.Center;
            label.TextColor = Color.FromHex("1F2933");
            label.FontSize = 16;
            return label;
        }

        private void Clear()
        {
            int count = ScanDetails.Children.Count;
            while (count > 2)
            {
                ScanDetails.Children.RemoveAt(2);
                count--;
            }
        }

        private void SetGpsOff(string message)
        {
            GPSLocation.Source = "GpsOff";
            State.Text = message;
        }

        private void SetGpsOn()
        {
            GPSLocation.Source = "GpsOn";
            State.Text = "Valid";
        }
    }
}
