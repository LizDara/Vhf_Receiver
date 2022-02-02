using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class RunningTest : PopupPage
    {
        public RunningTest()
        {
            InitializeComponent();

            Task.Delay(5000).ContinueWith(t => App.Current.MainPage.Navigation.PopPopupAsync(true));
        }
    }
}
