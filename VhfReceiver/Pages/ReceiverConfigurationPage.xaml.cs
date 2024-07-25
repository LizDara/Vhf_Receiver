using System;
using System.Threading.Tasks;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class ReceiverConfigurationPage : ContentPage
    {
        private readonly ReceiverInformation ReceiverInformation;
        private bool isChanged = false;

        public ReceiverConfigurationPage()
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

        private async void EditFrequencyTables_Tapped(object sender, EventArgs e)
        {
            var bytes = await GetTables();
            if (bytes != null)
                await Navigation.PushModalAsync(new TablesPage(bytes, false), false);
        }

        private async void EditReceiverDetails_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new EditReceiverDefaultsPage(), false);
        }

        private async void SetDetectionFilter_Tapped(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<string>(this, ValueCodes.TX_TYPE, (value) =>
            {
                if (value.Equals("Changing"))
                {
                    Receiver.Status = ReceiverInformation.GetDeviceStatus();
                    isChanged = true;
                }
            });
            var bytes = await GetDetectionFilter();
            if (bytes != null)
                await Navigation.PushModalAsync(new SelectDetectionFilterPage(bytes), false);
        }

        private async void CloneFromOtherReceiver_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new CloneReceiverPage(), false);
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

        private async Task<byte[]> GetDetectionFilter()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_TX_TYPE);
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
