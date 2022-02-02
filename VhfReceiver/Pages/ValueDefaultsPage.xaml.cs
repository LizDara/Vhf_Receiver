using System;
using System.Collections.Generic;
using System.Windows.Input;
using VhfReceiver.Utils;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class ValueDefaultsPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;
        private List<ValueInformation> Values;
        private int ValueType;

        public const int FREQUENCY_TABLE_NUMBER = 1001;
        public const int SCAN_TIME_SECONDS = 1002;
        public const int NUMBER_OF_ANTENNAS = 1003;
        public const int SCAN_TIMEOUT_SECONDS= 1004;
        public const int STORE_RATE = 1005;
        public const int REFERENCE_FREQUENCY = 1006;
        public const int REFERENCE_FREQUENCY_STORE_RATE = 1007;

        public ICommand StoreRateCommand => new Command<string>(StoreRate_Tapped);

        public ValueDefaultsPage(DeviceInformation device, byte[] bytes, int value)
        {
            InitializeComponent();
            BindingContext = this;

            ConnectedDevice = device;

            Name.Text = ConnectedDevice.Name;
            Range.Text = ConnectedDevice.Range;
            Battery.Text = ConnectedDevice.Battery;

            switch (value)
            {
                case FREQUENCY_TABLE_NUMBER:
                    SetFrequencyTableNumber(bytes);
                    break;
                case SCAN_TIME_SECONDS:
                    SetScanTimeSeconds(bytes);
                    break;
                case NUMBER_OF_ANTENNAS:
                    SetNumberOfAntennas(bytes);
                    break;
                case SCAN_TIMEOUT_SECONDS:
                    SetScanTimeoutSeconds(bytes);
                    break;
                case STORE_RATE:
                    SetStoreRate(bytes);
                    break;
                case REFERENCE_FREQUENCY:
                    SetReferenceFrequency(bytes);
                    break;
                case REFERENCE_FREQUENCY_STORE_RATE:
                    SetReferenceFrequencyStoreRate(bytes);
                    break;
            }
            ValueType = value;
        }

        private void SetFrequencyTableNumber(byte[] bytes)
        {
            Values = new List<ValueInformation>();
            int position = 0;
            byte b = Converters.GetHexValue(bytes[0]).Equals("6D") ? bytes[6] : bytes[9];

            for (int i = 1; i <= 8; i++)
            {
                if ((b & 1) == 1)
                {
                    Values.Add(new ValueInformation(i, "Table " + i));
                    position = (i == bytes[1]) ? Values.Count - 1 : 0;
                }
                b = (byte)(b >> 1);
            }

            b = Converters.GetHexValue(bytes[0]).Equals("6D") ? bytes[7] : bytes[10];
            for (int i = 9; i <= 12; i++)
            {
                if ((b & 1) == 1)
                {
                    Values.Add(new ValueInformation(i, "Table " + i));
                    position = (i == bytes[1]) ? Values.Count - 1 : 0;
                }
                b = (byte)(b >> 1);
            }

            if (Values.Count == 0)
            {
                Values.Add(new ValueInformation(0, "None"));
            }

            Value.ItemsSource = Values;
            Value.SelectedIndex = position;
        }

        private void SetScanTimeSeconds(byte[] bytes)
        {
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
            int antennasNumber = bytes[2] & 15;

            Value.ItemsSource = ItemList.Antenna();

            Value.SelectedIndex = (antennasNumber <= 4 && antennasNumber > 0) ? antennasNumber - 1 : 0;
        }

        private void SetScanTimeoutSeconds(byte[] bytes)
        {
            int timeout = bytes[4];

            Value.ItemsSource = ItemList.Timeout();

            Value.SelectedIndex = (timeout <= 200) ? timeout - 2 : 0;
        }

        private void SetStoreRate(byte[] bytes)
        {
            SetValue.IsVisible = false;
            StoreRate.IsVisible = true;

            switch (bytes[5])
            {
                case 0:
                    NoStoreRate.IsVisible = true;
                    break;
                case 5:
                    FiveMinutes.IsVisible = true;
                    break;
                case 10:
                    TenMinutes.IsVisible = true;
                    break;
                case 20:
                    TwentyMinutes.IsVisible = true;
                    break;
                case 30:
                    ThirtyMinutes.IsVisible = true;
                    break;
                case 60:
                    SixtyMinutes.IsVisible = true;
                    break;
            }

            Values = new List<ValueInformation>() { new ValueInformation(bytes[5], bytes[5].ToString()) };
        }

        private void SetReferenceFrequency(byte[] bytes)
        {
        }

        private void SetReferenceFrequencyStoreRate(byte[] bytes)
        {
            int referenceFrequencyStoreRate = bytes[8];

            Value.ItemsSource = ItemList.ReferenceFrequencyStoreRate();

            Value.SelectedIndex = (referenceFrequencyStoreRate <= 24) ? referenceFrequencyStoreRate : 0;
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (ValueType == STORE_RATE)
            {
                MessagingCenter.Send(new int[] { ValueType, Values[0].Value }, "Value");
            }
            await Navigation.PopModalAsync();
        }

        private async void SaveChanges_Clicked(object sender, EventArgs e)
        {
            var selectedItem = Value.SelectedItem as ValueInformation;
            MessagingCenter.Send(new int[] { ValueType, selectedItem.Value }, "Value");

            await Navigation.PopModalAsync();
        }

        private void StoreRate_Tapped(string value)
        {
            switch (value)
            {
                case "0":
                    NoStoreRate.IsVisible = true;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = false;
                    TwentyMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = false;
                    break;
                case "5":
                    NoStoreRate.IsVisible = false;
                    FiveMinutes.IsVisible = true;
                    TenMinutes.IsVisible = false;
                    TwentyMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = false;
                    break;
                case "10":
                    NoStoreRate.IsVisible = false;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = true;
                    TwentyMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = false;
                    break;
                case "20":
                    NoStoreRate.IsVisible = false;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = false;
                    TwentyMinutes.IsVisible = true;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = false;
                    break;
                case "30":
                    NoStoreRate.IsVisible = false;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = false;
                    TwentyMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = true;
                    SixtyMinutes.IsVisible = false;
                    break;
                case "60":
                    NoStoreRate.IsVisible = false;
                    FiveMinutes.IsVisible = false;
                    TenMinutes.IsVisible = false;
                    TwentyMinutes.IsVisible = false;
                    ThirtyMinutes.IsVisible = false;
                    SixtyMinutes.IsVisible = true;
                    break;
            }

            Values[0] = new ValueInformation(int.Parse(value), value);
        }
    }
}
