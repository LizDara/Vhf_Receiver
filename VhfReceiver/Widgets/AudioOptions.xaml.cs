using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;

using Xamarin.Forms;
using VhfReceiver.Utils;
using VhfReceiver.Pages;

namespace VhfReceiver.Widgets
{
    public partial class AudioOptions : PopupPage
    {
        private byte Option = 0x5A;

        public AudioOptions()
        {
            InitializeComponent();
        }

        private async void Close_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopPopupAsync();
        }

        private void Single_Clicked(object sender, EventArgs e)
        {
            Single.TextColor = Color.FromHex("F5F7FA");
            Single.BackgroundColor = Color.FromHex("3E4C59");
            All.TextColor = Color.FromHex("3E4C59");
            All.BackgroundColor = Color.FromHex("F5F7FA");
            None.TextColor = Color.FromHex("3E4C59");
            None.BackgroundColor = Color.FromHex("F5F7FA");
            EnterDigit.IsVisible = true;
            NewDigit.Text = "";
            Option = 0x59;
        }

        private void All_Clicked(object sender, EventArgs e)
        {
            Single.TextColor = Color.FromHex("3E4C59");
            Single.BackgroundColor = Color.FromHex("F5F7FA");
            All.TextColor = Color.FromHex("F5F7FA");
            All.BackgroundColor = Color.FromHex("3E4C59");
            None.TextColor = Color.FromHex("3E4C59");
            None.BackgroundColor = Color.FromHex("F5F7FA");
            EnterDigit.IsVisible = false;
            Option = 0x5A;
        }

        private void None_Clicked(object sender, EventArgs e)
        {
            Single.TextColor = Color.FromHex("3E4C59");
            Single.BackgroundColor = Color.FromHex("F5F7FA");
            All.TextColor = Color.FromHex("3E4C59");
            All.BackgroundColor = Color.FromHex("F5F7FA");
            None.TextColor = Color.FromHex("F5F7FA");
            None.BackgroundColor = Color.FromHex("3E4C59");
            EnterDigit.IsVisible = false;
            Option = 0x5B;
        }

        private void Number_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string text = NewDigit.Text;
            NewDigit.Text = text + button.Text;
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            if (!NewDigit.Text.Equals(""))
            {
                string text = NewDigit.Text;
                NewDigit.Text = text.Substring(0, text.Length - 1);
            }
        }

        private async void SaveChanges_Clicked(object sender, EventArgs e)
        {
            ReceiverInformation receiverInformation = ReceiverInformation.GetReceiverInformation();
            try
            {
                var service = await receiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_TX_TYPE);
                    if (characteristic != null)
                    {
                        byte[] b;
                        if (Converters.GetHexValue(Option).Equals("59"))
                            b = new byte[] { Option, (byte)int.Parse(NewDigit.Text), Background.IsToggled ? (byte)1 : (byte)0 };
                        else
                            b = new byte[] { Option, Background.IsToggled ? (byte)1 : (byte)0 };

                        bool result = await characteristic.WriteAsync(b);
                        if (result)
                            await App.Current.MainPage.Navigation.PopPopupAsync();
                    }
                }
            }
            catch
            {
                await App.Current.MainPage.Navigation.PopPopupAsync();

                var popMessage = new ReceiverDisconnected();
                await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
                await Navigation.PushModalAsync(new SearchingDevicesPage());
            }
        }
    }
}