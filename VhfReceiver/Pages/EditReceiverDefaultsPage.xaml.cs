using System;
using System.Threading.Tasks;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class EditReceiverDefaultsPage : ContentPage
    {
        private readonly ReceiverInformation ReceiverInformation;

        public EditReceiverDefaultsPage()
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(false);
        }

        private async void EditMobileDefaults_Tapped(object sender, EventArgs e)
        {
            var bytes = await GetMobileDefaults();
            if (bytes != null)
                await Navigation.PushModalAsync(new MobileDefaultsPage(bytes), false);
        }

        private async void EditStationaryDefaults_Tapped(object sender, EventArgs e)
        {
            var bytes = await GetStationaryDefaults();
            if (bytes != null)
                await Navigation.PushModalAsync(new StationaryDefaultsPage(bytes), false);
        }

        private async Task<byte[]> GetMobileDefaults()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
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

        private async Task<byte[]> GetStationaryDefaults()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY);
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
