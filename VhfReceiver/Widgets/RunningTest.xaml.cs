using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;
using System.Threading.Tasks;

namespace VhfReceiver.Widgets
{
    public partial class RunningTest : PopupPage
    {
        private bool isVisibleLoading;
        public bool IsVisibleLoading
        {
            set
            {
                isVisibleLoading = value;
                OnPropertyChanged(nameof(IsVisibleLoading));
            }
            get { return isVisibleLoading; }
        }
        private bool isVisibleChecked;
        public bool IsVisibleChecked
        {
            set
            {
                isVisibleChecked = value;
                OnPropertyChanged(nameof(IsVisibleChecked));
            }
            get { return isVisibleChecked; }
        }
        private string message;
        public string Message
        {
            set
            {
                message = value;
                OnPropertyChanged(nameof(Message));
            }
            get { return message; }
        }
        public RunningTest()
        {
            InitializeComponent();
            BindingContext = this;
            IsVisibleLoading = true;
            IsVisibleChecked = false;
            Message = "Runnning Diagnostics...";

            Task.Delay(2000).ContinueWith(t => { IsVisibleLoading = false; IsVisibleChecked = true; Message = "Diagnostics Complete"; });
            Task.Delay(4000).ContinueWith(t => App.Current.MainPage.Navigation.PopPopupAsync(true));
        }
    }
}
