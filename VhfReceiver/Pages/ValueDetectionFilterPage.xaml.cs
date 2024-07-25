using System;
using System.Windows.Input;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class ValueDetectionFilterPage : ContentPage
    {
        private int Value;
        private readonly int ValueType;

        public ICommand PulseRateCommand => new Command<string>(PulseRate_Tapped);
        public ICommand NumberMatchesCommand => new Command<string>(NumberMatches_Tapped);
        public ICommand DataCalculationCommand => new Command<string>(DataCalculation_Tapped);

        public ValueDetectionFilterPage(byte[] bytes, int value)
        {
            InitializeComponent();
            BindingContext = this;
            switch (value)
            {
                case ValueCodes.PULSE_RATE_TYPE:
                    SetVisibility("pulseRateTypes");
                    SetPulseRateType(bytes);
                    break;
                case ValueCodes.MATCHES_FOR_VALID_PATTERN:
                    SetVisibility("matches");
                    SetMatchesForValidPattern(bytes);
                    break;
                case ValueCodes.MAX_PULSE_RATE:
                    TitleToolbar.Text = "Maximum Pulse Rate";
                    MaxMinPulseRateTitle.Text = "Max Pulse Rate (ppm)";
                    SetVisibility("maxMin");
                    SetMaxPulseRate(bytes);
                    break;
                case ValueCodes.MIN_PULSE_RATE:
                    TitleToolbar.Text = "Minimum Pulse Rate";
                    MaxMinPulseRateTitle.Text = "Min Pulse Rate (ppm)";
                    SetVisibility("maxMin");
                    SetMinPulseRate(bytes);
                    break;
                case ValueCodes.DATA_CALCULATION_TYPES:
                    SetVisibility("calculation");
                    SetDataCalculationTypes(bytes);
                    break;
                case ValueCodes.PULSE_RATE_1:
                    TitleToolbar.Text = "Target Pulse Rate 1";
                    PulseRateTitle.Text = "PR1 (ppm)";
                    PulseRateToleranceTitle.Text = "PR1 Tolerance (ppm)";
                    SetVisibility("pulseRateValues");
                    SetPulseRate1(bytes);
                    break;
                case ValueCodes.PULSE_RATE_2:
                    TitleToolbar.Text = "Target Pulse Rate 2";
                    PulseRateTitle.Text = "PR2 (ppm)";
                    PulseRateToleranceTitle.Text = "PR2 Tolerance (ppm)";
                    SetVisibility("pulseRateValues");
                    SetPulseRate2(bytes);
                    break;
                case ValueCodes.PULSE_RATE_3:
                    TitleToolbar.Text = "Target Pulse Rate 3";
                    PulseRateTitle.Text = "PR3 (ppm)";
                    PulseRateToleranceTitle.Text = "PR3 Tolerance (ppm)";
                    SetVisibility("pulseRateValues");
                    SetPulseRate3(bytes);
                    break;
                case ValueCodes.PULSE_RATE_4:
                    TitleToolbar.Text = "Target Pulse Rate 4";
                    PulseRateTitle.Text = "PR4 (ppm)";
                    PulseRateToleranceTitle.Text = "PR4 Tolerance (ppm)";
                    SetVisibility("pulseRateValues");
                    SetPulseRate4(bytes);
                    break;
            }
            ValueType = value;
        }

        private void SetPulseRateType(byte[] bytes)
        {
            if (Converters.GetHexValue(bytes[1]).Equals("08"))
            {
                FixedPulseRate.IsVisible = true;
                Value = ValueCodes.FIXED_PULSE_RATE;
            }
            else if (Converters.GetHexValue(bytes[1]).Equals("07"))
            {
                VariablePulseRate.IsVisible = true;
                Value = ValueCodes.VARIABLE_PULSE_RATE;
            }
            else if (Converters.GetHexValue(bytes[1]).Equals("09"))
            {
                Coded.IsVisible = true;
                Value = ValueCodes.CODED;
            }
        }

        private void SetMatchesForValidPattern(byte[] bytes)
        {
            switch (Converters.GetDecimalValue(bytes[2]))
            {
                case "2":
                    Two.IsVisible = true;
                    break;
                case "3":
                    Three.IsVisible = true;
                    break;
                case "4":
                    Four.IsVisible = true;
                    break;
                case "5":
                    Five.IsVisible = true;
                    break;
                case "6":
                    Six.IsVisible = true;
                    break;
                case "7":
                    Seven.IsVisible = true;
                    break;
                case "8":
                    Eight.IsVisible = true;
                    break;
            }
            Value = bytes[2];
        }

        private void SetMaxPulseRate(byte[] bytes)
        {
            MaxMinPulseRate.IsVisible = true;
            int max = (bytes[3] * 256) + bytes[4];
            MaxMin.Text = max.ToString();
            double period = (max == 0) ? 0 : (double)60000 / max;
            PeriodPulseRate.Text = period.ToString() + " ms (period)";
            Value = max;
        }

        private void SetMinPulseRate(byte[] bytes)
        {
            MaxMinPulseRate.IsVisible = true;
            int min = (bytes[5] * 256) + bytes[6];
            MaxMin.Text = min.ToString();
            double period = (min == 0) ? 0 : (double)60000 / min;
            PeriodPulseRate.Text = period.ToString() + " ms (period)";
            Value = min;
        }

        private void SetDataCalculationTypes(byte[] bytes)
        {
            DataCalculationTypes.IsVisible = true;
            switch (Converters.GetHexValue(bytes[11]))
            {
                case "00":
                    None.IsVisible = true;
                    Value = ValueCodes.NONE;
                    break;
                case "08":
                    Period.IsVisible = true;
                    Value = ValueCodes.PERIOD;
                    break;
                case "04":
                    Temperature.IsVisible = true;
                    Value = ValueCodes.TEMPERATURE;
                    break;
            }
        }

        private void SetPulseRate1(byte[] bytes)
        {
            PulseRate.Text = bytes[3].ToString();
            PulseRateTolerance.Text = "+/- " + bytes[4].ToString();
            PulseRateTolerance.CursorPosition = PulseRateTolerance.Text.Length;
            Value = (bytes[3] * 100) + bytes[4];
        }

        private void SetPulseRate2(byte[] bytes)
        {
            PulseRate.Text = bytes[5].ToString();
            PulseRateTolerance.Text = "+/- " + bytes[6].ToString();
            PulseRateTolerance.CursorPosition = PulseRateTolerance.Text.Length;
            Value = (bytes[5] * 100) + bytes[6];
        }

        private void SetPulseRate3(byte[] bytes)
        {
            PulseRate.Text = bytes[7].ToString();
            PulseRateTolerance.Text = "+/- " + bytes[8].ToString();
            PulseRateTolerance.CursorPosition = PulseRateTolerance.Text.Length;
            Value = (bytes[7] * 100) + bytes[8];
        }

        private void SetPulseRate4(byte[] bytes)
        {
            PulseRate.Text = bytes[9].ToString();
            PulseRateTolerance.Text = "+/- " + bytes[10].ToString();
            PulseRateTolerance.CursorPosition = PulseRateTolerance.Text.Length;
            Value = (bytes[9] * 100) + bytes[10];
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (ValueType == ValueCodes.PULSE_RATE_1 || ValueType == ValueCodes.PULSE_RATE_2 || ValueType == ValueCodes.PULSE_RATE_3 || ValueType == ValueCodes.PULSE_RATE_4)
            {
                int pulseRate = int.Parse(PulseRate.Text);
                int pulseRateTolerance = int.Parse(PulseRateTolerance.Text.Replace("+/- ", ""));
                if (pulseRate > 0 && pulseRate <= 150 && pulseRateTolerance > 0 && pulseRateTolerance <= 10)
                {
                    Value = (pulseRate * 100) + pulseRateTolerance;
                }
                else
                {
                    await DisplayAlert("Invalid Format or Values", "Please enter valid pulse rate or tolerance values.", "OK");
                    return;
                }
            }
            else if (ValueType == ValueCodes.MAX_PULSE_RATE || ValueType == ValueCodes.MIN_PULSE_RATE)
            {
                Value = int.Parse(MaxMin.Text);
            }
            MessagingCenter.Send(new int[] { ValueType, Value }, ValueCodes.VALUE);

            await Navigation.PopModalAsync(false);
        }

        private void PulseRate_Tapped(string value)
        {
            switch (value)
            {
                case "Coded":
                    Coded.IsVisible = true;
                    FixedPulseRate.IsVisible = false;
                    VariablePulseRate.IsVisible = false;
                    Value = ValueCodes.CODED;
                    break;
                case "Fixed":
                    Coded.IsVisible = false;
                    FixedPulseRate.IsVisible = true;
                    VariablePulseRate.IsVisible = false;
                    Value = ValueCodes.FIXED_PULSE_RATE;
                    break;
                case "Variable":
                    Coded.IsVisible = false;
                    VariablePulseRate.IsVisible = true;
                    FixedPulseRate.IsVisible = false;
                    Value = ValueCodes.VARIABLE_PULSE_RATE;
                    break;
            }
        }

        private void NumberMatches_Tapped(string value)
        {
            switch (value)
            {
                case "2":
                    Two.IsVisible = true;
                    Three.IsVisible = false;
                    Four.IsVisible = false;
                    Five.IsVisible = false;
                    Six.IsVisible = false;
                    Seven.IsVisible = false;
                    Eight.IsVisible = false;
                    break;
                case "3":
                    Three.IsVisible = true;
                    Two.IsVisible = false;
                    Four.IsVisible = false;
                    Five.IsVisible = false;
                    Six.IsVisible = false;
                    Seven.IsVisible = false;
                    Eight.IsVisible = false;
                    break;
                case "4":
                    Four.IsVisible = true;
                    Two.IsVisible = false;
                    Three.IsVisible = false;
                    Five.IsVisible = false;
                    Six.IsVisible = false;
                    Seven.IsVisible = false;
                    Eight.IsVisible = false;
                    break;
                case "5":
                    Five.IsVisible = true;
                    Two.IsVisible = false;
                    Three.IsVisible = false;
                    Four.IsVisible = false;
                    Six.IsVisible = false;
                    Seven.IsVisible = false;
                    Eight.IsVisible = false;
                    break;
                case "6":
                    Six.IsVisible = true;
                    Two.IsVisible = false;
                    Three.IsVisible = false;
                    Four.IsVisible = false;
                    Five.IsVisible = false;
                    Seven.IsVisible = false;
                    Eight.IsVisible = false;
                    break;
                case "7":
                    Seven.IsVisible = true;
                    Two.IsVisible = false;
                    Three.IsVisible = false;
                    Four.IsVisible = false;
                    Five.IsVisible = false;
                    Six.IsVisible = false;
                    Eight.IsVisible = false;
                    break;
                case "8":
                    Eight.IsVisible = true;
                    Two.IsVisible = false;
                    Three.IsVisible = false;
                    Four.IsVisible = false;
                    Five.IsVisible = false;
                    Six.IsVisible = false;
                    Seven.IsVisible = false;
                    break;
            }
            Value = int.Parse(value);
        }

        private void DataCalculation_Tapped(string value)
        {
            switch (value)
            {
                case "0":
                    None.IsVisible = true;
                    Period.IsVisible = false;
                    Temperature.IsVisible = false;
                    break;
                case "8":
                    Period.IsVisible = true;
                    None.IsVisible = false;
                    Temperature.IsVisible = false;
                    break;
                case "4":
                    Temperature.IsVisible = true;
                    None.IsVisible = false;
                    Period.IsVisible = false;
                    break;
            }
            Value = int.Parse(value);
        }

        private void MaxMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            int pulseRate = (e.NewTextValue.Length == 0) ? 0 : int.Parse(e.NewTextValue);
            double period = (e.NewTextValue.Length == 0) ? 0 : (double)60000 / pulseRate;
            PeriodPulseRate.Text = string.Format("{0:f2}", period) + " ms (period)";
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "pulseRateTypes":
                    SelectPulseRate.IsVisible = true;
                    NumberOfMatches.IsVisible = false;
                    TargetPulseRate.IsVisible = false;
                    MaxMinPulseRate.IsVisible = false;
                    DataCalculationTypes.IsVisible = false;
                    TitleToolbar.Text = "Pulse Rate Type Options";
                    break;
                case "matches":
                    SelectPulseRate.IsVisible = false;
                    NumberOfMatches.IsVisible = true;
                    TargetPulseRate.IsVisible = false;
                    MaxMinPulseRate.IsVisible = false;
                    DataCalculationTypes.IsVisible = false;
                    TitleToolbar.Text = "Matches for Valid Pattern";
                    break;
                case "pulseRateValues":
                    SelectPulseRate.IsVisible = false;
                    NumberOfMatches.IsVisible = false;
                    TargetPulseRate.IsVisible = true;
                    MaxMinPulseRate.IsVisible = false;
                    DataCalculationTypes.IsVisible = false;
                    break;
                case "maxMin":
                    SelectPulseRate.IsVisible = false;
                    NumberOfMatches.IsVisible = false;
                    TargetPulseRate.IsVisible = false;
                    MaxMinPulseRate.IsVisible = true;
                    DataCalculationTypes.IsVisible = false;
                    break;
                case "calculation":
                    SelectPulseRate.IsVisible = false;
                    NumberOfMatches.IsVisible = false;
                    TargetPulseRate.IsVisible = false;
                    MaxMinPulseRate.IsVisible = false;
                    DataCalculationTypes.IsVisible = true;
                    TitleToolbar.Text = "Optional Data Calculations";
                    break;
            }
        }
    }
}
