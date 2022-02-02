using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VhfReceiver
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new Pages.SearchingDevicesPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
