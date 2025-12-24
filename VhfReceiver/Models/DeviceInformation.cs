using System;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Forms;

namespace VhfReceiver.Utils
{
    public class DeviceInformation
    {
        public IDevice Device;
        public byte[] Record;
        public Brush BaseColor
        {
            get;set;
        }
        public string LogoImage
        {
            get;set;
        }
        public string Name
        {
            get;set;
        }
        public string Status
        {
            get;set;
        }
        public bool Visible
        {
            get;set;
        }
        public string Range
        {
            get;set;
        }
        public string BatteryImage
        {
            get; set;
        }
        public string Battery
        {
            get;set;
        }
        public double Height
        {
            get;set;
        }
        public bool VisibleDetails
        {
            get;set;
        }

        public DeviceInformation(IDevice device, byte[] record)
        {
            Device = device;
            Record = record;
            SetInformation();
        }

        public DeviceInformation(IDevice device)
        {
            Device = device;
            SetInformation();
        }

        private void SetInformation()
        {
            string deviceName = Device.Name;
            string serialNumber = deviceName.Substring(0, 7);
            if (!serialNumber.Equals("0000000"))
            {
                Name = serialNumber + " " + GetType(deviceName);
                SetDeviceType(deviceName);
                if (deviceName.Contains(ValueCodes.VHF))
                {
                    string detectionFilter = Converters.GetDetectionFilter(deviceName.Substring(15, 1));
                    string status = Converters.GetStatusVhfReceiver(Record);
                    int percent = Converters.GetPercentBatteryVhfReceiver(Record);
                    int baseFrequency = int.Parse(deviceName.Substring(12, 3)) * 1000;
                    int frequencyRange = ((int.Parse(deviceName.Substring(17)) + (baseFrequency / 1000)) * 1000) - 1;
                    Range = Converters.GetFrequency(baseFrequency) + "-" + Converters.GetFrequency(frequencyRange);
                    Status = detectionFilter + status;
                    Battery = percent + "%";
                    BatteryImage = "FullBatteryLight";
                }
                else
                {
                    Status = "Extra Details";
                    Battery = "0%";
                    BatteryImage = "FullBatteryLight";
                }
            }
            else
            {
                SetUnknownDevice();
            }
            SetVhfVisibility(false);
        }

        private string GetType(string name)
        {
            if (name.Contains(ValueCodes.VHF))
                return "VHF Receiver";
            else if (name.Contains(ValueCodes.ACOUSTIC))
                return "Acoustic Receiver";
            else if (name.Contains(ValueCodes.WILDLINK))
                return "Wildlink Receiver";
            else if (name.Contains(ValueCodes.BLUETOOTH_RECEIVER))
                return "Bluetooth Receiver";
            else if (name.Contains(ValueCodes.TAGS))
                return "Bluetooth Tag";
            return "Unknown";
        }

        private void SetDeviceType(string name)
        {
            if (name.Contains(ValueCodes.VHF))
            {
                BaseColor = Color.FromHex("#EAB308");
                LogoImage = "vhf.png";
            }
            else if (name.Contains(ValueCodes.ACOUSTIC))
            {
                BaseColor = Color.FromHex("#22C55E");
                LogoImage = "acoustic.png";
            }
            else if (name.Contains(ValueCodes.WILDLINK))
            {
                BaseColor = Color.FromHex("#F97316");
                LogoImage = "wildlink.png";
            }
            else if (name.Contains(ValueCodes.BLUETOOTH_RECEIVER))
            {
                BaseColor = Color.FromHex("#3994C1");
                LogoImage = "bluetooth.png";
            }
            else if (name.Contains(ValueCodes.TAGS))
            {
                BaseColor = Color.FromHex("#3994C1");
                LogoImage = "tags.png";
            }
        }

        private void SetUnknownDevice()
        {
            Name = "Unknown";
            Status = "None";
            Battery = "0%";
            BatteryImage = "FullBatteryLight";
            Visible = false;
        }

        public void SetVhfVisibility(bool isVisible)
        {
            Visible = VisibleDetails = isVisible;
            Height = isVisible ? 162 : 86;
        }
    }
}
