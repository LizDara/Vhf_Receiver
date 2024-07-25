using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;
using VhfReceiver.Utils;
using VhfReceiver.Pages;

namespace VhfReceiver.Widgets
{
    public partial class DetectionFilter : PopupPage
    {
        readonly ReceiverInformation ReceiverInformation;
        private byte typeDetection = 9;

        public DetectionFilter()
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();
        }

        private void Detection_Changed(object sender, CheckedChangedEventArgs e)
        {
            RadioButton detection = sender as RadioButton;
            typeDetection = byte.Parse((string)detection.Value);
        }

        private async void Continue_Clicked(object sender, EventArgs e)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_TX_TYPE);
                    if (characteristic != null)
                    {
                        byte[] b = new byte[11];
                        b[0] = 0x47;
                        b[1] = typeDetection;
                        Console.WriteLine("TX TYPE: "+Converters.GetHexValue(b));

                        bool result = await characteristic.WriteAsync(b);
                        if (result)
                        {
                            MessagingCenter.Send(Converters.GetHexValue(typeDetection), "DetectionType");
                            await App.Current.MainPage.Navigation.PopPopupAsync();
                        }
                    }
                }
            }
            catch
            {
                await App.Current.MainPage.Navigation.PopPopupAsync();

                var popMessage = new ReceiverDisconnected();
                await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
                await Navigation.PushModalAsync(new SearchingDevicesPage());
            }
        }
    }
}
