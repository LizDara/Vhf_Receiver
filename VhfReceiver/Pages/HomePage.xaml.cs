using System;
using System.Threading.Tasks;
using VhfReceiver.Utils;
using Xamarin.Forms;
using VhfReceiver.Widgets;
using Rg.Plugins.Popup.Extensions;

namespace VhfReceiver.Pages
{
    public partial class HomePage : ContentPage
    {
        private ReceiverInformation ReceiverInformation;

        public HomePage()
        {
            Initialize();
        }

        public HomePage(byte detectionType)
        {
            Initialize();
            if (detectionType == 0)
            {
                MessagingCenter.Subscribe<string>(this, "DetectionType", (value) =>
                {
                    ReceiverInformation.ChangeTxType(byte.Parse(value));
                    DeviceName.Text = ReceiverInformation.GetDeviceStatus();
                });

                var popMessage = new DetectionFilter();
                _ = App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
            }
        }

        private void Initialize()
        {
            InitializeComponent();

            ReceiverInformation = ReceiverInformation.GetReceiverInformation();
            DeviceName.Text = ReceiverInformation.GetDeviceStatus();
            DeviceBattery.Text = ReceiverInformation.GetDeviceBattery();
        }

        private async void Disconnect_Clicked(object sender, EventArgs e)
        {
            await ReceiverInformation.GetAdapter().DisconnectDeviceAsync(ReceiverInformation.GetDevice());
        }

        private async void StartScanning_Clicked(object sender, EventArgs e)
        {
            var bytes = await GetTables();
            if (bytes != null)
                await Navigation.PushModalAsync(new StartScanningPage(bytes), false);
        }

        private async void ReceiverOptions_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<string>(this, ValueCodes.TX_TYPE, (value) =>
            {
                if (value.Equals("Changing"))
                    DeviceName.Text = ReceiverInformation.GetDeviceStatus();
            });
            await Navigation.PushModalAsync(new ReceiverOptionsPage(), false);
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
