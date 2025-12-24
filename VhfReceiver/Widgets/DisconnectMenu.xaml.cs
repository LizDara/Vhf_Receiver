using System;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class DisconnectMenu : ContentView
    {
        public DisconnectMenu()
        {
            InitializeComponent();
        }

        public void SetData(bool showMenu)
        {
            MenuOption.IsVisible = showMenu;
        }

        private async void Disconnect_Clicked(object sender, EventArgs e)
        {
            ReceiverInformation receiverInformation = ReceiverInformation.GetInstance();
            await receiverInformation.GetAdapter().DisconnectDeviceAsync(receiverInformation.GetDevice());
        }

        private void Menu_Clicked(object sender, EventArgs e)
        {
        }
    }
}
