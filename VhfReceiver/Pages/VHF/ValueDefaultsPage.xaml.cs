using System;
using System.Collections.Generic;
using System.Windows.Input;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class ValueDefaultsPage : ContentPage
    {
        private List<TableInformation> TablesInformation;
        private int ValueType;
        private int StoreRateNumber;
        private int Count;

        public ICommand StoreRateCommand => new Command<string>(StoreRate_Tapped);

        public ValueDefaultsPage(byte[] bytes, int value)
        {
            Initialize(value);

            switch (value)
            {
                case ValueCodes.SCAN_RATE_SECONDS_CODE:
                    SetScanTimeSeconds(bytes);
                    break;
                case ValueCodes.NUMBER_OF_ANTENNAS_CODE:
                    SetNumberOfAntennas(bytes);
                    break;
                case ValueCodes.SCAN_TIMEOUT_SECONDS_CODE:
                    SetScanTimeoutSeconds(bytes);
                    break;
                case ValueCodes.STORE_RATE_CODE:
                    SetStoreRate(bytes);
                    break;
                case ValueCodes.REFERENCE_FREQUENCY_CODE:
                    SetReferenceFrequency(bytes);
                    break;
                case ValueCodes.REFERENCE_FREQUENCY_STORE_RATE_CODE:
                    SetReferenceFrequencyStoreRate(bytes);
                    break;
            }
        }

        public ValueDefaultsPage(byte[] bytes, byte[] tables, int value)
        {
            Initialize(value);

            SetFrequencyTableNumber(bytes, tables);
        }

        private void Initialize(int value)
        {
            InitializeComponent();
            BindingContext = this;
            ValueType = value;
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery
        }

        private void SetFrequencyTableNumber(byte[] bytes, byte[] tables)
        {
            if (ValueType == ValueCodes.TABLES_NUMBER_CODE)
            {
                SetVisibility("tables");
                
                TablesInformation = new List<TableInformation>() {
                    new TableInformation(1, tables[1]),
                    new TableInformation(2, tables[2]),
                    new TableInformation(3, tables[3]),
                    new TableInformation(4, tables[4]),
                    new TableInformation(5, tables[5]),
                    new TableInformation(6, tables[6]),
                    new TableInformation(7, tables[7]),
                    new TableInformation(8, tables[8]),
                    new TableInformation(9, tables[9]),
                    new TableInformation(10, tables[10]),
                    new TableInformation(11, tables[11]),
                    new TableInformation(12, tables[12])
                };
                Count = 0;
                if (bytes[9] != 0 && bytes[9] <= 12)
                {
                    TablesInformation[bytes[9] - 1].IsChecked = true;
                    Count++;
                }
                if (bytes[10] != 0 && bytes[10] <= 12)
                {
                    TablesInformation[bytes[10] - 1].IsChecked = true;
                    Count++;
                }
                if (bytes[11] != 0 && bytes[11] <= 12)
                {
                    TablesInformation[bytes[11] - 1].IsChecked = true;
                    Count++;
                }
                SelectedTables.Text = Count + " Selected Tables (3 Max)";
                TablesList.ItemsSource = TablesInformation;
                SaveTables.IsEnabled = false;
                SaveTables.Opacity = 0.6;
            }
            else
            {
                SetVisibility("");

                int position = 0;
                List<ValueInformation> tablesWithFrequencies = new List<ValueInformation>();
                for (int i = 1; i < 12; i++)
                {
                    if (tables[i] > 0)
                    {
                        tablesWithFrequencies.Add(new ValueInformation(i, "Table " + i));
                        if (bytes[1] == (byte)i)
                            position = tablesWithFrequencies.Count - 1;
                    }
                }
                if (tablesWithFrequencies.Count == 0)
                    tablesWithFrequencies.Add(new ValueInformation(0, "None"));
                Value.ItemsSource = tablesWithFrequencies;
                Value.SelectedIndex = position;
            }
        }

        private void SetScanTimeSeconds(byte[] bytes)
        {
            SetVisibility("");

            int scanTime = bytes[3];

            if (Converters.GetHexValue(bytes[0]).Equals("6D"))
            {
                Value.ItemsSource = ItemList.ScanTimeMobile();

                int position = (scanTime / 10) * 2;

                Value.SelectedIndex = (scanTime % 10 > 0) ? position - 2 : position - 3;
            }
            else
            {
                Value.ItemsSource = ItemList.ScanTimeStationary();

                Value.SelectedIndex = (scanTime <= 255) ? scanTime - 3 : 0;
            }
        }

        private void SetNumberOfAntennas(byte[] bytes)
        {
            SetVisibility("");

            int antennasNumber = bytes[1];

            Value.ItemsSource = ItemList.Antenna();

            Value.SelectedIndex = (antennasNumber <= 4 && antennasNumber > 0) ? antennasNumber - 1 : 0;
        }

        private void SetScanTimeoutSeconds(byte[] bytes)
        {
            SetVisibility("");

            int timeout = bytes[4];

            Value.ItemsSource = ItemList.Timeout();

            Value.SelectedIndex = (timeout <= 200) ? timeout - 3 : 0;
        }

        private void SetStoreRate(byte[] bytes)
        {
            SetVisibility("storeRate");

            switch (bytes[5])
            {
                case 0:
                    ContinuousStore.IsVisible = true;
                    break;
                case 5:
                    FiveMinutes.IsVisible = true;
                    break;
                case 10:
                    TenMinutes.IsVisible = true;
                    break;
                case 15:
                    FifteenMinutes.IsVisible = true;
                    break;
                case 30:
                    ThirtyMinutes.IsVisible = true;
                    break;
                case 60:
                    SixtyMinutes.IsVisible = true;
                    break;
                case 120:
                    OneHundredTwentyMinutes.IsVisible = true;
                    break;
            }
            StoreRateNumber = bytes[5];
        }

        private void SetReferenceFrequency(byte[] bytes)
        {
            SetVisibility("frequency");

            int baseFrequency = Preferences.Get("BaseFrequency", 0);
            int baseRange = Preferences.Get("Range", 0);

            EnterFrequency.SetData(baseFrequency, baseRange, SaveFrequency);

            if (bytes[6] != 0 || bytes[7] != 0)
            {
                int frequency = (bytes[6] * 256) + bytes[7] + (baseFrequency * 1000);
                EnterFrequency.newFrequency = frequency;
            }
        }

        private void SetReferenceFrequencyStoreRate(byte[] bytes)
        {
            SetVisibility("");

            int referenceFrequencyStoreRate = bytes[8];

            Value.ItemsSource = ItemList.ReferenceFrequencyStoreRate();

            Value.SelectedIndex = (referenceFrequencyStoreRate <= 24) ? referenceFrequencyStoreRate : 0;
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (ValueType == ValueCodes.STORE_RATE_CODE)
                MessagingCenter.Send(new int[] { ValueType, StoreRateNumber }, ValueCodes.VALUE);
            await Navigation.PopModalAsync(false);
        }

        private async void SaveChanges_Clicked(object sender, EventArgs e)
        {
            var selectedItem = Value.SelectedItem as ValueInformation;
            MessagingCenter.Send(new int[] { ValueType, selectedItem.Value }, ValueCodes.VALUE);

            await Navigation.PopModalAsync(false);
        }

        private void StoreRate_Tapped(string value)
        {
            switch (value)
            {
                case "0":
                    ContinuousStore.IsVisible = true;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = false;
                    FifteenMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = false;
                    OneHundredTwentyMinutes.IsVisible = false;
                    break;
                case "5":
                    ContinuousStore.IsVisible = false;
                    FiveMinutes.IsVisible = true;
                    TenMinutes.IsVisible = false;
                    FifteenMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = false;
                    OneHundredTwentyMinutes.IsVisible = false;
                    break;
                case "10":
                    ContinuousStore.IsVisible = false;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = true;
                    FifteenMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = false;
                    OneHundredTwentyMinutes.IsVisible = false;
                    break;
                case "15":
                    ContinuousStore.IsVisible = false;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = false;
                    FifteenMinutes.IsVisible = true;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = false;
                    OneHundredTwentyMinutes.IsVisible = false;
                    break;
                case "30":
                    ContinuousStore.IsVisible = false;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = false;
                    FifteenMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = true;
                    SixtyMinutes.IsVisible = false;
                    OneHundredTwentyMinutes.IsVisible = false;
                    break;
                case "60":
                    ContinuousStore.IsVisible = false;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = false;
                    FifteenMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = true;
                    OneHundredTwentyMinutes.IsVisible = false;
                    break;
                case "120":
                    ContinuousStore.IsVisible = false;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = false;
                    FifteenMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = false;
                    OneHundredTwentyMinutes.IsVisible = true;
                    break;
            }

            StoreRateNumber = int.Parse(value);
        }

        private async void SaveFrequency_Clicked(object sender, EventArgs e)
        {
            int frequencyNumber = EnterFrequency.newFrequency;
            MessagingCenter.Send(new int[] { ValueType, frequencyNumber }, ValueCodes.VALUE);

            await Navigation.PopModalAsync(false);
        }

        private void TablesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            TablesInformation[e.ItemIndex].IsChecked = !TablesInformation[e.ItemIndex].IsChecked;
            TablesList.ItemsSource = null;
            TablesList.ItemsSource = TablesInformation;
            if (TablesInformation[e.ItemIndex].IsChecked)
            {
                Count++;
                SelectedTables.Text = Count + " Selected Tables (3 Max)";
            }
            else
            {
                Count--;
                SelectedTables.Text = Count + " Selected Tables (3 Max)";
            }
            SaveTables.IsEnabled = Count > 0 && Count <= 3;
            SaveTables.Opacity = Count > 0 && Count <= 3 ? 1 : 0.6;
        }

        private async void SaveTables_Clicked(object sender, EventArgs e)
        {
            List<int> tables = new List<int>();
            foreach (TableInformation tableInformation in TablesInformation)
            {
                if (tableInformation.IsChecked)
                    tables.Add(tableInformation.Table);
            }
            int[] values = new int[tables.Count + 1];
            values[0] = ValueType;
            for (int i = 0; i < tables.Count; i++)
                values[i + 1] = tables[i];
            MessagingCenter.Send(values, ValueCodes.VALUE);

            await Navigation.PopModalAsync(false);
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "storeRate":
                    StoreRate.IsVisible = true;
                    SetValue.IsVisible = false;
                    Tables.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    Toolbar.SetData("Store Rate", true, Back_Clicked);
                    break;
                case "tables":
                    StoreRate.IsVisible = false;
                    SetValue.IsVisible = false;
                    Tables.IsVisible = true;
                    EditFrequency.IsVisible = false;
                    Toolbar.SetData("Frequency Tables to Scan", true);
                    break;
                case "frequency":
                    StoreRate.IsVisible = false;
                    SetValue.IsVisible = false;
                    Tables.IsVisible = false;
                    EditFrequency.IsVisible = true;
                    Toolbar.SetData("Reference Frequency Value", true);
                    break;
                default:
                    StoreRate.IsVisible = false;
                    SetValue.IsVisible = true;
                    Tables.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    Toolbar.SetData("Set Value", true);
                    break;
            }
        }

        private void ValueUpdateState(object o, CharacteristicUpdatedEventArgs args)
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
        }
    }
}
