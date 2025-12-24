using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;

namespace VhfReceiver.Widgets
{
    public partial class ViewDetectionFilter : PopupPage
    {
        public ViewDetectionFilter(byte[] value)
        {
            InitializeComponent();

            SetData(value);
        }

        private void SetData(byte[] value)
        {
            string detection = Converters.GetHexValue(value[18]).Equals("08") ? "Fixed Pulse Rate" : "Variable Pulse Rate";
            string dataCalculation = "";
            switch (Converters.GetHexValue(value[18]))
            {
                case "06":
                    dataCalculation = "Yes";
                    break;
                case "07":
                    dataCalculation = "None";
                    break;
            }
            string matches = Converters.GetDecimalValue(value[19]);
            string pr1 = Converters.GetDecimalValue(value[20]);
            string pr1Tolerance = Converters.GetDecimalValue(value[21]);
            string pr2 = Converters.GetDecimalValue(value[22]);
            string pr2Tolerance = Converters.GetDecimalValue(value[23]);
            FilterType.Text = detection;
            PR1.Text = pr1;
            PR1Tolerance.Text = pr1Tolerance;
            PR2.Text = pr2;
            PR2Tolerance.Text = pr2Tolerance;
            Matches.Text = matches;
            if (dataCalculation.Equals(""))
            {
                OptionalDataCalculation.IsVisible = false;
                DataCalculation.IsVisible = false;
            }
            else
            {
                PulseRate2.IsVisible = false;
                PR2.IsVisible = false;
                PulseRate2Tolerance.IsVisible = false;
                PR2Tolerance.IsVisible = false;
                PulseRate1.Text = "Max Pulse Rate (ppm)";
                PulseRate1Tolerance.Text = "Min Pulse Rate (ppm)";
                DataCalculation.Text = dataCalculation;
            }
        }

        private async void Close_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopPopupAsync();
        }
    }
}
