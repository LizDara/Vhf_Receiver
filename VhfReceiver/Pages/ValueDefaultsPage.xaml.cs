using System;
using System.Collections.Generic;
using System.Windows.Input;
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
        private int BaseFrequency;
        private int FrequencyRange;
        private int StoreRateNumber;

        public ICommand StoreRateCommand => new Command<string>(StoreRate_Tapped);

        public ValueDefaultsPage(byte[] bytes, int value)
        {
            Initialize(value);

            switch (value)
            {
                case ValueCodes.SCAN_RATE_SECONDS:
                    SetScanTimeSeconds(bytes);
                    break;
                case ValueCodes.NUMBER_OF_ANTENNAS:
                    SetNumberOfAntennas(bytes);
                    break;
                case ValueCodes.SCAN_TIMEOUT_SECONDS:
                    SetScanTimeoutSeconds(bytes);
                    break;
                case ValueCodes.STORE_RATE:
                    SetStoreRate(bytes);
                    break;
                case ValueCodes.REFERENCE_FREQUENCY:
                    SetReferenceFrequency(bytes);
                    break;
                case ValueCodes.REFERENCE_FREQUENCY_STORE_RATE:
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
        }

        private void SetFrequencyTableNumber(byte[] bytes, byte[] tables)
        {
            if (ValueType == ValueCodes.TABLES_NUMBER)
            {
                SetVisibility("tables");

                TablesInformation = new List<TableInformation>() {
                    new TableInformation(1, bytes[1]),
                    new TableInformation(2, bytes[2]),
                    new TableInformation(3, bytes[3]),
                    new TableInformation(4, bytes[4]),
                    new TableInformation(5, bytes[5]),
                    new TableInformation(6, bytes[6]),
                    new TableInformation(7, bytes[7]),
                    new TableInformation(8, bytes[8]),
                    new TableInformation(9, bytes[9]),
                    new TableInformation(10, bytes[10]),
                    new TableInformation(11, bytes[11]),
                    new TableInformation(12, bytes[12])
                };
                if (bytes[9] != 0)
                    TablesInformation[bytes[9] - 1].IsChecked = true;
                if (bytes[10] != 0)
                    TablesInformation[bytes[10] - 1].IsChecked = true;
                if (bytes[11] != 0)
                    TablesInformation[bytes[11] - 1].IsChecked = true;
                TablesList.ItemsSource = TablesInformation;
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

            int antennasNumber = bytes[2] & 15;

            Value.ItemsSource = ItemList.Antenna();

            Value.SelectedIndex = (antennasNumber <= 4 && antennasNumber > 0) ? antennasNumber - 1 : 0;
        }

        private void SetScanTimeoutSeconds(byte[] bytes)
        {
            SetVisibility("");

            int timeout = bytes[4];

            Value.ItemsSource = ItemList.Timeout();

            Value.SelectedIndex = (timeout <= 200) ? timeout - 2 : 0;
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
            BaseFrequency = baseFrequency * 1000;
            int baseRange = Preferences.Get("Range", 0);
            FrequencyRange = ((baseFrequency + baseRange) * 1000) - 1;
            string message = "Frequency range is " + BaseFrequency + " to " + FrequencyRange;
            FrequencyBaseRange.Text = message;
            CreateNumberButtons(baseRange);

            if (bytes[6] != 0 && bytes[7] != 0)
            {
                int frequency = (bytes[6] * 256) + bytes[7] + BaseFrequency;
                NewFrequency.Text = frequency.ToString();
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
            if (ValueType == ValueCodes.STORE_RATE)
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

        private void TextListener_Changed(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (NewFrequency.Text.Length == 6)
            {
                int frequency = int.Parse(NewFrequency.Text);
                if (frequency >= BaseFrequency && frequency <= FrequencyRange)
                {
                    SaveFrequency.IsEnabled = true;
                    SaveFrequency.Opacity = 1;
                    Line.BackgroundColor = Color.FromHex("#CBD2D9");
                    FrequencyBaseRange.TextColor = Color.FromRgb(123, 135, 148);
                }
            }
            else
            {
                SaveFrequency.IsEnabled = false;
                SaveFrequency.Opacity = 0.6;
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

        private void SaveFrequency_Clicked(object sender, EventArgs e)
        {
            int frequencyNumber = int.Parse(NewFrequency.Text);
            MessagingCenter.Send(new int[] { ValueType, frequencyNumber }, ValueCodes.VALUE);
        }

        private void TablesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            TablesInformation[e.ItemIndex].IsChecked = !TablesInformation[e.ItemIndex].IsChecked;
            TablesList.ItemsSource = TablesInformation;
        }

        private void SaveTables_Clicked(object sender, EventArgs e)
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
                    TitleToolbar.Text = "Store Rate";
                    break;
                case "tables":
                    StoreRate.IsVisible = false;
                    SetValue.IsVisible = false;
                    Tables.IsVisible = true;
                    EditFrequency.IsVisible = false;
                    TitleToolbar.Text = "Frequency Tables to Scan";
                    break;
                case "frequency":
                    StoreRate.IsVisible = false;
                    SetValue.IsVisible = false;
                    Tables.IsVisible = false;
                    EditFrequency.IsVisible = true;
                    TitleToolbar.Text = "Reference Frequency Value";
                    break;
                default:
                    StoreRate.IsVisible = false;
                    SetValue.IsVisible = true;
                    Tables.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    break;
            }
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
    }
}
