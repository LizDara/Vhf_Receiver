using System;
using System.Threading.Tasks;
using VhfReceiver.Utils;
using Xamarin.Forms;
using VhfReceiver.Widgets;
using Rg.Plugins.Popup.Extensions;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Pages.VHF;

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
                var popMessage = new DetectionFilter();
                _ = App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
            }
        }

        private void Initialize()
        {
            InitializeComponent();

            DisconnectMenu.SetData(false);
            ReceiverInformation = ReceiverInformation.GetInstance();
            DeviceName.Text = "Receiver " + ReceiverInformation.GetSerialNumber();
            SetBattery();
            SetSDCardState();
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery
        }

        private async void StartScanning_Clicked(object sender, EventArgs e)
        {
            var detection = await TransferBLEData.ReadDetectionFilter();
            var tables = await TransferBLEData.ReadTables();
            if (detection != null && tables != null)
                await Navigation.PushModalAsync(new StartScanningPage(detection, tables), false);
        }

        private async void ReceiverConfiguration_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ReceiverConfigurationPage(), false);
        }

        private async void ManageReceiverData_Tapped(object sender, EventArgs e)
        {
            var bytes = await TransferBLEData.ReadDataInfo();
            if (bytes != null)
                await Navigation.PushModalAsync(new ManageReceiverDataPage(bytes), false);
        }

        private async void ConvertRawData_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new RawDataPage(), false);
        }

        private async void TestReceiver_Tapped(object sender, EventArgs e)
        {
            var bytes = await TransferBLEData.ReadDiagnostic();
            if (bytes != null)
            {
                var popMessage = new RunningTest();
                await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

                await Navigation.PushModalAsync(new TestReceiverPage(bytes), false);
                await Task.Delay(ValueCodes.BRANDING_PERIOD);
            }
        }

        private void ValueUpdateState(object o, CharacteristicUpdatedEventArgs args)
        {
            var value = args.Characteristic.Value;
            if (Converters.GetHexValue(value[0]).Equals("56"))
            {
                ReceiverInformation.ChangeSDCard(Converters.GetHexValue(value[1]).Equals("80"));
                SetSDCardState();
            }
            else if (Converters.GetHexValue(value[0]).Equals("88"))
            {
                ReceiverInformation.ChangeDeviceBattery(value[1]);
                SetBattery();
            }
        }

        private void SetSDCardState()
        {
            SDCard.Text = ReceiverInformation.IsSDCardInserted() ? "Inserted" : "None";
            SDCardImage.Source = ReceiverInformation.IsSDCardInserted() ? "SDCard" : "NoSDCard";
        }

        private void SetBattery()
        {
            DeviceBattery.Text = ReceiverInformation.GetDeviceBattery() + "%";
            BatteryImage.Source = ReceiverInformation.GetDeviceBattery() > 20 ? "FullBattery" : "EmptyBattery";
        }
    }
}
