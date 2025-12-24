using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class GpsOption : ContentView
    {
        private bool Scanning;

        public GpsOption()
        {
            InitializeComponent();
        }

        public void SetData(bool isScanning)
        {
            Scanning = isScanning;
        }

        public void SetGps(bool gpsOn)
        {
            GPS.IsToggled = gpsOn;
        }

        public bool IsGpsOn()
        {
            return GPS.IsToggled;
        }

        private async void GPS_Toggled(object sender, ToggledEventArgs e)
        {
            if (Scanning)
            {
                bool result = await TransferBLEData.WriteGps(e.Value);
                if (result)
                {
                    MessagingCenter.Send(e.Value ? "ON" : "OFF", ValueCodes.GPS);
                }
                else
                {
                    Scanning = false;
                    GPS.IsToggled = !e.Value;
                    Scanning = true;
                }
            }
        }
    }
}
