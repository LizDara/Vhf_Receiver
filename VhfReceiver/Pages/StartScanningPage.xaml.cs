using System;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class StartScanningPage : ContentPage
    {
        private byte[] TablesBytes;
        private bool IsEmpty;

        public StartScanningPage(byte[] bytes)
        {
            InitializeComponent();

            TablesBytes = bytes;
            CheckTables();
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new HomePage(), false);
        }

        private async void ManualScan_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ManualScanningPage(), false);
        }
        
        private async void MobileScan_Tapped(object sender, EventArgs e)
        {
            if (IsEmpty)
            {
                Warning.IsVisible = true;
                Menu.IsVisible = false;
            }
            else
                await Navigation.PushModalAsync(new MobileSettingsPage(), false);
        }

        private async void StationaryScan_Tapped(object sender, EventArgs e)
        {
            if (IsEmpty)
            {
                Warning.IsVisible = true;
                Menu.IsVisible = false;
            }
            else
                await Navigation.PushModalAsync(new StationarySettingsPage(), false);
        }

        private async void GoToTables_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<byte[]>(this, "TableScan", (value) =>
            {
                TablesBytes = value;
                CheckTables();
            });

            await Navigation.PushModalAsync(new TablesPage(TablesBytes, true), false);

            Warning.IsVisible = false;
            Menu.IsVisible = true;
        }

        private void CheckTables()
        {
            IsEmpty = TablesBytes[1] == 0 && TablesBytes[2] == 0 && TablesBytes[3] == 0 && TablesBytes[4] == 0 && TablesBytes[5] == 0
                && TablesBytes[6] == 0 && TablesBytes[7] == 0 && TablesBytes[8] == 0 && TablesBytes[9] == 0 && TablesBytes[10] == 0
                && TablesBytes[11] == 0 && TablesBytes[12] == 0;
        }
    }
}
