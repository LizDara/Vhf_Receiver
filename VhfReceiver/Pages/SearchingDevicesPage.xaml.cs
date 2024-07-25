using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE;
using Xamarin.Forms;
using VhfReceiver.Utils;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Widgets;
using Xamarin.Essentials;
using Plugin.BLE.Abstractions.EventArgs;

namespace VhfReceiver.Pages
{
    public partial class SearchingDevicesPage : ContentPage
    {
        private readonly IAdapter BluetoothAdapter;
        private readonly List<IDevice> GattDevices = new List<IDevice>();
        private readonly List<DeviceInformation> DevicesInformation = new List<DeviceInformation>();
        private DeviceInformation selectedItem;
        ReceiverInformation receiverStatus;

        public SearchingDevicesPage()
        {
            InitializeComponent();
            BindingContext = this;

            BluetoothAdapter = CrossBluetoothLE.Current.Adapter;
            BluetoothAdapter.ScanTimeout = 8000;
            BluetoothAdapter.DeviceDiscovered += DeviceDiscovered;

            Device.BeginInvokeOnMainThread(async () => {
                if (!await PermissionsGrantedAsync())
                {
                    await DisplayAlert("Permission required.", "Application needs location permission.", "OK");
                    LoadingSearch.IsVisible = LoadingSearch.IsRunning = Searching.IsVisible = !(Refresh.IsEnabled = true);
                    return;
                }

                StartSearch();

                await BluetoothAdapter.StartScanningForDevicesAsync();

                RefreshSpace.IsVisible = false;
                SetDevicesList();

                if (GattDevices.Count == 0)
                    await Navigation.PushModalAsync(new NoReceiversFoundPage(), false);
            });

            SetDevicesList();
        }

        private void DeviceDiscovered(object sender, DeviceEventArgs foundBLEDevice)
        {
            if (foundBLEDevice.Device != null
                    && !string.IsNullOrEmpty(foundBLEDevice.Device.Name)
                    && foundBLEDevice.Device.Name.Contains("ATSvr")
                    && !GattDevices.Contains(foundBLEDevice.Device))
            {
                GattDevices.Add(foundBLEDevice.Device);
                DevicesInformation.Add(
                    new DeviceInformation(foundBLEDevice.Device, foundBLEDevice.Device.AdvertisementRecords[2].Data));
            }
        }
        
        private async Task<bool> PermissionsGrantedAsync()
        {
            var locationPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            var storageWritePermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (locationPermissionStatus != PermissionStatus.Granted)
            {
                var locationStatus = await Permissions.RequestAsync<Permissions.LocationAlways>();

                if (storageWritePermissionStatus != PermissionStatus.Granted)
                {
                    var writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

                    return locationStatus == PermissionStatus.Granted &&
                        writeStatus == PermissionStatus.Granted;
                }

                return locationStatus == PermissionStatus.Granted;
            }
            
            return true;
        }

        private void StartSearch()
        {
            LoadingSearch.IsVisible = LoadingSearch.IsRunning = Searching.IsVisible = !(Refresh.IsEnabled = false);
            Refresh.Opacity = 0.6;
            Select.IsVisible = Refresh.IsEnabled;
            DevicesList.ItemsSource = null;
            GattDevices.Clear();
            DevicesInformation.Clear();
        }

        private void SetDevicesList()
        {
            if (GattDevices.Count > 0)
            {
                DevicesList.ItemsSource = DevicesInformation;
                LoadingSearch.IsVisible = LoadingSearch.IsRunning = Searching.IsVisible = !(Refresh.IsEnabled = true);
                Refresh.Opacity = 1.0;
                Select.IsVisible = DevicesList.IsVisible = Refresh.IsEnabled;
                Refresh.IsVisible = true;
            }
        }

        private async void Refresh_Clicked(object sender, EventArgs e)
        {
            StartSearch();
            await BluetoothAdapter.StartScanningForDevicesAsync();
            SetDevicesList();

            if (GattDevices.Count == 0)
                await Navigation.PushModalAsync(new NoReceiversFoundPage(), false);
        }

        private async void DevicesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            selectedItem = e.Item as DeviceInformation;

            if (selectedItem.Device == null) return;
            if (selectedItem.Device.Name.Contains("#000000")) // Error, factory setup required
            {
                await DisplayAlert("Error", "Factory Setup Required.", "OK");
            }
            else
            {
                SearchingDevices.IsVisible = false;
                LoadingConnect.IsRunning = ConnectingDevice.IsVisible = Status.IsVisible = true;
                DeviceName.Text = selectedItem.Name;
                DeviceRange.Text = selectedItem.Range;
                DeviceBattery.Text = selectedItem.Battery;

                BluetoothAdapter.DeviceConnected += DeviceConnected;
                BluetoothAdapter.DeviceConnectionLost += DeviceConnectionLost;
                BluetoothAdapter.DeviceDisconnected += DeviceDisconnected;

                var connectParameters = new ConnectParameters(false, true);
                await BluetoothAdapter.ConnectToDeviceAsync(selectedItem.Device, connectParameters);
            }
        }

        private void DeviceConnected(object sender, DeviceEventArgs args)
        {
            if (args.Device == selectedItem.Device)
            {
                receiverStatus = ReceiverInformation.GetReceiverInformation();
                receiverStatus.ChangeInformation(0, 0, selectedItem.Device.Name.Substring(0, 7), selectedItem.Range,
                    selectedItem.Battery, selectedItem.Device, BluetoothAdapter);

                ScanState(selectedItem);
            }
        }

