using System;
using Xamarin.Forms;

namespace VhfReceiver.Utils
{
    public class DeviceCategory
    {
        public string LogoImage
        {
            get;set;
        }

        public Brush BaseColor
        {
            get;set;
        }

        public string Name
        {
            get;set;
        }

        public string Description
        {
            get;set;
        }

        public DeviceCategory(string name, string description)
        {
            Name = name;
            Description = description;
            SetType();
        }

        private void SetType()
        {
            switch (Name)
            {
                case "VHF Receivers":
                    BaseColor = Color.FromHex("#EAB308");
                    LogoImage = "vhf.png";
                    break;
                case "Acoustic Receivers":
                    BaseColor = Color.FromHex("#22C55E");
                    LogoImage = "acoustic.png";
                    break;
                case "Bluetooth Tags":
                    BaseColor = Color.FromHex("#3994C1");
                    LogoImage = "tags.png";
                    break;
                case "Connect to Bluetooth Receiver":
                    BaseColor = Color.FromHex("#3994C1");
                    LogoImage = "bluetooth.png";
                    break;
                case "View Beacon Tags Directly":
                    BaseColor = Color.FromHex("#3994C1");
                    LogoImage = "beacon.png";
                    break;
                case "Wildlink":
                    BaseColor = Color.FromHex("#F97316");
                    LogoImage = "wildlink.png";
                    break;
            }
        }
    }
}
