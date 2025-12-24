using System;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages.Acoustic
{
    public partial class HomePage : ContentPage
    {
        private readonly ReceiverInformation ReceiverInformation;

        public HomePage(ICharacteristic characteristic)
        {
            InitializeComponent();
            DisconnectMenu.SetData(true);
            ReceiverInformation = ReceiverInformation.GetInstance();
            DeviceName.Text = ReceiverInformation.GetSerialNumber() + " Acoustic Receiver";
            LogAcoustic(characteristic);
        }

        private void LogAcoustic(ICharacteristic characteristic)
        {
            try
            {
                characteristic.ValueUpdated += ValueUpdateAcoustic;
                characteristic.StartUpdatesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
        }

        private void ValueUpdateAcoustic(object o, CharacteristicUpdatedEventArgs args)
        {
            byte[] value = args.Characteristic.Value;
            Console.WriteLine(Converters.GetHexValue(value));
            string volts = System.Text.Encoding.UTF8.GetString(new byte[] { value[5], 46, value[6] });
            BatteryVoltage.Text = volts + " V";

            string detections = System.Text.Encoding.UTF8.GetString(new byte[] { value[7], value[8], value[9], value[10] });
            DetsHR.Text = detections.ToString();

            string batteryUsage = System.Text.Encoding.UTF8.GetString(new byte[] { value[11], value[12], value[13], value[14] });
            BatteryUsage.Text = (int.Parse(batteryUsage) * 100) + " mahrs";

            string status = (Converters.GetDecimalValue(value[15]).Equals("97") && Converters.GetDecimalValue(value[16]).Equals("97")) ? "NONE" : "ERROR";
            ErrorCode.Text = status;
        }
    }
}
