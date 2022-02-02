using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class NoReceiversFoundPage : ContentPage
    {
        public NoReceiversFoundPage()
        {
            InitializeComponent();
        }

        private async void Retry_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new SearchingDevicesPage());
        }
    }
}
