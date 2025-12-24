using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;

namespace VhfReceiver.Widgets
{
    public partial class ReceiverDisconnected : PopupPage
    {
        public ReceiverDisconnected(string message)
        {
            InitializeComponent();
            BindingContext = this;
            Message.Text = message;

            Task.Delay(ValueCodes.DISCONNECTION_MESSAGE_PERIOD).ContinueWith(t => App.Current.MainPage.Navigation.PopPopupAsync(true));
        }
    }
}
