using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class MobileDefaultsPage : ContentPage
    {
        private readonly bool ReturnDefaults;

        public MobileDefaultsPage(byte[] bytes, bool returnDefaults)
        {
            InitializeComponent();

            ReturnDefaults = returnDefaults;
            Toolbar.SetData("Mobile Default Settings", true, Back_Clicked);
            MobileSetting.SetData(bytes, false);
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (!ExistNotSet())
            {
                if (ExistChanges())
                {
                    bool result = await SetMobileDefaults();
                    if (!result) return;
                    if (ReturnDefaults)
                        MobileSetting.MobileBytes = await TransferBLEData.ReadDefaults(true);
                }
            }
            if (ReturnDefaults)
                MessagingCenter.Send(MobileSetting.MobileBytes, ValueCodes.DEFAULTS_SCAN);
            await Navigation.PopModalAsync(false);
        }

        private async Task<bool> SetMobileDefaults()
        {
            int info = (MobileSetting.gps ? 1 : 0) << 7;
            info |= (MobileSetting.autoRecord ? 1 : 0) << 6;

            byte[] b = new byte[] { 0x4D, (byte)MobileSetting.tableNumber, (byte)info, (byte)(MobileSetting.scanTime * 10), 0, 0, 0, 0 };

            bool result = await TransferBLEData.WriteDefaults(true, b);
            return result;
        }

        private bool ExistNotSet()
        {
            return MobileSetting.tableNumber == 255 || MobileSetting.scanTime == 255;
        }

        private bool ExistChanges()
        {
            return (int) MobileSetting.OriginalData[ValueCodes.TABLE_NUMBER] != MobileSetting.tableNumber || (bool) MobileSetting.OriginalData[ValueCodes.GPS] != MobileSetting.gps ||
                (bool) MobileSetting.OriginalData[ValueCodes.AUTO_RECORD] != MobileSetting.autoRecord || (double) MobileSetting.OriginalData[ValueCodes.SCAN_RATE] != MobileSetting.scanTime;
        }

        private void ValueUpdateState(object o, CharacteristicUpdatedEventArgs args)
        {
            var value = args.Characteristic.Value;
            if (Converters.GetHexValue(value[0]).Equals("56"))
            {
                ReceiverInformation.GetInstance().ChangeSDCard(Converters.GetHexValue(value[1]).Equals("80"));
                ReceiverStatus.UpdateSDCardState();
            }
            else if (Converters.GetHexValue(value[0]).Equals("88"))
            {
                ReceiverInformation.GetInstance().ChangeDeviceBattery(value[1]);
                ReceiverStatus.UpdateBattery();
            }
        }
    }
}
