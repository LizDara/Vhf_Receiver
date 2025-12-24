using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class IDAudio : ContentView
    {
        public IDAudio()
        {
            InitializeComponent();
        }

        public void SetData(string audioDescription)
        {
            Audio.Text = audioDescription;
        }

        private async void EditAudio_Tapped(object sender, EventArgs e)
        {
            var popMessage = new AudioOptions();
            await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
        }
    }
}
