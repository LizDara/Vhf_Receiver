using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;
using VhfReceiver.Utils;

namespace VhfReceiver.Widgets
{
    public partial class DetectionFilter : PopupPage
    {
        private byte typeDetection = 9;

        public DetectionFilter()
        {
            InitializeComponent();
        }

        private void Detection_Changed(object sender, CheckedChangedEventArgs e)
        {
            RadioButton detection = sender as RadioButton;
            typeDetection = byte.Parse((string)detection.Value);
        }

        private async void Continue_Clicked(object sender, EventArgs e)
        {
            byte[] b = new byte[11];
            b[0] = 0x47;
            b[1] = typeDetection;
            Console.WriteLine("TX TYPE: " + Converters.GetHexValue(b));
            bool result = await TransferBLEData.WriteDetectionFilter(b);
            if (result)
                await App.Current.MainPage.Navigation.PopPopupAsync();
        }
    }
}