using System;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class CloneReceiverPage : ContentPage
    {
        public CloneReceiverPage()
        {
            InitializeComponent();
            Toolbar.SetData("Clone Receiver", true);

            CheckReceiversDetected();
        }

        private void CheckReceiversDetected()
        {//Check if there is some receiver available to connect to clone that device
            NoReceiversDetected.IsVisible = true;
        }
    }
}
