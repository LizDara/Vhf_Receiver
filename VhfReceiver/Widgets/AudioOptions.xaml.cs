using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;

using Xamarin.Forms;
using VhfReceiver.Utils;

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
            byte[] b;
            string description;
            if (Converters.GetHexValue(Option).Equals("59"))
            {
                description = "Single (" + NewDigit.Text + ")";
                b = new byte[] { Option, (byte)int.Parse(NewDigit.Text), Background.IsToggled ? (byte)1 : (byte)0 };
            }
            else
            {
                description = Converters.GetHexValue(Option).Equals("5A") ? "All" : "None";
                b = new byte[] { Option, Background.IsToggled ? (byte)1 : (byte)0 };
            }

            bool result = await TransferBLEData.WriteScanning(b);
            if (result)
            {
                await App.Current.MainPage.Navigation.PopPopupAsync();
                MessagingCenter.Send(description, ValueCodes.VALUE);
            }
        }
    }
}