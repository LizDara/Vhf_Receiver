using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages.BluetoothReceiver
{
    public partial class HomePage : ContentPage
    {
        private readonly ReceiverInformation ReceiverInformation;

        public HomePage()
        {
            InitializeComponent();

            ReceiverInformation = ReceiverInformation.GetInstance();
            DeviceName.Text = ReceiverInformation.GetSerialNumber() + " Bluetooth Receiver";
            Toolbar.SetData("BLUETOOTH BEACON", false, Back_Clicked);
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await ReceiverInformation.GetAdapter().DisconnectDeviceAsync(ReceiverInformation.GetDevice());
        }

        private async void DetectTags_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new TagDetectionPage(), false);
        }
    }
}
