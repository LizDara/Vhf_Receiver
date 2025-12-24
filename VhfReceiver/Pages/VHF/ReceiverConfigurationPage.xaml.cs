using System;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class ReceiverConfigurationPage : ContentPage
    {
        public ReceiverConfigurationPage()
        {
            InitializeComponent();
            Toolbar.SetData("Receiver Configuration", true);
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery
        }

        private async void EditFrequencyTables_Tapped(object sender, EventArgs e)
        {
            var bytes = await TransferBLEData.ReadTables();
            if (bytes != null)
                await Navigation.PushModalAsync(new TablesPage(bytes, false), false);
        }

        private async void EditReceiverDetails_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new EditReceiverDefaultsPage(), false);
        }

        private async void SetDetectionFilter_Tapped(object sender, EventArgs e)
        {
            var bytes = await TransferBLEData.ReadDetectionFilter();
            if (bytes != null)
                await Navigation.PushModalAsync(new SelectDetectionFilterPage(bytes, false), false);
        }

        private async void CloneFromOtherReceiver_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new CloneReceiverPage(), false);
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
