using System;
using Plugin.BLE.Abstractions.Contracts;

namespace VhfReceiver.Utils
{
    public class DeviceInformation
    {
        public IDevice Device;
        public byte[] Record;
        public string Name
        {
            get;
            set;
        }
        public string Range
        {
            get;
            set;
        }
        public string Battery
        {
            get;
            set;
        }

        public DeviceInformation(IDevice device, byte[] record)
        {
            Device = device;
            Record = record;

            SetDeviceName();
            if (!Name.Contains("#000000"))
            {
                SetFrequencyRange();
                SetBatteryPercentage();
            }
            else
            {
                Range = "None";
                Battery = "0%";
            }
        }

        private void SetFrequencyRange()
        {
            int baseFrequency = int.Parse(Device.Name.Substring(12, 3)) * 1000;
            int frequencyRange = ((int.Parse(Device.Name.Substring(17)) + (baseFrequency / 1000)) * 1000) - 1;

            Range = baseFrequency.ToString().Substring(0, 3) + "." + baseFrequency.ToString().Substring(3)
                + "-" + frequencyRange.ToString().Substring(0, 3) + "." + frequencyRange.ToString().Substring(3) + " MHz";
        }

        private void SetDeviceName()
        {
            string deviceName = Device.Name.Substring(0, 7);
            if (!deviceName.Contains("#000000"))
            {
                string type = Device.Name.Substring(15, 1);
                string status = GetStatus();
                switch (type)
                {
                    case "C":
                        type = " Coded,";
                        break;
                    case "F":
                        type = " Fixed PR,";
                        break;
                    case "V":
                        type = " Variable PR,";
                        break;
                }
                Name = deviceName + type + status;
            }
            else
            {
                Name = deviceName;
            }
        }

        private string GetStatus()
        {
            string status = Converters.GetHexValue(Record[3]);

            switch (status)
            {
                case "00":
                    status = " Not scanning";
                    break;
                case "82":
                case "81":
                case "80":
                    status = " Scanning, mobile";
                    break;
                case "83":
                    status = " Scanning, stationary";
                    break;
                case "86":
                    status = " Scanning, manual";
                    break;
                default:
                    status = " None";
                    break;
            }

            return status;
        }

        private void SetBatteryPercentage()
        {
            Battery = Record[2].ToString() + "%";
        }
    }
}
