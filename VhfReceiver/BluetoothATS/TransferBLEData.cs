using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace VhfReceiver.Utils
{
    public class TransferBLEData
    {
        public static async Task<bool> NotificationLog(EventHandler<CharacteristicUpdatedEventArgs> valueUpdateScan)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCREEN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SEND_LOG);
                    if (characteristic != null)
                    {
                        characteristic.ValueUpdated += valueUpdateScan;
                        await characteristic.StartUpdatesAsync();

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR NOTIFICATION LOG: " + e.Message);
            }
            return false;
        }

        public static async Task<byte[]> ReadBoardState()
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_DIAGNOSTIC);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_BOARD_STATUS);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return null;
        }

        public static async Task<bool> WriteDetectionFilter(byte[] value)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_TX_TYPE);
                    if (characteristic != null)
                    {
                        bool result = await characteristic.WriteAsync(value);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR WRITE DETECTION FILTER: " + ex.Message);
            }
            return false;
        }

        public static async Task<byte[]> ReadTables()
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_FREQ_TABLE);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        Console.WriteLine("READ TABLES");
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR READ TABLES: " + e.Message);
            }
            return null;
        }

        public static async Task<bool> WriteStartScan(string type, byte[] value)
        {
            Guid uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_MANUAL;
            switch (type)
            {
                case ValueCodes.MOBILE:
                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL;
                    break;
                case ValueCodes.STATIONARY:
                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY;
                    break;
            }
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(uuid);
                    if (characteristic != null)
                    {
                        bool result = await characteristic.WriteAsync(value);
                        Console.WriteLine("WRITE START SCAN");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WRITE START SCAN: " + e.Message);
            }
            return false;
        }

        public static async Task<bool> WriteStopScan(string type)
        {
            Guid uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_MANUAL;
            switch (type)
            {
                case ValueCodes.MOBILE:
                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL;
                    break;
                case ValueCodes.STATIONARY:
                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY;
                    break;
            }
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(uuid);
                    if (characteristic != null)
                    {
                        byte[] b = new byte[] { 0x87 };
                        bool result = await characteristic.WriteAsync(b);
                        Console.WriteLine("WRITE STOP SCAN");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WRITE STOP SCAN: " + e.Message);
            }
            return false;
        }

        public static async Task<bool> WriteRecord(bool start, bool isManual)
        {
            Guid uuid = isManual ? VhfReceiverUuids.UUID_CHARACTERISTIC_MANUAL : VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL;
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(uuid);
                    if (characteristic != null)
                    {
                        byte[] b = new byte[] { start ? (byte)0x8C : (byte)0x8E };
                        bool result = await characteristic.WriteAsync(b);
                        Console.WriteLine("WRITE RECORD");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WRITE RECORD: " + e.Message);
            }
            return false;
        }

        public static async Task<bool> WriteGps(bool gpsOn)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_GPS);
                    if (characteristic != null)
                    {
                        byte[] b = new byte[] { gpsOn ? (byte)0x92 : (byte)0x91 };
                        bool result = await characteristic.WriteAsync(b);
                        Console.WriteLine("WRITE GPS");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WRITE GPS: " + e.Message);
            }
            return false;
        }

        public static async Task<bool> WriteDecreaseIncrease(bool isDecrease)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SCAN_TABLE);
                    if (characteristic != null)
                    {
                        byte[] b = new byte[] { isDecrease ? (byte)0x5E : (byte)0x5F };
                        bool result = await characteristic.WriteAsync(b);
                        Console.WriteLine("WRITE DECREASE OR INCREASE");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR WRITE DECREASE INCREASE: " + ex.Message);
            }
            return false;
        }

        public static async Task<bool> WriteScanning(byte[] value)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SCAN_TABLE);
                    if (characteristic != null)
                    {
                        bool result = await characteristic.WriteAsync(value);
                        Console.WriteLine("WRITE SCANNING " + Converters.GetHexValue(value));
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR WRITE SCANNING: " + ex.Message);
            }
            return false;
        }

        public static async Task<bool> WriteHold(bool isHold)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
                    if (characteristic != null)
                    {
                        byte[] b = new byte[] { (byte)(isHold ? 0x80 : 0x81) };
                        bool result = await characteristic.WriteAsync(b);
                        Console.WriteLine("WRITE HOLD");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR WRITE HOLD: " + ex.Message);
            }
            return false;
        }

        public static async Task<bool> WriteLeftRight(bool isLeft)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SCAN_TABLE);
                    if (characteristic != null)
                    {
                        byte[] b;
                        if (isLeft)
                            b = new byte[] { 0x57 };
                        else
                            b = new byte[] { 0x58 };
                        bool result = await characteristic.WriteAsync(b);
                        Console.WriteLine("WRITE LEFT OR RIGHT");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WRITE LEFT RIGHT: " + e.Message);
            }
            return false;
        }

        public static async Task<byte[]> ReadDefaults(bool isMobile)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    ICharacteristic characteristic;
                    if (isMobile)
                        characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
                    else
                        characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR READ DEFAULTS: " + e.Message);
            }
            return null;
        }

        public static async Task<byte[]> ReadFrequencies(int number)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(GetTableCharacteristic(number));
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR READ FREQUENCIES: " + e.Message);
            }
            return null;
        }

        public static async Task<bool> WriteFrequencies(int number, byte[] value)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(GetTableCharacteristic(number));
                    if (characteristic != null)
                    {
                        bool result = await characteristic.WriteAsync(value);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WRITE FREQUENCIES: " + e.Message);
            }
            return false;
        }

        private static Guid GetTableCharacteristic(int number)
        {
            Guid characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_FREQ_TABLE;
            switch (number)
            {
                case 1:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_1;
                    break;
                case 2:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_2;
                    break;
                case 3:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_3;
                    break;
                case 4:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_4;
                    break;
                case 5:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_5;
                    break;
                case 6:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_6;
                    break;
                case 7:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_7;
                    break;
                case 8:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_8;
                    break;
                case 9:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_9;
                    break;
                case 10:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_10;
                    break;
                case 11:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_11;
                    break;
                case 12:
                    characteristic = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_12;
                    break;
            }
            return characteristic;
        }

        public static async Task<bool> WriteDefaults (bool isMobile, byte[] value)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    ICharacteristic characteristic;
                    if (isMobile)
                        characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_AERIAL);
                    else
                        characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STATIONARY);
                    if (characteristic != null)
                    {
                        bool result = await characteristic.WriteAsync(value);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WRITE DEFAULTS: " + e.Message);
            }
            return false;
        }

        public static async Task<byte[]> ReadDetectionFilter()
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SCAN);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_TX_TYPE);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR READ DETECTION FILTER: " + e.Message);
            }
            return null;
        }

        public static async Task<byte[]> ReadDiagnostic()
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_DIAGNOSTIC);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_DIAGNOSTIC_INFO);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR READ DIAGNOSTIC: " + e.Message);
            }
            return null;
        }

        public static async Task<byte[]> ReadDataInfo()
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_DIAGNOSTIC);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_DATA_INFO);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR READ DATA INFO: " + e.Message);
            }
            return null;
        }

        public static async Task<bool> DownloadResponse(EventHandler<CharacteristicUpdatedEventArgs> valueUpdateDownload)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STUDY_DATA);
                    if (characteristic != null)
                    {
                        characteristic.ValueUpdated += valueUpdateDownload;
                        await characteristic.StartUpdatesAsync();

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR DOWNLOAD RESPONSE: " + e.Message);
            }
            return false;
        }

        public static async Task<byte[]> ReadPageNumber()
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STUDY_DATA);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        return bytes;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR READ PAGE NUMBER: " + e.Message);
            }
            return null;
        }

        public static async Task<bool> WriteResponse(byte[] value)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STUDY_DATA);
                    if (characteristic != null)
                    {
                        bool result = await characteristic.WriteAsync(value);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WRITE RESPONSE: " + e.Message);
            }
            return false;
        }

        public async static Task<bool> ReceiveTags(EventHandler<CharacteristicUpdatedEventArgs> valueUpdateTags)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_TAG);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_TAG);
                    if (characteristic != null)
                    {
                        characteristic.ValueUpdated += valueUpdateTags;
                        await characteristic.StartUpdatesAsync();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR DOWNLOAD RESPONSE: " + e.Message);
            }
            return false;
        }

        public static async Task<bool> WriteOTA(byte[] value)
        {
            try
            {
                var service = await ReceiverInformation.GetInstance().GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_SILICON_LABS_OTA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_SILICON_LABS_OTA_CONTROL);
                    if (characteristic != null)
                    {
                        characteristic.WriteType = value.Length == 1 ? CharacteristicWriteType.Default : CharacteristicWriteType.WithoutResponse;
                        bool result = await characteristic.WriteAsync(value);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WRITE RESPONSE: " + e.Message);
            }
            return false;
        }

        public static async Task<bool> RequestMtu(int mtu)
        {
            try
            {
                int value = await ReceiverInformation.GetInstance().GetDevice().RequestMtuAsync(mtu);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WRITE RESPONSE: " + e.Message);
            }
            return false;
        }
    }
}
