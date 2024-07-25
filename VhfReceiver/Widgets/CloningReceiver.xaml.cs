using System.Threading.Tasks;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;

namespace VhfReceiver.Widgets
{
    public partial class CloningReceiver : PopupPage
    {
        public CloningReceiver()
        {
            InitializeComponent();

            Task.Delay(3000).ContinueWith(t => App.Current.MainPage.Navigation.PopPopupAsync(true));
        }
    }
}
