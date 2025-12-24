using System;
using System.Collections.Generic;
using VhfReceiver.Pages;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class Devices : ContentView
    {
        private SearchingDevicesPage Content;
        private bool IsEditable;
        public readonly List<DeviceInformation> DevicesInformation = new List<DeviceInformation>();
        public DeviceInformation SelectedItem;
        public string Selector;

        public Devices()
        {
            InitializeComponent();
        }

        public void SetData(SearchingDevicesPage content, bool isEditable)
        {
            Content = content;
            IsEditable = isEditable;
        }

        public void SetDevices()
        {
            if (!IsEditable)
                DevicesList.HeightRequest = DevicesInformation[0].Height;
            DevicesList.ItemsSource = null;
            DevicesList.ItemsSource = DevicesInformation;
        }

        public string GetType(string type)
        {
            string title;
            switch (type)
            {
                case "VHF Receivers":
                    Selector = ValueCodes.VHF;
                    title = "VHF RECEIVER";
                    break;
                case "Acoustic Receivers":
                    Selector = ValueCodes.ACOUSTIC;
                    title = "ACOUSTIC RECEIVER";
                    break;
                case "Wildlink":
                    Selector = ValueCodes.WILDLINK;
                    title = type.ToUpper();
                    break;
                case "Connect to Bluetooth Receiver":
                    Selector = ValueCodes.BLUETOOTH_RECEIVER;
                    title = "BLUETOOTH RECEIVER";
                    break;
                case "View Beacon Tags Directly":
                    Selector = ValueCodes.TAGS;
                    title = "BEACON TAGS DIRECTLY";
                    break;
                default:
                    Selector = "ATS";
                    title = "RECEIVER";
                    break;
            }
            return title;
        }

        private async void DevicesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            if (IsEditable)
            {
                SelectedItem = e.Item as DeviceInformation;
                if (SelectedItem.Device == null) return;
                if (SelectedItem.Device.Name.Contains("#000000")) // Error, factory setup required
                {
                    await Content.DisplayAlert("Error", "Factory Setup Required.", "OK");
                }
                else
                {
                    if (!DevicesInformation[e.ItemIndex].Visible)
                        RemoveSelectedDevices();
                    if (Selector.Equals(ValueCodes.VHF))
                        DevicesInformation[e.ItemIndex].SetVhfVisibility(!DevicesInformation[e.ItemIndex].Visible);
                    else
                        DevicesInformation[e.ItemIndex].Visible = !DevicesInformation[e.ItemIndex].Visible;
                    DevicesList.ItemsSource = null;
                    DevicesList.ItemsSource = DevicesInformation;
                    Content.TitleSearching = DevicesInformation[e.ItemIndex].Visible ? "Device Selected" : "Found " + DevicesInformation.Count + " Devices";
                    Content.Message = DevicesInformation[e.ItemIndex].Visible ? "Click the connect button." : "Select device and click the connect button.";
                    Content.OpacityButton = DevicesInformation[e.ItemIndex].Visible ? 1 : 0.6;
                    Content.IsEnabledButton = DevicesInformation[e.ItemIndex].Visible;
                }
            }
        }

        private void RemoveSelectedDevices()
        {
            foreach (DeviceInformation device in DevicesInformation)
            {
                if (device.Visible)
                    device.SetVhfVisibility(false);
            }
        }
    }
}
