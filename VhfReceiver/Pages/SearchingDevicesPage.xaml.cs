using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using XamarinEssentials = Xamarin.Essentials;
using Plugin.BLE;
using Xamarin.Forms;
using VhfReceiver.Utils;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Widgets;

namespace VhfReceiver.Pages
{
    public partial class SearchingDevicesPage : ContentPage
    {
        private readonly IAdapter BluetoothAdapter;
        private List<IDevice> GattDevices = new List<IDevice>();
        private List<DeviceInformation> DevicesInformation = new List<DeviceInformation>();

        public SearchingDevicesPage()
        {
            InitializeComponent();
            BindingContext = this;

            BluetoothAdapter = CrossBluetoothLE.Current.Adapter;
            BluetoothAdapter.ScanTimeout = 8000;
            BluetoothAdapter.DeviceDiscovered += (sender, foundBLEDevice) =>
            {
                if (foundBLEDevice.Device != null
                    && !string.IsNullOrEmpty(foundBLEDevice.Device.Name)
                    && foundBLEDevice.Device.Name.Contains("ATSvr")
                    && !GattDevices.Contains(foundBLEDevice.Device))
                {
                    Console.WriteLine("Device Found Added: " + foundBLEDevice.Device.Name);
                    GattDevices.Add(foundBLEDevice.Device);
                    DevicesInformation.Add(
                        new DeviceInformation(foundBLEDevice.Device, foundBLEDevice.Device.AdvertisementRecords[2].Data));
                }
            };
        }

        private async Task<bool> PermissionsGrantedAsync()
        {
            var locationPermissionStatus = await XamarinEssentials.Permissions.CheckStatusAsync<XamarinEssentials.Permissions.LocationAlways>();

            if (locationPermissionStatus != XamarinEssentials.PermissionStatus.Granted)
            {
                var status = await XamarinEssentials.Permissions.RequestAsync<XamarinEssentials.Permissions.LocationAlways>();

                return status == XamarinEssentials.PermissionStatus.Granted;
            }
            return true;
        }

        private async void Refresh_Clicked(object sender, EventArgs e)
        {
            LoadingSearch.IsVisible = LoadingSearch.IsRunning = Searching.IsVisible = !(Refresh.IsEnabled = false);
            Refresh.Opacity = 0.6;
            Select.IsVisible = Refresh.IsEnabled;
            DevicesList.ItemsSource = null;
            GattDevices.Clear();
            DevicesInformation.Clear();

            if (!await PermissionsGrantedAsync())
            {
                await DisplayAlert("Permission required.", "Application needs location permission.", "OK");
                LoadingSearch.IsVisible = LoadingSearch.IsRunning = Searching.IsVisible = !(Refresh.IsEnabled = true);
                return;
            }

            await BluetoothAdapter.StartScanningForDevicesAsync();

            if (GattDevices.Count > 0)
            {
                DevicesList.ItemsSource = DevicesInformation;
                LoadingSearch.IsVisible = LoadingSearch.IsRunning = Searching.IsVisible = !(Refresh.IsEnabled = true);
                Refresh.Opacity = 1.0;
                Select.IsVisible = DevicesList.IsVisible = Refresh.IsEnabled;
            }
            else
            {
                await Navigation.PushModalAsync(new NoReceiversFoundPage());
            }
        }

        private async void DevicesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            DeviceInformation selectedItem = e.Item as DeviceInformation;

            try
            {
                SearchingDevices.IsVisible = false;
                LoadingConnect.IsRunning = ConnectingDevice.IsVisible = Status.IsVisible = true;
                DeviceName.Text = selectedItem.Name;
                DeviceRange.Text = selectedItem.Range;
                DeviceBattery.Text = selectedItem.Battery;

                var connectParameters = new ConnectParameters(false, true);
                await BluetoothAdapter.ConnectToDeviceAsync(selectedItem.Device, connectParameters);

                var service = await selectedItem.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_DIAGNOSTIC);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_BOARD_STATUS);
                    if (characteristic != null)
                    {
                        var bytes = await characteristic.ReadAsync();
                        switch (Converters.GetHexValue(bytes[0]))
                        {
                            case "00":
                                await Navigation.PushModalAsync(new HomePage(BluetoothAdapter, selectedItem, bytes));
                                break;
                            case "82":
                                Console.WriteLine("AERIAL SCAN");
                                break;
                            case "83":
                                Console.WriteLine("STATIONARY SCAN");
                                break;
                        }
                    }
                }
            }
            catch
            {
                //await DisplayAlert("Error connecting.", $"Error connecting to BLE device: {selectedItem.Name ?? "N/A"}", "Retry");

                var popMessage = new ReceiverDisconnected();
                await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

                await Navigation.PushModalAsync(new NoReceiversFoundPage());
            }
        }
    }
}