        private void DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            if (e.Device == selectedItem.Device)
            {
                Console.WriteLine("Connection Lost: " + e.ErrorMessage);
                ShowDisconnectionMessage();
            }
        }

        private void DeviceDisconnected(object sender, DeviceEventArgs args)
        {
            if (args.Device == selectedItem.Device)
            {
                Console.WriteLine("Disconnected");
                ShowDisconnectionMessage();
            }
        }

        private async void ScanState(DeviceInformation selectedItem)
        {
            try
            {
                var service = await selectedItem.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCREEN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SEND_LOG);
                    if (characteristic != null)
                    {
                        characteristic.ValueUpdated += ValueUpdateScan;

                        await characteristic.StartUpdatesAsync();
                        byte[] resultBytes = await GetBoardState(selectedItem);
                        Console.WriteLine(Converters.GetHexValue(resultBytes));
                        SetData(resultBytes, selectedItem);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
        }

        private async void ValueUpdateScan(object o, CharacteristicUpdatedEventArgs args)
        {
            var bytes = args.Characteristic.Value;
            if (bytes.Length > 0)
            {
                Console.WriteLine(Converters.GetHexValue(bytes));
                if (Converters.GetHexValue(bytes[0]).Equals("50"))
                {
                    byte detectionType = 0;
                    if (selectedItem.Name.Contains("Fixed"))
                        detectionType = 0x08;
                    else if (selectedItem.Name.Contains("Variable"))
                        detectionType = 0x07;
                    else if (selectedItem.Name.Contains("Coded"))
                        detectionType = 0x09;
                    receiverStatus.ChangeScanState(bytes[1]);
                    if (Converters.GetHexValue(bytes[1]).Equals("00"))
                    {
                        await Navigation.PushModalAsync(new HomePage(detectionType));
                        args.Characteristic.ValueUpdated -= ValueUpdateScan;
                    }
                    else if (Converters.GetHexValue(bytes[1]).Equals("82") || Converters.GetHexValue(bytes[1]).Equals("81") || Converters.GetHexValue(bytes[1]).Equals("80"))
                    {
                        var characteristic = await args.Characteristic.Service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SEND_LOG);
                        bool isHold = Converters.GetHexValue(bytes[1]).Equals("81");
                        int autoRecord = bytes[2] >> 6 & 1;
                        int currentFrequency = (bytes[16] * 256) + bytes[17];
                        int currentIndex = (bytes[7] * 256) + bytes[8];
                        int maxIndex = (bytes[5] * 256) + bytes[6];
                        args.Characteristic.ValueUpdated -= ValueUpdateScan;
                        bytes = await GetMobileDefaults(selectedItem);
                        if (bytes != null)
                            await Navigation.PushModalAsync(new MobileScanningPage(characteristic, bytes, isHold, autoRecord == 1, currentFrequency, currentIndex, maxIndex, detectionType), false);
                    }
                    else if (Converters.GetHexValue(bytes[1]).Equals("83"))
                    {
                        var characteristic = await args.Characteristic.Service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SEND_LOG);
                        int currentFrequency = (bytes[16] * 256) + bytes[17];
                        int currentIndex = (bytes[7] * 256) + bytes[8];
                        int maxIndex = (bytes[5] * 256) + bytes[6];
                        args.Characteristic.ValueUpdated -= ValueUpdateScan;
                        bytes = await GetStationaryDefaults(selectedItem);
                        if (bytes != null)
                            await Navigation.PushModalAsync(new StationaryScanningPage(characteristic, bytes, currentFrequency, currentIndex, maxIndex, detectionType), false);
                    }
                    else if (Converters.GetHexValue(bytes[1]).Equals("86"))
                    {
                        var characteristic = await args.Characteristic.Service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SEND_LOG);
                        args.Characteristic.ValueUpdated -= ValueUpdateScan;
                        await Navigation.PushModalAsync(new ManualScanningPage(characteristic, detectionType), false);
                    }
                }
            }
        }

        private async Task<byte[]> GetBoardState(DeviceInformation selectedItem)
        {
            try
            {
                var service = await selectedItem.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_DIAGNOSTIC);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_BOARD_STATUS);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return null;
        }

        private async Task<byte[]> GetMobileDefaults(DeviceInformation selectedItem)
        {
            try
            {
                var service = await selectedItem.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return null;
        }

        private async Task<byte[]> GetStationaryDefaults(DeviceInformation selectedItem)
        {
            try
            {
                var service = await selectedItem.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return null;
        }

        private void SetData(byte[] bytes, DeviceInformation selectedItem)
        {
            int baseFrequency = bytes[1];
            int range = bytes[2];
            Preferences.Set("BaseFrequency", baseFrequency);
            Preferences.Set("Range", range);

            byte detectionType = 0;
            if (selectedItem.Name.Contains("Fixed"))
                detectionType = 0x08;
            else if (selectedItem.Name.Contains("Variable"))
                detectionType = 0x07;
            else if (selectedItem.Name.Contains("Coded"))
                detectionType = 0x09;
            receiverStatus.ChangeTxType(detectionType);
        }

        private async void ShowDisconnectionMessage()
        {
            var popMessage = new ReceiverDisconnected();
            await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

            await Navigation.PushModalAsync(new SearchingDevicesPage(), false);
        }
    }
}
