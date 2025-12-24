using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VhfReceiver.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class BridgePage : ContentPage
    {
        private List<DeviceCategory> deviceCategories;
        public BridgePage()
        {
            InitializeComponent();
            BindingContext = this;

            SetCategories();
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (!await PermissionsGrantedAsync())
                {
                    await DisplayAlert("Permission required.", "Application needs location permission.", "OK");
                    await Navigation.PopModalAsync();
                }
            });
        }

        private void SetCategories()
        {
            Toolbar.SetData("BRIDGE APP", false, Back_Clicked);
            Subtitle.Text = "Device Selection";
            Message.Text = "What type of device are you using? Please select from the categories below.";
            Title.Text = "Device Categories";
            deviceCategories = new List<DeviceCategory>();
            deviceCategories.Add(new DeviceCategory("VHF Receivers", "List of supported models."));
            deviceCategories.Add(new DeviceCategory("Acoustic Receivers", "List of supported models."));
            deviceCategories.Add(new DeviceCategory("Bluetooth Tags", "List of supported models."));
            deviceCategories.Add(new DeviceCategory("Wildlink", "List of supported models."));
            CategoriesList.ItemsSource = deviceCategories;
        }

        private void SetModes()
        {
            Toolbar.SetData("BLUETOOTH TAGS", false);
            Subtitle.Text = "How are you going to receive data?";
            Message.Text = "Bluetooth tags give you the option of connecting to a receiver or listening to tag transmissions directly.";
            Title.Text = "Connection Modes";
            deviceCategories = new List<DeviceCategory>();
            deviceCategories.Add(new DeviceCategory("Connect to Bluetooth Receiver", "Brief Description."));
            deviceCategories.Add(new DeviceCategory("View Beacon Tags Directly", "Brief Description."));
            CategoriesList.ItemsSource = deviceCategories;

        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (deviceCategories.Count == 4)
                await Navigation.PopModalAsync();
            else
                SetCategories();
        }

        private async void CategoriesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            DeviceCategory category = (DeviceCategory)e.Item;
            switch (category.Name)
            {
                case "VHF Receivers":
                case "Acoustic Receivers":
                case "Wildlink":
                case "Connect to Bluetooth Receiver":
                case "View Beacon Tags Directly":
                    Console.WriteLine("BEFORE NAVIGATION TO SEARCHING DEVICES");
                    await Navigation.PushModalAsync(new SearchingDevicesPage(category.Name), false);
                    Console.WriteLine("AFTER NAVIGATION TO SEARCHING DEVICES");
                    break;
                case "Bluetooth Tags":
                    SetModes();
                    break;
                    
            }
        }

        private async Task<bool> PermissionsGrantedAsync()
        {
            var locationPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            var storageWritePermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            var storageReadPermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            Console.WriteLine("Location: " + locationPermissionStatus + ", Write: " + storageWritePermissionStatus + ", Read: " + storageReadPermissionStatus);

            if (locationPermissionStatus != PermissionStatus.Granted)
                locationPermissionStatus = await Permissions.RequestAsync<Permissions.LocationAlways>();
            if (storageWritePermissionStatus != PermissionStatus.Granted)
                storageWritePermissionStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (storageReadPermissionStatus != PermissionStatus.Granted)
                storageReadPermissionStatus = await Permissions.RequestAsync<Permissions.StorageRead>();

            return locationPermissionStatus == PermissionStatus.Granted && storageWritePermissionStatus == PermissionStatus.Granted
                && storageReadPermissionStatus == PermissionStatus.Granted;
        }
    }
}
