using System;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class EditReceiverDefaultsPage : ContentPage
    {
        public EditReceiverDefaultsPage()
        {
            InitializeComponent();
            Toolbar.SetData("Edit Receiver Defaults", true);
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery
        }

        private async void EditMobileDefaults_Tapped(object sender, EventArgs e)
        {
            var bytes = await TransferBLEData.ReadDefaults(true);
            if (bytes != null)
                await Navigation.PushModalAsync(new MobileDefaultsPage(bytes, false), false);
        }

        private async void EditStationaryDefaults_Tapped(object sender, EventArgs e)
        {
            var bytes = await TransferBLEData.ReadDefaults(false);
            if (bytes != null)
                await Navigation.PushModalAsync(new StationaryDefaultsPage(bytes, false), false);
        }

        private void ValueUpdateState(object o, CharacteristicUpdatedEventArgs args)
        {
            var value = args.Characteristic.Value;
            if (Converters.GetHexValue(value[0]).Equals("56"))
            {
                ReceiverInformation.GetInstance().ChangeSDCard(Converters.GetHexValue(value[1]).Equals("80"));
                ReceiverStatus.UpdateSDCardState();
            }
            else if (Converters.GetHexValue(value[0]).Equals("88"))
            {
                ReceiverInformation.GetInstance().ChangeDeviceBattery(value[1]);
                ReceiverStatus.UpdateBattery();
            }
        }
    }
}
