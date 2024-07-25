using System;
using Plugin.BLE.Abstractions.Contracts;

namespace VhfReceiver.Utils
{
    public class ReceiverInformation
    {
        private static ReceiverInformation receiverInformation = null;
        private string DeviceName;
        private string DeviceStatus;
        private string DeviceRange;
        private string DeviceBattery;
        private IDevice Device;
        private IAdapter BluetoothAdapter;
        private byte TxType;
        private byte ScanState;

        private ReceiverInformation()
        {
            DeviceName = "Unknown";
            DeviceStatus = "Unknown";
            DeviceRange = "None";
            DeviceBattery = "%";
        }

        public static ReceiverInformation GetReceiverInformation()
        {
            if (receiverInformation == null)
            {
                receiverInformation = new ReceiverInformation();
            }
            return receiverInformation;
        }

        public void ChangeInformation(byte txType, byte scanState, string deviceName, string deviceRange, string deviceBattery, IDevice device, IAdapter adapter)
        {
            DeviceName = deviceName;
            DeviceRange = deviceRange;
            DeviceBattery = deviceBattery;
            Device = device;
            BluetoothAdapter = adapter;
            TxType = txType;
            ScanState = scanState;
            DeviceStatus = deviceName;
            SetTxType();
            SetScanState();
        }

        public void ChangeTxType(byte type)
        {
            DeviceStatus = DeviceName;
            TxType = type;
            SetTxType();
            SetScanState();
        }

        public void ChangeScanState(byte state)
        {
            DeviceStatus = DeviceName;
            ScanState = state;
            SetTxType();
            SetScanState();
        }

        public void ChangeDeviceBattery(string deviceBattery)
        {
            DeviceBattery = deviceBattery;
        }

        public string GetDeviceStatus()
        {
            return DeviceStatus;
        }

        public string GetDeviceRange()
        {
            return DeviceRange;
        }

        public string GetDeviceBattery()
        {
            return DeviceBattery;
        }

        public IDevice GetDevice()
        {
            return Device;
        }

        public IAdapter GetAdapter()
        {
            return BluetoothAdapter;
        }

        public void SetTxType()
        {
            switch (Converters.GetHexValue(TxType))
            {
                case "09":
                    DeviceStatus += " Coded,";
                    break;
                case "08":
                    DeviceStatus += " Fixed PR,";
                    break;
                case "07":
                    DeviceStatus += " Variable PR,";
                    break;
            }
        }

        public void SetScanState()
        {
            switch (Converters.GetHexValue(ScanState))
            {
                case "00":
                    DeviceStatus += " Not scanning";
                    break;
                case "82":
                case "81":
                case "80":
                    DeviceStatus += " Scanning, mobile";
                    break;
                case "83":
                    DeviceStatus += " Scanning, stationary";
                    break;
                case "86":
                    DeviceStatus += " Scanning, manual";
                    break;
                default:
                    DeviceStatus += " None";
                    break;
            }
        }
    }
}
