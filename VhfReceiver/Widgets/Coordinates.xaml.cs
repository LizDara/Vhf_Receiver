using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class Coordinates : ContentView
    {
        public Coordinates()
        {
            InitializeComponent();
        }

        public void SetData(byte[] value)
        {
            string[] coordinates = Converters.GetGpsData(value);
            Latitude.Text = coordinates[0];
            Longitude.Text = coordinates[1];
        }
    }
}
