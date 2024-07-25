using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;

namespace VhfReceiver.Widgets
{
    public partial class ReceiverDisconnected : PopupPage
    {
        public ReceiverDisconnected()
        {
            InitializeComponent();

            Task.Delay(1500).ContinueWith(t => App.Current.MainPage.Navigation.PopPopupAsync(true));
        }
    }
}
