using System;
using System.Collections.Generic;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages.BluetoothReceiver
{
    public partial class TagDetectionPage : ContentPage
    {
        private List<TagInformation> Tags;
        private bool Enabled;

        public TagDetectionPage()
        {
            InitializeComponent();
            BindingContext = this;

            Toolbar.SetData("TAG DETECTION", false);
            SetData();
            _ = TransferBLEData.ReceiveTags(ValueUpdateTags);
        }

        private void SetData()
        {
            Tags = new List<TagInformation>();
            Enabled = false;
            SetLocationData();
        }

        private void TagsList_Tapped(object sender, ItemTappedEventArgs e)
        {
            Tags[e.ItemIndex].SetVisibility(!Tags[e.ItemIndex].Visible);
            TagsList.ItemsSource = null;
            TagsList.ItemsSource = Tags;
        }

        private void ValueUpdateTags(object o, CharacteristicUpdatedEventArgs args)
        {
            byte[] value = args.Characteristic.Value;
            Console.WriteLine(Converters.GetHexValue(value));
            bool update = false;
            for (int i = 0; i < Tags.Count; i++)
            {
                if (Tags[i].Code.Equals(Converters.GetAsciiValue(6, 14, value)))
                {
                    Tags[i].UpdateData(value);
                    update = true;
                }
            }
            if (!update)
                Tags.Add(new TagInformation(value));
            TagsList.ItemsSource = null;
            TagsList.ItemsSource = Tags;
        }

        private void Location_Clicked(object sender, EventArgs e)
        {
            Enabled = !Enabled;
            SetLocationData();
        }

        private void SetLocationData()
        {
            Gps.Source = Enabled ? "GpsValid" : "GpsOff";
            LocationData.Text = Enabled ? "Location Data (Enabled)" : "Location Data (Disabled)";
            Coordinates.Text = Enabled ? "00.000000, -00.000000" : "Location: Unknown";
            Location.Text = Enabled ? "Disable" : "Enable";
        }
    }
}
