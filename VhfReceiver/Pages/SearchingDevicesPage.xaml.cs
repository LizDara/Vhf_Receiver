using System;
using System.Collections.Generic;
using System.Timers;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class SearchingDevicesPage : ContentPage
    {
        private IAdapter BluetoothAdapter;
        private readonly List<IDevice> GattDevices = new List<IDevice>();
        private bool IsCancel;
        private Timer ConnectionTimeout;
        private ReceiverInformation receiverStatus;
        public string titleSearching;
        public string TitleSearching
        {
            set
            {
                titleSearching = value;
                OnPropertyChanged(nameof(TitleSearching));
            }
            get { return titleSearching; }
        }
        public string message;
        public string Message
        {
            set
            {
                message = value;
                OnPropertyChanged(nameof(Message));
            }
            get { return message; }
        }
        public bool isEnabledButton;
        public bool IsEnabledButton
        {
            set
            {
                isEnabledButton = value;
                OnPropertyChanged(nameof(IsEnabledButton));
            }
            get { return isEnabledButton; }
        }
        public double opacityButton;
        public double OpacityButton
        {
            set
            {
                opacityButton = value;
                OnPropertyChanged(nameof(OpacityButton));
            }
            get { return opacityButton; }
        }

        public SearchingDevicesPage(string type)
        {
            InitializeComponent();
            BindingContext = this;

            DevicesFound.SetData(this, true);
            SelectedDevice.SetData(this, false);
            Toolbar.SetData("SELECT " + DevicesFound.GetType(type), false);
            StartSearch();
            Device.BeginInvokeOnMainThread(async () => {
                BluetoothAdapter = CrossBluetoothLE.Current.Adapter;
                BluetoothAdapter.ScanTimeout = ValueCodes.SCAN_PERIOD;
                BluetoothAdapter.DeviceDiscovered += DeviceDiscovered;
                await BluetoothAdapter.StartScanningForDevicesAsync();

                SetDevicesList();
            });
        }

        private void DeviceDiscovered(object sender, DeviceEventArgs foundBLEDevice)
        {
            if (foundBLEDevice.Device != null
                    && !string.IsNullOrEmpty(foundBLEDevice.Device.Name)
                    && foundBLEDevice.Device.Name.Contains(DevicesFound.Selector)
                    && !GattDevices.Contains(foundBLEDevice.Device))
            {
                GattDevices.Add(foundBLEDevice.Device);
                if (DevicesFound.Selector.Equals(ValueCodes.BLUETOOTH_RECEIVER))
                    DevicesFound.DevicesInformation.Add(new DeviceInformation(foundBLEDevice.Device));
                else
                    DevicesFound.DevicesInformation.Add(
                    new DeviceInformation(foundBLEDevice.Device, foundBLEDevice.Device.AdvertisementRecords[2].Data));
                Console.WriteLine("AFTER TO ADD DEVICE");
            }
        }

        private void StartSearch()
        {
            TitleSearching = "Searching For Devices ...";
            Message = "It is only going to take a few seconds.";
            Loading.IsVisible = Loading.IsRunning = true;
            OpacityButton = 0.6;
            IsEnabledButton = Retry.IsVisible = Cancel.IsVisible = DevicesFound.IsVisible = false;
            GattDevices.Clear();
            DevicesFound.DevicesInformation.Clear();
        }

        private void SetDevicesList()
        {
            if (DevicesFound.DevicesInformation.Count > 0)
            {
                Console.WriteLine("BEFORE SET DEVICES");
                TitleSearching = "Found " + DevicesFound.DevicesInformation.Count + " Devices";
                Message = "Select device and click the connect button.";
                DevicesFound.IsVisible = true;
                DevicesFound.SetDevices();
                Loading.IsVisible = Loading.IsRunning = Cancel.IsVisible = false;
                Console.WriteLine("AFTER SET DEVICES");
            }
            else
            {
                TitleSearching = "No Devices Found";
                Message = "Please make sure your devices are within bluetooth range.";
                Loading.IsVisible = Loading.IsRunning = false;
                Retry.IsVisible = true;
            }
        }

        private async void Retry_Clicked(object sender, EventArgs e)
        {
            StartSearch();
            await BluetoothAdapter.StartScanningForDevicesAsync();
            SetDevicesList();
        }

        private async void Connect_Clicked(object sender, EventArgs e)
        {
            IsEnabledButton = IsCancel = DevicesFound.IsVisible = false;
            Loading.IsVisible = Loading.IsRunning = Cancel.IsVisible = SelectedDevice.IsVisible = true;
            SelectedDevice.DevicesInformation.Add(DevicesFound.SelectedItem);
            SelectedDevice.SetDevices();
            TitleSearching = "Connecting ...";
            Message = "This should only take a few seconds.";
            OpacityButton = 0.6;

            BluetoothAdapter.DeviceConnected += DeviceConnected;
            BluetoothAdapter.DeviceConnectionLost += DeviceConnectionLost;
            BluetoothAdapter.DeviceDisconnected += DeviceDisconnected;

            ConnectionTimeout = new Timer(ValueCodes.CONNECT_TIMEOUT);
            ConnectionTimeout.Elapsed += Tick;
            ConnectionTimeout.Enabled = true;

            var connectParameters = new ConnectParameters(false, true);
            await BluetoothAdapter.ConnectToDeviceAsync(DevicesFound.SelectedItem.Device, connectParameters);
        }

        private void Tick(object source, ElapsedEventArgs e)
        {
            ShowDisconnectionMessage("Failed to connect to device");
            ConnectionTimeout.Enabled = false;
            ConnectionTimeout.Dispose();
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            ConnectionTimeout.Enabled = false;
            IsCancel = true;
            SelectedDevice.IsVisible = false;
            DevicesFound.IsVisible = true;
            SetDevicesList();
        }

        private async void DeviceConnected(object sender, DeviceEventArgs args)
        {
            if (!IsCancel)
            {
                if (args.Device == DevicesFound.SelectedItem.Device)
                {
                    ConnectionTimeout.Enabled = false;
                    ConnectionTimeout.Dispose();
                    receiverStatus = ReceiverInformation.GetInstance();
                    receiverStatus.ChangeInformation(DevicesFound.SelectedItem.Battery, DevicesFound.SelectedItem.Device, BluetoothAdapter);
                    if (DevicesFound.Selector.Equals(ValueCodes.VHF) || DevicesFound.Selector.Equals(ValueCodes.ACOUSTIC))
                        ScanState();
                    else if (DevicesFound.Selector.Equals(ValueCodes.BLUETOOTH_RECEIVER))
                        await Navigation.PushModalAsync(new BluetoothReceiver.HomePage(), false);

                    TitleSearching = "Success";
                    Message = "Your device is connected.";
                    Loading.IsVisible = Loading.IsRunning = Cancel.IsVisible = false;
                    Connected.IsVisible = true;
                }
            }
            else
            {
                await BluetoothAdapter.DisconnectDeviceAsync(args.Device);
            }
        }

        private void DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            if (e.Device == DevicesFound.SelectedItem.Device)
            {
                Console.WriteLine("Connection Lost: " + e.ToString());
                ReceiverInformation receiverInformation = ReceiverInformation.GetInstance();
                receiverInformation.Initialize();
                ShowDisconnectionMessage("Receiver Disconnected");
            }
        }

        private void DeviceDisconnected(object sender, DeviceEventArgs args)
        {
            if (!IsCancel)
            {
                if (args.Device == DevicesFound.SelectedItem.Device)
                {
                    Console.WriteLine("Disconnected");
                    ReceiverInformation receiverInformation = ReceiverInformation.GetInstance();
                    receiverInformation.Initialize();
                    ShowDisconnectionMessage("Receiver Disconnected");
                }
            }
        }

        private async void ScanState()
        {
            bool result = await TransferBLEData.NotificationLog(ValueUpdateScan);
            if (result)
            {
                byte[] resultBytes = await TransferBLEData.ReadBoardState();
                Console.WriteLine(Converters.GetHexValue(resultBytes));
                if (DevicesFound.Selector.Equals(ValueCodes.VHF))
                    SetData(resultBytes);
            }
        }

        private async void ValueUpdateScan(object o, CharacteristicUpdatedEventArgs args)
        {
            var value = args.Characteristic.Value;
            if (value.Length > 0)
            {
                Console.WriteLine(Converters.GetHexValue(value));
                if (DevicesFound.Selector.Equals(ValueCodes.VHF))
                {
                    if (Converters.GetHexValue(value[0]).Equals("50"))
                    {
                        byte detectionType = 0;
                        if (DevicesFound.SelectedItem.Status.Contains("Fixed"))
                            detectionType = 0x08;
                        else if (DevicesFound.SelectedItem.Status.Contains("Variable"))
                            detectionType = 0x07;
                        else if (DevicesFound.SelectedItem.Status.Contains("Coded"))
                            detectionType = 0x09;
                        if (Converters.GetHexValue(value[1]).Equals("00"))
                        {
                            await Navigation.PushModalAsync(new HomePage(detectionType), false);
                            args.Characteristic.ValueUpdated -= ValueUpdateScan;
                        }
                        else if (Converters.GetHexValue(value[1]).Equals("82") || Converters.GetHexValue(value[1]).Equals("81") || Converters.GetHexValue(value[1]).Equals("80"))
                        {
                            var characteristic = await args.Characteristic.Service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SEND_LOG);
                            args.Characteristic.ValueUpdated -= ValueUpdateScan;
                            var bytes = await TransferBLEData.ReadDefaults(true);
                            if (bytes != null)
                                await Navigation.PushModalAsync(new MobileScanningPage(characteristic, bytes, value), false);
                        }
                        else if (Converters.GetHexValue(value[1]).Equals("83"))
                        {
                            var characteristic = await args.Characteristic.Service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SEND_LOG);
                            args.Characteristic.ValueUpdated -= ValueUpdateScan;
                            var bytes = await TransferBLEData.ReadDefaults(false);
                            if (bytes != null)
                                await Navigation.PushModalAsync(new StationaryScanningPage(characteristic, bytes, value), false);
                        }
                        else if (Converters.GetHexValue(value[1]).Equals("86"))
                        {
                            var characteristic = await args.Characteristic.Service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SEND_LOG);
                            args.Characteristic.ValueUpdated -= ValueUpdateScan;
                            await Navigation.PushModalAsync(new ManualScanningPage(characteristic, value), false);
                        }
                    }
                }
                else if (DevicesFound.Selector.Equals(ValueCodes.ACOUSTIC))
                {
                    var characteristic = await args.Characteristic.Service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SEND_LOG);
                    args.Characteristic.ValueUpdated -= ValueUpdateScan;
                    await Navigation.PushModalAsync(new Acoustic.HomePage(characteristic));
                }
            }
        }

        private void SetData(byte[] bytes)
        {
            if (DevicesFound.Selector.Equals(ValueCodes.VHF))
            {
                receiverStatus.ChangeSDCard(bytes[7] == 1);
                int baseFrequency = bytes[2];
                int range = bytes[3];
                Preferences.Set("BaseFrequency", baseFrequency);
                Preferences.Set("Range", range);
            }
        }

        private async void ShowDisconnectionMessage(string message)
        {
            var popMessage = new ReceiverDisconnected(message);
            await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

            await Navigation.PushModalAsync(new BridgePage(), false);
        }
    }
}
