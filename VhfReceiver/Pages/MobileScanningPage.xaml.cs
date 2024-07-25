using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class MobileScanningPage : ContentPage
    {
        private ReceiverInformation ReceiverInformation;
        private byte[] MobileBytes;
        private bool IsScanning;
        private bool IsHold;
        private bool IsRecord; //This can change during scanning
        private int BaseFrequency;
        private int FrequencyRange;
        private int SelectedTableNumber;
        private int AutoRecordNumber; //This is the default record
        private int Gps;
        private int CodeNumber;
        private int DetectionsNumber;
        private int MortNumber;
        private byte DetectionType;
        private List<TableInformation> TablesInformation;

        public string firstColor;
        public string FirstColor
        {
            set
            {
                firstColor = value;
                OnPropertyChanged(nameof(FirstColor));
            }
            get { return firstColor; }
        }
        public string secondColor;
        public string SecondColor
        {
            set
            {
                secondColor = value;
                OnPropertyChanged(nameof(SecondColor));
            }
            get { return secondColor; }
        }

        public MobileScanningPage(byte[] bytes)
        {
            Initialize();

            IsScanning = false;
            IsHold = false;

            SetData(bytes);
        }

        public MobileScanningPage(ICharacteristic characteristic, byte[] bytes, bool isHold, bool autoRecord, int currentFrequency, int currentIndex, int maxIndex, byte detectionType)
        {
            Initialize();

            IsScanning = true;
            IsHold = isHold;
            IsRecord = autoRecord;
            AutoRecordNumber = IsRecord ? 1 : 0;
            DetectionType = detectionType;
            FrequencyScan.Text = (currentFrequency + BaseFrequency).ToString();
            TableIndex.Text = currentIndex.ToString();
            MaxIndex.Text = maxIndex.ToString();
            TableTotal.Text = maxIndex.ToString();
            if (IsHold) SetHold();
            else RemoveHold();
            if (IsRecord) SetRecord();
            else RemoveRecord();
            UpdateVisibility();
            SetVisibility("scanning");

            LogScan(characteristic);

            SetData(bytes);
        }

        public MobileScanningPage(string scanTime, string tableNumber, string gps, string autoRecord, byte[] bytes)
        {
            Initialize();

            IsScanning = false;

            SetData(scanTime, tableNumber, gps, autoRecord, bytes);
        }

        private void Initialize()
        {
            InitializeComponent();
            BindingContext = this;
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();

            int baseFrequency = Preferences.Get("BaseFrequency", 0);
            BaseFrequency = baseFrequency * 1000;
            int baseRange = Preferences.Get("Range", 0);
            FrequencyRange = ((baseFrequency + baseRange) * 1000) - 1;
            string message = "Frequency range is " + BaseFrequency + " to " + FrequencyRange;
            FrequencyBaseRange.Text = message;
            IsHold = false;

            CreateNumberButtons(baseRange);
        }

        private void SetData(byte[] bytes)
        {
            if (Converters.GetHexValue(bytes[0]).Equals("6D"))
            {
                if (bytes[1] == 0)
                {
                    SelectedFrequencyTable.Text = "None";
                    StartMobileScan.IsEnabled = false;
                    StartMobileScan.Opacity = 0.6;
                }
                else
                {
                    SelectedFrequencyTable.Text = bytes[1].ToString();
                    StartMobileScan.IsEnabled = true;
                    StartMobileScan.Opacity = 1;
                }
                SelectedTableNumber = bytes[1];
                float scanTime = (float) (bytes[3] * 0.1);
                ScanTime.Text = scanTime.ToString();
                Gps = bytes[2] >> 7 & 1;
                GPS.Text = Gps == 1 ? "ON" : "OFF";
                AutoRecordNumber = bytes[2] >> 6 & 1;
                AutoRecord.Text = AutoRecordNumber == 1 ? "ON" : "OFF";

                MobileBytes = bytes;
            }
        }

        private void SetData(string scanTime, string tableNumber, string gps, string autoRecord, byte[] bytes)
        {
            SelectedFrequencyTable.Text = tableNumber;
            StartMobileScan.IsEnabled = true;
            StartMobileScan.Opacity = 1;

            ScanTime.Text = scanTime;
            GPS.Text = gps;
            Gps = gps.Equals("ON") ? 1 : 0;
            AutoRecord.Text = autoRecord;
            AutoRecordNumber = autoRecord.Equals("ON") ? 1 : 0;

            MobileBytes = bytes;
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
                await Navigation.PushModalAsync(new MobileSettingsPage(), false);
            }
        }

        private async void EditSettings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new MobileDefaultsPage(MobileBytes), false);
        }

        private async void StartMobileScan_Clicked(object sender, EventArgs e)
        {
            bool result = await LogScan();
            if (result)
            {
                IsScanning = await SetStartScan();
                if (IsScanning)
                {
                    SetVisibility("scanning");
                    RemoveHold();
                    IsRecord = AutoRecordNumber == 1;
                    if (IsRecord) SetRecord();
                    else RemoveRecord();
                    SetGpsOff("Searching ...");
                    GPSState.IsVisible = Gps == 1;
                }
            }
        }

        private async void HoldFrequency_Clicked(object sender, EventArgs e)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
                    if (characteristic != null)
                    {
                        bool result = await SetHold(characteristic);

                        if (result)
                        {
                            IsHold = !IsHold;
                            if (IsHold) SetHold();
                            else RemoveHold();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Service: " + ex.Message);
            }
        }

        private async void TuneDown_Clicked(object sender, EventArgs e)
        {
            bool result = await SetFrequency(true);
            if (result)
            {
                int newFrequency = GetFrequencyNumber(FrequencyScan.Text) - 1;
                if (newFrequency == BaseFrequency)
                {
                    TuneDown.Source = "TuneDownLight";
                    TuneDown.BorderColor = Color.FromRgba(203, 210, 217, 1);
                    TuneDown.IsEnabled = false;
                }
                else if (newFrequency == FrequencyRange - 1)
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
                int newFrequency = GetFrequencyNumber(FrequencyScan.Text) + 1;
                if (newFrequency == BaseFrequency + 1)
                {
                    TuneDown.Source = "TuneDown";
                    TuneDown.BorderColor = Color.FromRgba(31, 41, 51, 1);
                    TuneDown.IsEnabled = true;
                }
                else if (newFrequency == FrequencyRange)
                {
                    TuneUp.Source = "TuneUpLight";
                    TuneUp.BorderColor = Color.FromRgba(203, 210, 217, 1);
                    TuneUp.IsEnabled = false;
                }
            }
        }

        private void EditTable_Tapped(object sender, EventArgs e)
        {
            SetVisibility("editTable");
            CurrentFrequency.Text = FrequencyScan.Text;
            CurrentIndex.Text = TableIndex.Text;
        }

        private void AddFrequencyTable_Tapped(object sender, EventArgs e)
        {
            SetVisibility("addFrequency");
            NewFrequency.Text = "Enter Frequency Digits";
            NewFrequency.TextColor = Color.FromRgba(123, 135, 148, 1);
            Line.BackgroundColor = Color.FromRgba(203, 210, 217, 1);
            FrequencyBaseRange.TextColor = Color.FromRgba(123, 135, 148, 1);
            SaveFrequency.Opacity = 0.6;
            SaveFrequency.IsEnabled = false;
        }

        private async void DeleteFrequencyTable_Tapped(object sender, EventArgs e)
        {
            int index = int.Parse(TableIndex.Text);
            bool result = await DeleteFrequency(index);
            if (result)
            {
                var popMessage = new FrequencyMessage("Frequency Deleted");
                await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

                SetVisibility("scanning");
            }
        }

        private async void MergeTable_Tapped(object sender, EventArgs e)
        {
            if (TablesInformation == null)
            {
                byte[] bytes = await GetTables();
                if (bytes != null)
                {
                    TablesInformation = new List<TableInformation>();
                    for (int i = 1; i <= 12; i++)
                    {
                        if (bytes[i] > 0)
                            TablesInformation.Add(new TableInformation(i, bytes[i]));
                    }
                    TablesList.ItemsSource = TablesInformation;
                }
            }
            SetVisibility("mergeTable");
        }

        private void TextListener_Changed(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (NewFrequency.Text.Length == 6)
            {
                int frequency = int.Parse(NewFrequency.Text);
                if (frequency >= BaseFrequency && frequency <= FrequencyRange)
                {
                    SaveFrequency.IsEnabled = true;
                    SaveFrequency.Opacity = 1;
                    Line.BackgroundColor = Color.FromRgba(123, 135, 148, 1);
                    FrequencyBaseRange.TextColor = Color.FromRgba(123, 135, 148, 1);
                }
            }
            else
            {
                SaveFrequency.IsEnabled = false;
                SaveFrequency.Opacity = 0.6;
                Line.BackgroundColor = Color.FromRgba(186, 37, 37, 1);
                FrequencyBaseRange.TextColor = Color.FromRgba(186, 37, 37, 1);
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

        private async void AddFrequency_Clicked(object sender, EventArgs e)
        {
            int newFrequency = GetFrequencyNumber(NewFrequency.Text);
            bool result = await AddFrequency(newFrequency);
            if (result)
            {
                var popMessage = new FrequencyMessage("Frequency Added");
                await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

                SetVisibility("scanning");
            }
        }

        private void TablesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            TablesInformation[e.ItemIndex].IsChecked = !TablesInformation[e.ItemIndex].IsChecked;
            TablesList.ItemsSource = TablesInformation;
        }

        private async void MergeTables_Clicked(object sender, EventArgs e)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SCAN_TABLE);
                    if (characteristic != null)
                    {
                        int index = 0;
                        while (index < TablesInformation.Count)
                        {
                            if (TablesInformation[index].IsChecked)
                            {
                                bool result = await SetTable(TablesInformation[index].Table, characteristic);
                                if (result)
                                    index++;
                            }
                            else
                            {
                                index++;
                            }
                        }
                        var popMessage = new FrequencyMessage("Tables Merged");
                        await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

                        SetVisibility("scanning");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Service: " + ex.Message);
            }
        }

        private async void RecordData_Clicked(object sender, EventArgs e)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
                    if (characteristic != null)
                    {
                        bool result = await SetRecord(characteristic);
                        if (result)
                        {
                            IsRecord = !IsRecord;
                            if (IsRecord) SetRecord();
                            else RemoveRecord();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Service: " + ex.Message);
            }
        }

        private async void Left_Clicked(object sender, EventArgs e)
        {
            bool result = await SetIndex(true);
        }

        private async void Right_Clicked(object sender, EventArgs e)
        {
            bool result = await SetIndex(false);
        }

        private void EditAudio_Tapped(object sender, EventArgs e)
        {
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "overview":
                    CurrentSettings.IsVisible = true;
                    MobileScanning.IsVisible = false;
                    EditTableScan.IsVisible = false;
                    AddFrequencyScan.IsVisible = false;
                    Tables.IsVisible = false;
                    TitleToolbar.Text = "Mobile Scanning";
                    Back.Source = "Back";
                    break;
                case "scanning":
                    CurrentSettings.IsVisible = false;
                    MobileScanning.IsVisible = true;
                    EditTableScan.IsVisible = false;
                    AddFrequencyScan.IsVisible = false;
                    Tables.IsVisible = false;
                    TitleToolbar.Text = "Mobile Scanning ...";
                    Back.Source = "Exit";
                    HoldFrequency.Text = "Hold Frequency";
                    break;
                case "editTable":
                    CurrentSettings.IsVisible = false;
                    MobileScanning.IsVisible = false;
                    EditTableScan.IsVisible = true;
                    AddFrequencyScan.IsVisible = false;
                    Tables.IsVisible = false;
                    break;
                case "addFrequency":
                    CurrentSettings.IsVisible = false;
                    MobileScanning.IsVisible = false;
                    EditTableScan.IsVisible = false;
                    AddFrequencyScan.IsVisible = true;
                    Tables.IsVisible = false;
                    TitleToolbar.Text = "Add to Scan Table";
                    Back.Source = "Back";
                    break;
                case "mergeTable":
                    CurrentSettings.IsVisible = false;
                    MobileScanning.IsVisible = false;
                    EditTableScan.IsVisible = false;
                    AddFrequencyScan.IsVisible = false;
                    Tables.IsVisible = true;
                    TitleToolbar.Text = "Merge Tables";
                    Back.Source = "Back";
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
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
                    if (characteristic != null)
                    {
                        DateTime dateTime = DateTime.Now;

                        byte[] b = new byte[] { 0x82, (byte) (dateTime.Year % 100), (byte) dateTime.Month,
                        (byte) dateTime.Day, (byte) dateTime.Hour, (byte) dateTime.Minute,
                        (byte) dateTime.Second, (byte) SelectedTableNumber, 0, 0 };

                        bool result = await characteristic.WriteAsync(b);
                        Console.WriteLine(Converters.GetDecimalValue(b));
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
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
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

        private async Task<bool> SetHold(ICharacteristic characteristic)
        {
            try
            {
                byte[] b;
                if (IsHold)
                    b = new byte[] { 0x80 };
                else
                    b = new byte[] { 0x81 };
                bool result = await characteristic.WriteAsync(b);

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return false;
        }

        private async Task<bool> SetRecord(ICharacteristic characteristic)
        {
            try
            {
                byte[] b;
                if (IsRecord)
                    b = new byte[] { 0x8E };
                else
                    b = new byte[] { 0x8C };
                bool result = await characteristic.WriteAsync(b);

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return false;
        }

        private async Task<bool> SetIndex(bool isLeft)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SCAN_TABLE);
                    if (characteristic != null)
                    {
                        byte[] b;
                        if (isLeft)
                            b = new byte[] { 0x57 };
                        else
                            b = new byte[] { 0x58 };
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

        private async Task<bool> AddFrequency(int frequency)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SCAN_TABLE);
                    if (characteristic != null)
                    {
                        byte[] b = new byte[] { 0x5D, (byte)((frequency - BaseFrequency) / 256), (byte)((frequency - BaseFrequency) % 256) };

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

        private async Task<bool> DeleteFrequency(int index)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SCAN_TABLE);
                    if (characteristic != null)
                    {
                        byte[] b = new byte[] { 0x5C, (byte)(index / 256), (byte)(index % 256) };

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

        private async Task<bool> SetTable(int tableNumber, ICharacteristic characteristic)
        {
            try
            {
                byte[] b = new byte[] { 0x8A, (byte)tableNumber };
                bool result = await characteristic.WriteAsync(b);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return false;
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
                case "F0":
                    LogScanHeader(value);
                    break;
                case "F1":
                    LogScanFix(value);
                    break;
                case "F2":
                    LogScanFixConsolidated(value);
                    break;
                case "A1":
                    break;
                default: //E1 and E2
                    LogScanData(value);
                    break;
            }
        }

        private void ScanState(byte[] value)
        {
            int maxIndex = (value[5] * 256) + value[6];
            MaxIndex.Text = "Table Index (" + maxIndex + " Total)";
            TableTotal.Text = maxIndex.ToString();
            DetectionType = value[18];
            UpdateVisibility();
        }

        private void GpsState(byte[] value)
        {
            int state = value[1];
            if (state == 3) SetGpsOn();
            else if (state == 2) SetGpsOff("Failed");
            else if (state == 1) SetGpsOff("Searching ...");
        }

        private void LogScanHeader(byte[] value)
        {
            Clear();
            int frequency = (value[1] * 256) + value[2] + BaseFrequency;
            FrequencyScan.Text = GetFrequency(frequency);
            TableIndex.Text = value[3].ToString();
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

        private int GetFrequencyNumber(string frequency)
        {
            return int.Parse(frequency.Replace(".", ""));
        }

        private void SetHold()
        {
            HoldFrequency.Text = "Release Frequency";
            HoldState.Source = "Locked";
            FrequencyScan.TextColor = Color.FromRgba(20, 125, 100, 1);
            EditTable.TextColor = Color.FromRgba(31, 41, 51, 1);
            EditTable.IsEnabled = true;
        }

        private void RemoveHold()
        {
            HoldFrequency.Text = "Hold Frequency";
            HoldState.Source = "Unlocked";
            FrequencyScan.TextColor = Color.FromRgba(31, 41, 51, 1);
            EditTable.TextColor = Color.FromRgba(203, 210, 217, 1);
            EditTable.IsEnabled = false;
        }

        private void SetRecord()
        {
            RecordData.Text = "Stop Recording";
            FirstColor = "#BA2525";
            SecondColor = "#BA2525";
        }

        private void RemoveRecord()
        {
            RecordData.Text = "Record Data";
            FirstColor = "#1BA786";
            SecondColor = "#147D64";
        }

        private void SetGpsOff(string message)
        {
            GPSLocation.Source = "GpsOff";
            GPSState.Text = message;
        }

        private void SetGpsOn()
        {
            GPSLocation.Source = "GpsOn";
            GPSState.Text = "Valid";
        }
    }
}
