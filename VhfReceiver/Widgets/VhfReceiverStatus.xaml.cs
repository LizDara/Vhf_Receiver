using System;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class VhfReceiverStatus : ContentView
    {
        public ReceiverInformation ReceiverInformation;
        private string sdCardImage;
        public string SDCardImage
        {
            set
            {
                sdCardImage = value;
                OnPropertyChanged(nameof(SDCardImage));
            }
            get { return sdCardImage; }
        }
        private string sdCardState;
        public string SDCardState
        {
            set
            {
                sdCardState = value;
                OnPropertyChanged(nameof(SDCardState));
                SDCardImage = ReceiverInformation.IsSDCardInserted() ? "SDCard" : "NoSDCard";
            }
            get { return sdCardState; }
        }
        private string batteryImage;
        public string BatteryImage
        {
            set
            {
                batteryImage = value;
                OnPropertyChanged(nameof(BatteryImage));
            }
            get { return batteryImage; }
        }
        private string batteryPercent;
        public string BatteryPercent
        {
            set
            {
                batteryPercent = value;
                OnPropertyChanged(nameof(BatteryPercent));
                BatteryImage = ReceiverInformation.GetDeviceBattery() > 20 ? "FullBattery" : "EmptyBattery";
            }
            get { return batteryPercent; }
        }

        public VhfReceiverStatus()
        {
            InitializeComponent();
            BindingContext = this;

            ReceiverInformation = ReceiverInformation.GetInstance();
            UpdateBattery();
            UpdateSDCardState();
        }

        public void UpdateSDCardState()
        {
            SDCardState = ReceiverInformation.IsSDCardInserted() ? "Inserted" : "None";
        }

        public void UpdateBattery()
        {
            BatteryPercent = ReceiverInformation.GetDeviceBattery() + "%";
        }
    }
}
