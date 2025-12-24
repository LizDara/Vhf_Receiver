using System;
using Plugin.BLE.Abstractions.EventArgs;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class StartScanningPage : ContentPage
    {
        private byte[] DetectionBytes;
        private byte[] TablesBytes;
        private byte[] DefaultBytes;
        private bool IsDetectionFilterEmpty;
        private bool AreTablesEmpty;
        private bool IsDefaultEmpty = false;

        public StartScanningPage(byte[] detection, byte[] tables) //Coming from Home
        {
            Initialize();

            DetectionBytes = detection;
            TablesBytes = tables;
            CheckDetection();
            CheckTables();
        }

        public StartScanningPage(byte[] tables) //Coming from Mobile Scan
        {
            Initialize();

            IsDetectionFilterEmpty = false;
            TablesBytes = tables;
            CheckTables();
        }

        public StartScanningPage() //Coming from Manual and Stationary Scan
        {
            Initialize();

            IsDetectionFilterEmpty = false;
            AreTablesEmpty = false;
        }

        private void Initialize()
        {
            InitializeComponent();
            Toolbar.SetData("Start Scanning", true, Back_Clicked);
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new HomePage(), false);
        }

        private async void ManualScan_Tapped(object sender, EventArgs e)
        {
            if (IsDetectionFilterEmpty)
                ShowNoDetectionFilter();
            else
                await Navigation.PushModalAsync(new ManualScanningPage(), false);
        }
        
        private async void MobileScan_Tapped(object sender, EventArgs e)
        {
            DefaultBytes = await TransferBLEData.ReadDefaults(true);
            if (DefaultBytes != null)
            {
                CheckDefaults();
                if (IsDetectionFilterEmpty)
                    ShowNoDetectionFilter();
                else if (AreTablesEmpty)
                    ShowNoTables();
                else if (IsDefaultEmpty)
                    ShowNoDefaultSetting();
                else
                    await Navigation.PushModalAsync(new MobileScanningPage(DefaultBytes), false);
            }
        }

        private async void StationaryScan_Tapped(object sender, EventArgs e)
        {
            DefaultBytes = await TransferBLEData.ReadDefaults(false);
            if (DefaultBytes != null)
            {
                CheckDefaults();
                if (IsDetectionFilterEmpty)
                    ShowNoDetectionFilter();
                else if (AreTablesEmpty)
                    ShowNoTables();
                else if (IsDefaultEmpty)
                    ShowNoDefaultSetting();
                else
                    await Navigation.PushModalAsync(new StationaryScanningPage(DefaultBytes), false);
            }
        }

        private async void GoTo_Clicked(object sender, EventArgs e)
        {
            Button goTo = (Button)sender;
            MessagingCenter.Subscribe<byte[]>(this, goTo.Text, (value) =>
            {
                switch (goTo.Text)
                {
                    case ValueCodes.DETECTION_SCAN:
                        DetectionBytes = value;
                        CheckDetection();
                        break;
                    case ValueCodes.TABLE_SCAN:
                        TablesBytes = value;
                        CheckTables();
                        break;
                    case ValueCodes.DEFAULTS_SCAN:
                        DefaultBytes = value;
                        CheckDefaults();
                        break;
                }
            });
            switch (goTo.Text)
            {
                case ValueCodes.DETECTION_SCAN:
                    await Navigation.PushModalAsync(new SelectDetectionFilterPage(DetectionBytes, true), false);
                    break;
                case ValueCodes.TABLE_SCAN:
                    await Navigation.PushModalAsync(new TablesPage(TablesBytes, true), false);
                    break;
                case ValueCodes.DEFAULTS_SCAN:
                    if (Converters.GetHexValue(DefaultBytes[0]).Equals("6D"))
                        await Navigation.PushModalAsync(new MobileDefaultsPage(DefaultBytes, true), false);
                    else
                        await Navigation.PushModalAsync(new StationaryDefaultsPage(DefaultBytes, true), false);
                    break;
            }
            Warning.IsVisible = false;
            Menu.IsVisible = true;
        }

        private void CheckDetection()
        {
            IsDetectionFilterEmpty = false;
            if (!Converters.GetHexValue(DetectionBytes[1]).Equals("09"))
                IsDetectionFilterEmpty = DetectionBytes[2] == 0 && DetectionBytes[3] == 0 && DetectionBytes[4] == 0
                    && DetectionBytes[5] == 0 && DetectionBytes[6] == 0 && DetectionBytes[7] == 0 && DetectionBytes[8] == 0
                    && DetectionBytes[9] == 0 && DetectionBytes[10] == 0 && DetectionBytes[11] == 0;
        }

        private void CheckTables()
        {
            AreTablesEmpty = TablesBytes[1] == 0 && TablesBytes[2] == 0 && TablesBytes[3] == 0 && TablesBytes[4] == 0 && TablesBytes[5] == 0
                && TablesBytes[6] == 0 && TablesBytes[7] == 0 && TablesBytes[8] == 0 && TablesBytes[9] == 0 && TablesBytes[10] == 0
                && TablesBytes[11] == 0 && TablesBytes[12] == 0;
        }

        private void CheckDefaults()
        {
            IsDefaultEmpty = Converters.IsDefaultEmpty(DefaultBytes);
        }

        private void ShowNoDetectionFilter()
        {
            Menu.IsVisible = false;
            Warning.IsVisible = true;
            WarningMessage.Text = "Warning: Receiver detection filter not set. Please complete to be able to scan.";
            GoToPage.Text = "Go to Detection Filter Settings";
        }

        private void ShowNoTables()
        {
            Menu.IsVisible = false;
            Warning.IsVisible = true;
            WarningMessage.Text = "Warning: All frequency tables are empty. You must fill one before starting a mobile or stationary scan.";
            GoToPage.Text = "Go to Frequency Tables";
        }

        private void ShowNoDefaultSetting()
        {
            Menu.IsVisible = false;
            Warning.IsVisible = true;
            WarningMessage.Text = "Warning: Receiver default settings not complete. Please complete settings to be able to scan.";
            GoToPage.Text = "Go to Receiver Settings";
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
