using System.Threading.Tasks;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using VhfReceiver.Utils;

namespace VhfReceiver.Widgets
{
    public partial class FrequencyMessage : PopupPage
    {
        public FrequencyMessage(string message)
        {
            InitializeComponent();
            StateMessage.Text = message;

            Task.Delay(ValueCodes.MESSAGE_PERIOD).ContinueWith(t => App.Current.MainPage.Navigation.PopPopupAsync(true));
        }
    }
}
