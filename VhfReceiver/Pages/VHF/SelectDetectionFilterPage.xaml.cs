using System;
using System.Collections.Generic;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class SelectDetectionFilterPage : ContentPage
    {
        private Dictionary<string, object> OriginalData;
        private readonly byte[] DetectionFilterBytes;
        private readonly bool ReturnDetection;

        public SelectDetectionFilterPage(byte[] bytes, bool returnDetection)
        {
            InitializeComponent();

            DetectionFilterBytes = bytes;
            ReturnDetection = returnDetection;
            SetData(bytes);
            Toolbar.SetData("Select Detection Filter", true, Back_Clicked);
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery

            MessagingCenter.Subscribe<int[]>(this, ValueCodes.VALUE, (value) => {
                switch (value[0])
                {
                    case ValueCodes.PULSE_RATE_TYPE_CODE:
                        if (value[1] == ValueCodes.FIXED_PULSE_RATE)
                            SetVisibility("Fixed");
                        else if (value[1] == ValueCodes.VARIABLE_PULSE_RATE)
                            SetVisibility("Variable");
                        else if (value[1] == ValueCodes.CODED)
                            SetVisibility("Coded");
                        break;
                    case ValueCodes.MATCHES_FOR_VALID_PATTERN:
                        MatchesForValidPattern.Text = value[1].ToString();
                        break;
                    case ValueCodes.MAX_PULSE_RATE_CODE:
                        MaxPulseRate.Text = value[1].ToString();
                        break;
                    case ValueCodes.MIN_PULSE_RATE_CODE:
                        MinPulseRate.Text = value[1].ToString();
                        break;
                    case ValueCodes.DATA_CALCULATION_TYPES:
                        if (value[1] == 0)
                            OptionalDataCalculation.Text = "None";
                        else if (value[1] == 6)
                            OptionalDataCalculation.Text = "Temperature";
                        break;
                    case ValueCodes.PULSE_RATE_1_CODE:
                        PR1.Text = (value[1] / 100).ToString();
                        PR1Tolerance.Text = (value[1] % 100).ToString();
                        break;
                    case ValueCodes.PULSE_RATE_2_CODE:
                        PR2.Text = (value[1] / 100).ToString();
                        PR2Tolerance.Text = (value[1] % 100).ToString();
                        break;
                }
            });
        }

        private void SetData(byte[] bytes)
        {
            int pulseRateType = bytes[1];
            int matches = bytes[2];
            int pulseRate1 = 0;
            int pulseRate2 = 0;
            int pulseRate3 = 0;
            int pulseRate4 = 0;
            int pulseRateTolerance1 = 0;
            int pulseRateTolerance2 = 0;
            int pulseRateTolerance3 = 0;
            int pulseRateTolerance4 = 0;
            int maxPulseRate = 0;
            int minPulseRate = 0;
            int optionalData = 0;
            switch (Converters.GetHexValue(bytes[1]))
            {
                case "09":
                    SetVisibility("Coded");
                    break;
                case "08":
                    SetVisibility("Fixed");

                    MatchesForValidPattern.Text = matches.ToString();
                    PR1.Text = bytes[3].ToString();
                    PR1Tolerance.Text = bytes[4].ToString();
                    PR2.Text = bytes[5].ToString();
                    PR2Tolerance.Text = bytes[6].ToString();

                    pulseRate1 = bytes[3];
                    pulseRateTolerance1 = bytes[4];
                    pulseRate2 = bytes[5];
                    pulseRateTolerance2 = bytes[6];
                    pulseRate3 = bytes[7];
                    pulseRateTolerance3 = bytes[8];
                    pulseRate4 = bytes[9];
                    pulseRateTolerance4 = bytes[10];
                    break;
                case "07":
                    SetVisibility("Variable");

                    MatchesForValidPattern.Text = matches.ToString();
                    MaxPulseRate.Text = bytes[3].ToString();
                    MinPulseRate.Text = bytes[5].ToString();
                    OptionalDataCalculation.Text = Converters.GetHexValue(bytes[11]).Equals("06") ? "Temperature" : "None";

                    maxPulseRate = bytes[3];
                    minPulseRate = bytes[5];
                    optionalData = bytes[11];
                    break;
            }
            OriginalData = new Dictionary<string, object>
            {
                { ValueCodes.PULSE_RATE_TYPE, pulseRateType },
                { ValueCodes.MATCHES, matches },
                { ValueCodes.PULSE_RATE_1, pulseRate1 },
                { ValueCodes.PULSE_RATE_2, pulseRate2 },
                { ValueCodes.PULSE_RATE_3, pulseRate3 },
                { ValueCodes.PULSE_RATE_4, pulseRate4 },
                { ValueCodes.PULSE_RATE_TOLERANCE_1, pulseRateTolerance1 },
                { ValueCodes.PULSE_RATE_TOLERANCE_2, pulseRateTolerance2 },
                { ValueCodes.PULSE_RATE_TOLERANCE_3, pulseRateTolerance3 },
                { ValueCodes.PULSE_RATE_TOLERANCE_4, pulseRateTolerance4 },
                { ValueCodes.MAX_PULSE_RATE, maxPulseRate },
                { ValueCodes.MIN_PULSE_RATE, minPulseRate },
                { ValueCodes.DATA_CALCULATION, optionalData }
            };
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (ExistChanges())
            {
                if (IsDataCorrect())
                {
                    byte[] b = new byte[12];
                    switch (PulseRateType.Text)
                    {
                        case "Non Coded (Fixed Pulse Rate)":
                            b = new byte[] { 0x47, 0x08, (byte) int.Parse(MatchesForValidPattern.Text), (byte) int.Parse(PR1.Text),
                                (byte) int.Parse(PR1Tolerance.Text), (byte) int.Parse(PR2.Text), (byte) int.Parse(PR2Tolerance.Text),
                                0, 0, 0, 0, 0 };
                            break;
                        case "Non Coded (Variable Pulse Rate)":
                            int optionalData = OptionalDataCalculation.Text.Equals("Temperature") ? 6 : 0;
                            b = new byte[] { 0x47, 0x07, (byte) int.Parse(MatchesForValidPattern.Text),
                                (byte) int.Parse(MaxPulseRate.Text), 0,
                                (byte) int.Parse(MinPulseRate.Text), 0, 0, 0, 0, 0, (byte) optionalData };
                            break;
                        case "Coded":
                            b = new byte[] { 0x47, 0x09, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            break;
                    }

                    bool result = await TransferBLEData.WriteDetectionFilter(b);
                    if (!result) return;
                    if (ReturnDetection)
                    {
                        byte[] value = await TransferBLEData.ReadDetectionFilter();
                        while (value == null)
                            value = await TransferBLEData.ReadDetectionFilter();
                        MessagingCenter.Send(value, ValueCodes.DETECTION_SCAN);
                    }
                }
                else
                {
                    await DisplayAlert("Message", "Data incorrect.", "OK");
                    return;
                }
            }
            await Navigation.PopModalAsync(false);
        }

        private async void PulseRateType_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.PULSE_RATE_TYPE_CODE), false);
        }

        private async void MatchesForValidPattern_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.MATCHES_FOR_VALID_PATTERN), false);
        }

        private async void MaxPulseRate_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.MAX_PULSE_RATE_CODE), false);
        }

        private async void MinPulseRate_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.MIN_PULSE_RATE_CODE), false);
        }

        private async void OptionalDataCalculation_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.DATA_CALCULATION_TYPES), false);
        }

        private async void PR1_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.PULSE_RATE_1_CODE), false);
        }

        private async void PR2_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.PULSE_RATE_2_CODE), false);
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "Coded":
                    PulseRateType.Text = "Coded";
                    Matches.IsVisible = false;
                    PulseRates.IsVisible = false;
                    MaxMinPR.IsVisible = false;
                    break;
                case "Fixed":
                    PulseRateType.Text = "Non Coded (Fixed Pulse Rate)";
                    Matches.IsVisible = true;
                    PulseRates.IsVisible = true;
                    MaxMinPR.IsVisible = false;
                    MatchesForValidPattern.Text = "3";
                    PR1.Text = "0";
                    PR1Tolerance.Text = "0";
                    PR2.Text = "0";
                    PR2Tolerance.Text = "0";
                    break;
                case "Variable":
                    PulseRateType.Text = "Non Coded (Variable Pulse Rate)";
                    Matches.IsVisible = true;
                    PulseRates.IsVisible = false;
                    MaxMinPR.IsVisible = true;
                    MatchesForValidPattern.Text = "3";
                    MaxPulseRate.Text = "0";
                    MinPulseRate.Text = "0";
                    OptionalDataCalculation.Text = "None";
                    break;
            }
        }

        private bool ExistChanges()
        {
            byte pulseRateType = 0;
            int matches = MatchesForValidPattern.Text.Equals("") ? 0 : int.Parse(MatchesForValidPattern.Text);
            int pulseRate1 = 0;
            int pulseRate2 = 0;
            int pulseRate3 = 0;
            int pulseRate4 = 0;
            int pulseRateTolerance1 = 0;
            int pulseRateTolerance2 = 0;
            int pulseRateTolerance3 = 0;
            int pulseRateTolerance4 = 0;
            int maxPulseRate = 0;
            int minPulseRate = 0;
            int optionalData = 0;
            switch (PulseRateType.Text)
            {
                case "Non Coded (Fixed Pulse Rate)":
                    pulseRateType = 0x08;
                    pulseRate1 = int.Parse(PR1.Text);
                    pulseRate2 = int.Parse(PR2.Text);
                    pulseRateTolerance1 = int.Parse(PR1Tolerance.Text);
                    pulseRateTolerance2 = int.Parse(PR2Tolerance.Text);
                    break;
                case "Non Coded (Variable Pulse Rate)":
                    pulseRateType = 0x07;
                    maxPulseRate = int.Parse(MaxPulseRate.Text);
                    minPulseRate = int.Parse(MinPulseRate.Text);
                    optionalData = OptionalDataCalculation.Text.Equals("Temperature") ? 6 : 0;
                    break;
                case "Coded":
                    pulseRateType = 0x09;
                    break;
            }

            return (int)OriginalData[ValueCodes.PULSE_RATE_TYPE] != (int)pulseRateType || (int)OriginalData[ValueCodes.MATCHES] != matches
                || (int)OriginalData[ValueCodes.PULSE_RATE_1] != pulseRate1 || (int)OriginalData[ValueCodes.PULSE_RATE_2] != pulseRate2
                || (int)OriginalData[ValueCodes.PULSE_RATE_3] != pulseRate3 || (int)OriginalData[ValueCodes.PULSE_RATE_4] != pulseRate4
                || (int)OriginalData[ValueCodes.PULSE_RATE_TOLERANCE_1] != pulseRateTolerance1 || (int)OriginalData[ValueCodes.PULSE_RATE_TOLERANCE_2] != pulseRateTolerance2
                || (int)OriginalData[ValueCodes.PULSE_RATE_TOLERANCE_3] != pulseRateTolerance3 || (int)OriginalData[ValueCodes.PULSE_RATE_TOLERANCE_4] != pulseRateTolerance4
                || (int)OriginalData[ValueCodes.MAX_PULSE_RATE] != maxPulseRate || (int)OriginalData[ValueCodes.MIN_PULSE_RATE] != minPulseRate
                || (int)OriginalData[ValueCodes.DATA_CALCULATION] != optionalData;
        }

        private bool IsDataCorrect()
        {
            if (PulseRateType.Text.Equals("Non Coded (Fixed Pulse Rate)"))
                return !PR1.Text.Equals("0");
            else if (PulseRateType.Text.Equals("Non Coded (Variable Pulse Rate)"))
            {
                int max = int.Parse(MaxPulseRate.Text);
                int min = int.Parse(MinPulseRate.Text);
                return (max > 0 && max <= 240) && (min > 0 && min <= 240) && (max > min);
            }
            return true;
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
