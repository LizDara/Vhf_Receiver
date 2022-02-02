using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;

using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class ReceiverDisconnected : PopupPage
    {
        public ReceiverDisconnected()
        {
            InitializeComponent();

            Task.Delay(5000).ContinueWith(t => App.Current.MainPage.Navigation.PopPopupAsync(true));
        }
    }
}
