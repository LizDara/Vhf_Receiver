using System;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class ReceiverOptionsPage : ContentPage
    {
        private readonly ReceiverInformation ReceiverInformation;
        private bool isChanged = false;

        public ReceiverOptionsPage()
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (isChanged)
                MessagingCenter.Send("Changing", ValueCodes.TX_TYPE);
            await Navigation.PopModalAsync(false);
        }

        private async void ReceiverConfiguration_Tapped(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<string>(this, ValueCodes.TX_TYPE, (value) =>
            {
                if (value.Equals("Changing"))
                {
                    Receiver.Status = ReceiverInformation.GetDeviceStatus();
                    isChanged = true;
                }
            });
            await Navigation.PushModalAsync(new ReceiverConfigurationPage(), false);
        }

        private async void ManageReceiverData_Tapped(object sender, EventArgs e)
        {
            var bytes = await GetDiagnostic();
            if (bytes != null)
                await Navigation.PushModalAsync(new ManageReceiverDataPage(bytes), false);
        }

        private async void TestReceiver_Tapped(object sender, EventArgs e)
        {
            var bytes = await GetDiagnostic();
            if (bytes != null)
            {
                var popMessage = new RunningTest();
                await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

                await Task.Delay(4000);
                await Navigation.PushModalAsync(new TestReceiverPage(bytes), false);
            }
        }

        private async Task<byte[]> GetDiagnostic()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_DIAGNOSTIC);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_DIAGNOSTIC_INFO);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return null;
        }
    }
}
