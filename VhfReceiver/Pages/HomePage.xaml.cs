using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using VhfReceiver.Utils;
using Xamarin.Essentials;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class HomePage : ContentPage
    {
        private IAdapter BluetoothAdapter;
        private DeviceInformation ConnectedDevice;

        public HomePage(IAdapter bluetoothAdapter, DeviceInformation deviceInformation, byte[] bytes)
        {
            InitializeComponent();

            if (bytes.Length > 1)
            {
                Task.Run(async () => await Task.Delay(5000)).Wait();

                SetData(bytes);
            }
            
            BluetoothAdapter = bluetoothAdapter;
            ConnectedDevice = deviceInformation;
            DeviceName.Text = ConnectedDevice.Name;
            DeviceBattery.Text = ConnectedDevice.Battery;
        }

        private void SetData(byte[] bytes)
        {
            int baseFrequency = bytes[1];
            int range = bytes[2];
            int detectionType = bytes[3];
            int statusBytesDefault = bytes[7];

            Preferences.Set("BaseFrequency", baseFrequency);
        }

        private async void Disconnect_Clicked(object sender, EventArgs e)
        {
            await BluetoothAdapter.DisconnectDeviceAsync(ConnectedDevice.Device);

            await Navigation.PushModalAsync(new SearchingDevicesPage());
        }

        private async void StartScanning_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new StartScanningPage(ConnectedDevice));
        }

        private async void ReceiverOptions_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ReceiverOptionsPage(ConnectedDevice));
        }
    }
}
