using System;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class CloneReceiverPage : ContentPage
    {
        private ReceiverInformation ReceiverInformation;

        public CloneReceiverPage()
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();

            CheckReceiversDetected();
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(false);
        }

        private void CheckReceiversDetected()
        {//Check if there is some receiver available to connect to clone that device
            NoReceiversDetected.IsVisible = true;
        }
    }
}
