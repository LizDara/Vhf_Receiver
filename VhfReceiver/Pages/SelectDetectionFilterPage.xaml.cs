using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class SelectDetectionFilterPage : ContentPage
    {
        private readonly ReceiverInformation ReceiverInformation;
        private Dictionary<string, object> OriginalData;
        private readonly byte[] DetectionFilterBytes;

        public SelectDetectionFilterPage(byte[] bytes)
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();

            DetectionFilterBytes = bytes;
            SetData(bytes);

            MessagingCenter.Subscribe<int[]>(this, ValueCodes.VALUE, (value) => {
                switch (value[0])
                {
                    case ValueCodes.PULSE_RATE_TYPE:
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
                    case ValueCodes.MAX_PULSE_RATE:
                        MaxPulseRate.Text = value[1].ToString();
                        break;
                    case ValueCodes.MIN_PULSE_RATE:
                        MinPulseRate.Text = value[1].ToString();
                        break;
                    case ValueCodes.DATA_CALCULATION_TYPES:
                        if (value[1] == ValueCodes.NONE)
                            OptionalDataCalculation.Text = "None";
                        else if (value[1] == ValueCodes.TEMPERATURE)
                            OptionalDataCalculation.Text = "Temperature";
                        else if (value[1] == ValueCodes.PERIOD)
                            OptionalDataCalculation.Text = "Period";
                        break;
                    case ValueCodes.PULSE_RATE_1:
                        PR1.Text = (value[1] / 100).ToString();
                        PR1Tolerance.Text = (value[1] % 100).ToString();
                        break;
                    case ValueCodes.PULSE_RATE_2:
                        PR2.Text = (value[1] / 100).ToString();
                        PR2Tolerance.Text = (value[1] % 100).ToString();
                        break;
                    case ValueCodes.PULSE_RATE_3:
                        PR3.Text = (value[1] / 100).ToString();
                        PR3Tolerance.Text = (value[1] % 100).ToString();
                        break;
                    case ValueCodes.PULSE_RATE_4:
                        PR4.Text = (value[1] / 100).ToString();
                        PR4Tolerance.Text = (value[1] % 100).ToString();
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
            switch (Converters.GetHexValue(bytes[1]))
            {
                case "09":
                    SetVisibility("Coded");
                    break;
                case "08":
                    SetVisibility("Fixed");

                    MatchesForValidPattern.Text = bytes[2].ToString();
                    PR1.Text = bytes[3].ToString();
                    PR1Tolerance.Text = bytes[4].ToString();
                    PR2.Text = bytes[5].ToString();
                    PR2Tolerance.Text = bytes[6].ToString();
                    PR3.Text = bytes[7].ToString();
                    PR3Tolerance.Text = bytes[8].ToString();
                    PR4.Text = bytes[9].ToString();
                    PR4Tolerance.Text = bytes[10].ToString();

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

                    MatchesForValidPattern.Text = bytes[2].ToString();
                    maxPulseRate = (bytes[3] * 256) + bytes[4];
                    minPulseRate = (bytes[5] * 256) + bytes[6];
                    MaxPulseRate.Text = maxPulseRate.ToString();
                    MinPulseRate.Text = minPulseRate.ToString();
                    OptionalDataCalculation.Text = "None";
                    break;
            }
            OriginalData = new Dictionary<string, object>
            {
                { "PulseRateType", pulseRateType },
                { "Matches", matches },
                { "PulseRate1", pulseRate1 },
                { "PulseRate2", pulseRate2 },
                { "PulseRate3", pulseRate3 },
                { "PulseRate4", pulseRate4 },
                { "PulseRateTolerance1", pulseRateTolerance1 },
                { "PulseRateTolerance2", pulseRateTolerance2 },
                { "PulseRateTolerance3", pulseRateTolerance3 },
                { "PulseRateTolerance4", pulseRateTolerance4 },
                { "MaxPulseRate", maxPulseRate },
                { "MinPulseRate", minPulseRate }
            };
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (ExistChanges())
                {
                    var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                    if (service != null)
                    {
                        var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_TX_TYPE);

                        if (characteristic != null)
                        {
                            byte txType;
                            byte[] b = new byte[11];
                            switch (PulseRateType.Text)
                            {
                                case "Non Coded (Fixed Pulse Rate)":
                                    txType = 0x08;
                                    b = new byte[] { 0x47, txType, (byte) int.Parse(MatchesForValidPattern.Text), (byte) int.Parse(PR1.Text),
                                    (byte)(!PR1.Text.Equals("0") ? int.Parse(PR1Tolerance.Text) : 0), (byte) int.Parse(PR2.Text),
                                    (byte)(!PR2.Text.Equals("0") ? int.Parse(PR2Tolerance.Text) : 0), (byte) int.Parse(PR3.Text),
                                    (byte)(!PR3.Text.Equals("0") ? int.Parse(PR3Tolerance.Text) : 0), (byte) int.Parse(PR4.Text),
                                    (byte)(!PR4.Text.Equals("0") ? int.Parse(PR4Tolerance.Text) : 0) };
                                    break;
                                case "Non Coded (Variable Pulse Rate)":
                                    txType = 0x07;
                                    b = new byte[] { 0x47, txType, (byte) int.Parse(MatchesForValidPattern.Text),
                                    (byte) (int.Parse(MaxPulseRate.Text) / 256), (byte) (int.Parse(MaxPulseRate.Text) % 256),
                                    (byte) (int.Parse(MinPulseRate.Text) /256), (byte) (int.Parse(MinPulseRate.Text) % 256), 0, 0, 0, 0 };
                                    break;
                                case "Coded":
                                    txType = 0x09;
                                    b = new byte[] { 0x47, txType, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                                    break;
                            }

                            bool result = await characteristic.WriteAsync(b);
                            if (result)
                            {
                                ReceiverInformation.ChangeTxType(b[1]);
                                MessagingCenter.Send("Changing", ValueCodes.TX_TYPE);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
                await Navigation.PopModalAsync(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Service: " + ex.Message);
            }
        }

        private async void PulseRateType_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.PULSE_RATE_TYPE), false);
        }

        private async void MatchesForValidPattern_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.MATCHES_FOR_VALID_PATTERN), false);
        }

        private async void MaxPulseRate_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.MAX_PULSE_RATE), false);
        }

        private async void MinPulseRate_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.MIN_PULSE_RATE), false);
        }

        private async void OptionalDataCalculation_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.DATA_CALCULATION_TYPES), false);
        }

        private async void PR1_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.PULSE_RATE_1), false);
        }

        private async void PR2_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.PULSE_RATE_2), false);
        }

        private async void PR3_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.PULSE_RATE_3), false);
        }

        private async void PR4_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDetectionFilterPage(DetectionFilterBytes, ValueCodes.PULSE_RATE_4), false);
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
                    break;
                case "Variable":
                    PulseRateType.Text = "Non Coded (Variable Pulse Rate)";
                    Matches.IsVisible = true;
                    PulseRates.IsVisible = false;
                    MaxMinPR.IsVisible = true;
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
            switch (PulseRateType.Text)
            {
                case "Non Coded (Fixed Pulse Rate)":
                    pulseRateType = 0x08;
                    pulseRate1 = int.Parse(PR1.Text);
                    pulseRate2 = int.Parse(PR2.Text);
                    pulseRate3 = int.Parse(PR3.Text);
                    pulseRate4 = int.Parse(PR4.Text);
                    pulseRateTolerance1 = int.Parse(PR1Tolerance.Text);
                    pulseRateTolerance2 = int.Parse(PR2Tolerance.Text);
                    pulseRateTolerance3 = int.Parse(PR3Tolerance.Text);
                    pulseRateTolerance4 = int.Parse(PR4Tolerance.Text);
                    break;
                case "Non Coded (Variable Pulse Rate)":
                    pulseRateType = 0x07;
                    maxPulseRate = int.Parse(MaxPulseRate.Text);
                    minPulseRate = int.Parse(MinPulseRate.Text);
                    break;
                case "Coded":
                    pulseRateType = 0x09;
                    break;
            }

            return (int)OriginalData["PulseRateType"] != (int)pulseRateType || (int)OriginalData["Matches"] != matches
                || (int)OriginalData["PulseRate1"] != pulseRate1 || (int)OriginalData["PulseRate2"] != pulseRate2
                || (int)OriginalData["PulseRate3"] != pulseRate3 || (int)OriginalData["PulseRate4"] != pulseRate4
                || (int)OriginalData["PulseRateTolerance1"] != pulseRateTolerance1 || (int)OriginalData["PulseRateTolerance2"] != pulseRateTolerance2
                || (int)OriginalData["PulseRateTolerance3"] != pulseRateTolerance3 || (int)OriginalData["PulseRateTolerance4"] != pulseRateTolerance4
                || (int)OriginalData["MaxPulseRate"] != maxPulseRate || (int)OriginalData["MinPulseRate"] != minPulseRate;
        }
    }
}
