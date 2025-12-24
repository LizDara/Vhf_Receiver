using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Utils;
using Xamarin.Essentials;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class StationaryDefaultsPage : ContentPage
    {
        private readonly int BaseFrequency;
        private readonly bool ReturnDefaults;

        public StationaryDefaultsPage(byte[] bytes, bool returnDefaults)
        {
            InitializeComponent();

            BaseFrequency = Preferences.Get("BaseFrequency", 0) * 1000;
            ReturnDefaults = returnDefaults;
            Toolbar.SetData("Stationary Default Settings", true, Back_Clicked);
            StationarySetting.SetData(bytes, BaseFrequency, true);
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (ExistChanges())
            {
                if (IsDataCorrect())
                {
                    bool result = await SetStationaryDefaults();
                    if (!result) return;
                    if (ReturnDefaults)
                        StationarySetting.StationaryBytes = await TransferBLEData.ReadDefaults(false);
                }
                else
                {
                    await DisplayAlert("Message", "Data incorrect.", "OK");
                    return;
                }
            }
            if (ReturnDefaults)
                MessagingCenter.Send(StationarySetting.StationaryBytes, ValueCodes.DEFAULTS_SCAN);
            await Navigation.PopModalAsync(false);
        }

        private async Task<bool> SetStationaryDefaults()
        {
            int externalData = StationarySetting.externalDataTransfer ? 1 : 0;
            int referenceFrequency = StationarySetting.isReferenceFrequency ? StationarySetting.referenceFrequency - BaseFrequency : 0;

            byte[] b = new byte[] { 0x4C, (byte)StationarySetting.antennas, (byte)externalData, (byte)StationarySetting.scanTime, (byte)StationarySetting.scanTimeout,
            (byte)StationarySetting.storeRate, (byte)(referenceFrequency / 256), (byte)(referenceFrequency % 256),
            (byte)StationarySetting.referenceFrequencyStoreRate, (byte)StationarySetting.firstTable, (byte)StationarySetting.secondTable, (byte)StationarySetting.thirdTable };

            bool result = await TransferBLEData.WriteDefaults(false, b);
            return result;
        }

        private bool ExistChanges()
        {
            return (int)StationarySetting.OriginalData[ValueCodes.FIRST_TABLE_NUMBER] != StationarySetting.firstTable || (int)StationarySetting.OriginalData[ValueCodes.SECOND_TABLE_NUMBER] != StationarySetting.secondTable
                || (int)StationarySetting.OriginalData[ValueCodes.THIRD_TABLE_NUMBER] != StationarySetting.thirdTable || (int)StationarySetting.OriginalData[ValueCodes.ANTENNA_NUMBER] != StationarySetting.antennas
                || (int)StationarySetting.OriginalData[ValueCodes.SCAN_RATE] != StationarySetting.scanTime || (int)StationarySetting.OriginalData[ValueCodes.SCAN_TIMEOUT] != StationarySetting.scanTimeout
                || (int)StationarySetting.OriginalData[ValueCodes.STORE_RATE] != StationarySetting.storeRate || (int)StationarySetting.OriginalData[ValueCodes.REFERENCE_FREQUENCY] != StationarySetting.referenceFrequency
                || (int)StationarySetting.OriginalData[ValueCodes.REFERENCE_FREQUENCY_STORE_RATE] != StationarySetting.referenceFrequencyStoreRate
                || (bool)StationarySetting.OriginalData[ValueCodes.EXTERNAL_DATA_TRANSFER] != StationarySetting.externalDataTransfer;
        }

        private bool IsDataCorrect()
        {
            bool scanTimeCorrect = StationarySetting.scanTimeout < StationarySetting.scanTime;
            bool referenceFrequencyCorrect = !StationarySetting.isReferenceFrequency || StationarySetting.referenceFrequency != 0;
            return scanTimeCorrect && referenceFrequencyCorrect;
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
