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

            SetFrequencyRange();
            SetDeviceName();
            SetBatteryPercentage();
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
            string type = Device.Name.Substring(15, 1);
            string status = GetStatus();

            Name = deviceName + (type.Equals("C") ? " Coded, " : " Non coded, ") + status;
        }

        private string GetStatus()
        {
            string status = Converters.GetHexValue(Record[3]);

            switch (status)
            {
                case "00":
                    status = "Not scanning";
                    break;
                case "82":
                    status = "Scanning, mobile";
                    break;
                case "83":
                    status = "Scanning, stationary";
                    break;
                case "86":
                    status = "Scanning, manual";
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
