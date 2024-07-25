using System;
using System.Threading.Tasks;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class MobileSettingsPage : ContentPage
    {
        private readonly ReceiverInformation ReceiverInformation;

        public MobileSettingsPage()
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            var bytes = await GetTables();
            if (bytes != null)
                await Navigation.PushModalAsync(new StartScanningPage(bytes), false);
        }

        private async void ScanWithDefaultSettings_Tapped(object sender, EventArgs e)
        {
            byte[] bytes = await GetMobileDefaults();

            if (bytes != null)
                await Navigation.PushModalAsync(new MobileScanningPage(bytes), false);
        }

        private async void ScanWithTemporarySettings_Tapped(object sender, EventArgs e)
        {
            byte[] bytes = await GetMobileDefaults();

            if (bytes != null)
                await Navigation.PushModalAsync(new TemporaryMobilePage(bytes), false);
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

        private async Task<byte[]> GetTables()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_FREQ_TABLE);
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
