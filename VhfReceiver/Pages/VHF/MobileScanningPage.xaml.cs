using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class MobileScanningPage : ContentPage
    {
        private bool IsScanning;
        private bool IsHold;
        private int BaseFrequency;
        private int FrequencyRange;
        private byte DetectionType;
        private List<TableInformation> TablesInformation;
        private int TableIndexMerge;

        public MobileScanningPage(byte[] bytes) //Coming from select scan
        {
            Initialize(bytes);

            IsScanning = false;
            IsHold = false;
            _ = TransferBLEData.NotificationLog(ValueUpdateScan); // Log sd card state and battery
        }

        public MobileScanningPage(ICharacteristic characteristic, byte[] bytes, byte[] value) //Coming from connect ble
        {
            Initialize(bytes);

            IsScanning = true;
            IsHold = Converters.GetHexValue(value[1]).Equals("81");
            int autoRecord = value[2] >> 6 & 1;
            int gps = value[15] >> 7 & 1;
            GpsRecordOptions.IsRecord = autoRecord == 1;
            int currentFrequency = (value[16] * 256) + value[17] + BaseFrequency;
            int currentIndex = (value[7] * 256) + value[8];
            FrequencyScan.Text = Converters.GetFrequency(currentFrequency);
            TableIndex.Text = currentIndex.ToString();
            if (IsHold) SetHold();
            else RemoveHold();
            if (GpsRecordOptions.IsRecord) GpsRecordOptions.SetMobileRecording();
            else GpsRecordOptions.RemoveRecord();
            if (gps == 1) SetGpsSearching();
            else SetGpsOff();
            ScanState(value);
            SetVisibility("scanning");

            LogScan(characteristic);
        }

        private void Initialize(byte[] bytes)
        {
            InitializeComponent();
            BindingContext = this;

            int baseFrequency = Preferences.Get("BaseFrequency", 0);
            BaseFrequency = baseFrequency * 1000;
            int baseRange = Preferences.Get("Range", 0);
            FrequencyRange = ((baseFrequency + baseRange) * 1000) - 1;
            TableIndexMerge = -1;

            Toolbar.SetData("Mobile Scanning", true, Back_Clicked);
            EnterFrequency.SetData(baseFrequency, baseRange, SaveFrequency);
            MobileSetting.SetData(bytes, true);
            GpsRecordOptions.SetData(true);
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (IsScanning)
            {
                if (Tables.IsVisible || EditFrequency.IsVisible)
                    SetVisibility("editTable");
                else if (EditTableScan.IsVisible)
                    SetVisibility("scanning");
                else
                {
                    bool result = await TransferBLEData.WriteStopScan(ValueCodes.MOBILE);
                    if (result)
                    {
                        IsScanning = false;
                        SetVisibility("overview");
                        Clear();
                    }
                }
            }
            else
            {
                byte[] bytes = await TransferBLEData.ReadTables();
                if (bytes != null)
                    await Navigation.PushModalAsync(new StartScanningPage(bytes), false);
            }
        }

        private async void StartMobileScan_Clicked(object sender, EventArgs e)
        {
            /*bool result = await TransferBLEData.NotificationLog(ValueUpdateScan);
            if (result)
            {*/
                IsScanning = await SetStartScan();
                if (IsScanning)
                {
                    SetVisibility("scanning");
                    RemoveHold();
                    if (MobileSetting.autoRecord) GpsRecordOptions.SetMobileRecording();
                    else GpsRecordOptions.RemoveRecord();
                    if (MobileSetting.gps) SetGpsSearching();
                    else SetGpsOff();
                }
            //}
        }

        private async void HoldFrequency_Clicked(object sender, EventArgs e)
        {
            bool result = await TransferBLEData.WriteHold(IsHold);
            if (result)
            {
                IsHold = !IsHold;
                if (IsHold) SetHold();
                else RemoveHold();
            }
        }

        private async void TuneDown_Clicked(object sender, EventArgs e)
        {
            bool result = await TransferBLEData.WriteDecreaseIncrease(true);
            if (result)
            {
                int newFrequency = Converters.GetFrequencyNumber(FrequencyScan.Text) - 1;
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
            bool result = await TransferBLEData.WriteDecreaseIncrease(false);
            if (result)
            {
                int newFrequency = Converters.GetFrequencyNumber(FrequencyScan.Text) + 1;
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
            EnterFrequency.Initialize();
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
                byte[] bytes = await TransferBLEData.ReadTables();
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

        private async void SaveFrequency_Clicked(object sender, EventArgs e)
        {
            int newFrequency = EnterFrequency.newFrequency;
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
            if (TablesInformation[e.ItemIndex].IsChecked)
            {
                TablesInformation[e.ItemIndex].IsChecked = false;
                MergeTables.Opacity = 0.6;
                MergeTables.IsEnabled = false;
                TableIndexMerge = -1;
            }
            else
            {
                for (int i = 0; i < TablesInformation.Count; i++)
                {
                    if (i != e.ItemIndex)
                        TablesInformation[i].IsChecked = false;
                    else
                        TablesInformation[i].IsChecked = true;
                }
                TableIndexMerge = e.ItemIndex;
                MergeTables.Opacity = 1;
                MergeTables.IsEnabled = true;
            }
            TablesList.ItemsSource = null;
            TablesList.ItemsSource = TablesInformation;
        }

        private async void MergeTables_Clicked(object sender, EventArgs e)
        {
            bool result = await SetTable();
            if (result)
            {
                var popMessage = new FrequencyMessage("Table Merged");
                await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

                IsHold = false;
                RemoveHold();
                SetVisibility("scanning");
            }
        }

        private async void Left_Clicked(object sender, EventArgs e)
        {
            await TransferBLEData.WriteLeftRight(true);
        }

        private async void Right_Clicked(object sender, EventArgs e)
        {
            await TransferBLEData.WriteLeftRight(false);
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "overview":
                    CurrentSettings.IsVisible = true;
                    MobileScanning.IsVisible = false;
                    EditTableScan.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    Tables.IsVisible = false;
                    Toolbar.SetData("Mobile Scanning");
                    Toolbar.ChangeButton(true);
                    break;
                case "scanning":
                    CurrentSettings.IsVisible = false;
                    MobileScanning.IsVisible = true;
                    EditTableScan.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    Tables.IsVisible = false;
                    Toolbar.SetData("Mobile Scanning ...");
                    Toolbar.ChangeButton(false);
                    break;
                case "editTable":
                    CurrentSettings.IsVisible = false;
                    MobileScanning.IsVisible = false;
                    EditTableScan.IsVisible = true;
                    EditFrequency.IsVisible = false;
                    Tables.IsVisible = false;
                    break;
                case "addFrequency":
                    CurrentSettings.IsVisible = false;
                    MobileScanning.IsVisible = false;
                    EditTableScan.IsVisible = false;
                    EditFrequency.IsVisible = true;
                    Tables.IsVisible = false;
                    Toolbar.SetData("Add to Scan Table");
                    Toolbar.ChangeButton(true);
                    break;
                case "mergeTable":
                    CurrentSettings.IsVisible = false;
                    MobileScanning.IsVisible = false;
                    EditTableScan.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    Tables.IsVisible = true;
                    Toolbar.SetData("Merge Tables");
                    Toolbar.ChangeButton(true);
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
                Console.WriteLine("Error Service: " + e.Message);
            }
        }

        private async Task<bool> SetStartScan()
        {
            DateTime dateTime = DateTime.Now;

            byte[] b = new byte[] { 0x82, (byte) (dateTime.Year % 100), (byte) dateTime.Month,
            (byte) dateTime.Day, (byte) dateTime.Hour, (byte) dateTime.Minute,
            (byte) dateTime.Second, (byte) MobileSetting.tableNumber, 0, 0 };

            bool result = await TransferBLEData.WriteStartScan(ValueCodes.MOBILE, b);
            return result;
        }

        private async Task<bool> AddFrequency(int frequency)
        {
            byte[] b = new byte[] { 0x5D, (byte)((frequency - BaseFrequency) / 256), (byte)((frequency - BaseFrequency) % 256) };

            bool result = await TransferBLEData.WriteScanning(b);
            return result;
        }

        private async Task<bool> DeleteFrequency(int index)
        {
            byte[] b = new byte[] { 0x5C, (byte)(index / 256), (byte)(index % 256) };

            bool result = await TransferBLEData.WriteScanning(b);
            return result;
        }

        private async Task<bool> SetTable()
        {
            byte[] b = new byte[] { 0x8A, (byte)TablesInformation[TableIndexMerge].Table };
            bool result = await TransferBLEData.WriteScanning(b);
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
                case "8A":
                    FrequenciesNumber(value);
                    break;
                case "F0":
                    LogScanHeader(value);
                    break;
                case "F1":
                    LogScanCoded(value);
                    break;
                case "A1":
                    LogGps(value);
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
            TableTotal.Text = maxIndex.ToString();
            DetectionType = value[18];
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

        private void FrequenciesNumber(byte[] value)
        {
            int maxIndex = (value[1] * 256) + value[2];
            MaxIndex.Text = "Table Index (" + maxIndex + " Total)";
            TableTotal.Text = maxIndex.ToString();
        }

        private void LogGps(byte[] value)
        {
            Coordinates.SetData(value);
        }

        private void LogScanHeader(byte[] value)
        {
            Clear();
            int frequency = (value[1] * 256) + value[2] + BaseFrequency;
            int index = (((value[1] >> 6) & 1) * 256) + value[3];
            FrequencyScan.Text = Converters.GetFrequency(frequency);
            TableIndex.Text = index.ToString();
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

        private void Clear()
        {
            FrequencyScan.Text = "";
            TableIndex.Text = "";
            Scanning.Clear();
        }

        private void SetHold()
        {
            HoldFrequency.Text = "Release Frequency";
            HoldState.Source = "Locked";
            FrequencyScan.TextColor = Color.FromHex("#147D64");
            EditTable.TextColor = Color.FromHex("#1F2933");
            EditTable.IsEnabled = true;
        }

        private void RemoveHold()
        {
            HoldFrequency.Text = "Hold Frequency";
            HoldState.Source = "Unlocked";
            FrequencyScan.TextColor = Color.FromHex("#1F2933");
            EditTable.TextColor = Color.FromHex("#CBD2D9");
            EditTable.IsEnabled = false;
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
