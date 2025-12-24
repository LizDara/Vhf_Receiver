using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VhfReceiver.Pages;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class MobileSetting : ContentView
    {
        public Dictionary<string, object> OriginalData;
        public byte[] MobileBytes;
        private bool IsTemporary;
        public int tableNumber = 255;
        public double scanTime = 255;
        public bool autoRecord = true;
        public bool gps = true;

        public MobileSetting()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<int[]>(this, ValueCodes.VALUE, async (value) => {
                switch (value[0])
                {
                    case ValueCodes.TABLE_NUMBER_CODE:
                        FrequencyTableNumber.Text = value[1].ToString();
                        tableNumber = value[1];
                        break;
                    case ValueCodes.SCAN_RATE_SECONDS_CODE:
                        scanTime = value[1] * 0.1;
                        ScanTime.Text = scanTime.ToString().Contains(".") ? scanTime.ToString() : scanTime.ToString() + ".0";
                        break;
                }
                if (IsTemporary)
                {
                    bool result = await SetTemporary(value[0]);
                    if (!result)
                    {
                        OriginalData = null;
                        SetData(MobileBytes, true);
                    }
                }
            });
        }

        public void SetData(byte[] bytes, bool isTemporary)
        {
            MobileBytes = bytes;
            IsTemporary = isTemporary;
            if (!Converters.IsDefaultEmpty(bytes))
            {
                FrequencyTableNumber.Text = (MobileBytes[1] == 0) ? "None" : MobileBytes[1].ToString();
                tableNumber = MobileBytes[1];

                gps = (MobileBytes[2] >> 7 & 1) == 1;
                GPS.IsToggled = gps;

                autoRecord = (MobileBytes[2] >> 6 & 1) == 1;
                AutoRecord.IsToggled = autoRecord;

                scanTime = MobileBytes[3] * 0.1;
                ScanTime.Text = scanTime.ToString().Contains(".") ? scanTime.ToString() : scanTime.ToString() + ".0";
            }
            else
            {
                FrequencyTableNumber.Text = "Not Set";
                ScanTime.Text = "Not Set";
                GPS.IsToggled = true;
                AutoRecord.IsToggled = true;
            }

            OriginalData = new Dictionary<string, object>
            {
                { ValueCodes.TABLE_NUMBER, tableNumber },
                { ValueCodes.GPS, gps },
                { ValueCodes.AUTO_RECORD, autoRecord },
                { ValueCodes.SCAN_RATE, scanTime }
            };
        }

        private async void FrequencyTableNumber_Tapped(object sender, EventArgs e)
        {
            byte[] bytes = await TransferBLEData.ReadTables();
            if (bytes != null)
                await Navigation.PushModalAsync(new ValueDefaultsPage(MobileBytes, bytes, ValueCodes.TABLE_NUMBER_CODE), false);
        }

        private async void ScanTime_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ValueDefaultsPage(MobileBytes, ValueCodes.SCAN_RATE_SECONDS_CODE), false);
        }

        private async void AutoRecord_Toggled(object sender, ToggledEventArgs e)
        {
            autoRecord = e.Value;
            if (IsTemporary && OriginalData != null)
            {
                AutoRecord.IsEnabled = false;
                bool result = await SetTemporary(ValueCodes.AUTO_RECORD_CODE);
                if (result)
                {
                    AutoRecord.IsEnabled = true;
                }
                else
                {
                    OriginalData = null;
                    SetData(MobileBytes, true);
                }
            }
        }

        private async void Gps_Toggled(object sender, ToggledEventArgs e)
        {
            gps = e.Value;
            if (IsTemporary && OriginalData != null)
            {
                GPS.IsEnabled = false;
                bool result = await SetTemporary(ValueCodes.GPS_CODE);
                if (result)
                {
                    GPS.IsEnabled = true;
                }
                else
                {
                    OriginalData = null;
                    SetData(MobileBytes, true);
                }
            }
        }

        private async Task<bool> SetTemporary(int type)
        {
            byte[] b = new byte[] { 0x6F, MobileBytes[1], MobileBytes[2], MobileBytes[3] };
            switch (type)
            {
                case ValueCodes.TABLE_NUMBER_CODE:
                    b[1] = (byte)tableNumber;
                    break;
                case ValueCodes.SCAN_RATE_SECONDS_CODE:
                    b[3] = (byte)scanTime;
                    break;
                case ValueCodes.GPS_CODE:
                    b[2] = (byte)(gps ? MobileBytes[2] + 0x80 : MobileBytes[2] - 0x80);
                    break;
                case ValueCodes.AUTO_RECORD_CODE:
                    b[2] = (byte)(autoRecord ? MobileBytes[2] + 0x40 : MobileBytes[2] - 0x40);
                    break;
            }
            bool result = await TransferBLEData.WriteDefaults(true, b);
            return result;
        }
    }
}
