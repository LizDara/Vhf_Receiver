using System;
using Plugin.BLE.Abstractions.Contracts;

namespace VhfReceiver.Utils
{
    public class ReceiverInformation
    {
        private static ReceiverInformation receiverInformation = null;
        private int DeviceBattery;
        private string SerialNumber;
        private bool SDCardInserted;

        private IDevice Device;
        private IAdapter BluetoothAdapter;

        private ReceiverInformation()
        {
            SerialNumber = "Unknown";
            DeviceBattery = 0;
            SDCardInserted = false;
        }

        public static ReceiverInformation GetInstance()
        {
            if (receiverInformation == null)
                receiverInformation = new ReceiverInformation();
            return receiverInformation;
        }

        public void ChangeInformation(string deviceBattery, IDevice device, IAdapter adapter)
        {
            DeviceBattery = int.Parse(deviceBattery.Replace("%", ""));
            SerialNumber = device.Name.Substring(0, 7);
            Device = device;
            BluetoothAdapter = adapter;
        }

        public void ChangeSDCard(bool inserted)
        {
            SDCardInserted = inserted;
        }

        public void ChangeDeviceBattery(int deviceBattery)
        {
            DeviceBattery = deviceBattery;
        }

        public int GetDeviceBattery()
        {
            return DeviceBattery;
        }

        public string GetSerialNumber()
        {
            return SerialNumber;
        }

        public bool IsSDCardInserted()
        {
            return SDCardInserted;
        }

        public IDevice GetDevice()
        {
            return Device;
        }

        public IAdapter GetAdapter()
        {
            return BluetoothAdapter;
        }

        public void Initialize()
        {
            receiverInformation = null;
        }
    }
}
