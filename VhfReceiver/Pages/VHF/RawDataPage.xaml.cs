using System;
using System.Collections.Generic;
using System.IO;
using Plugin.BLE.Abstractions.EventArgs;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Xamarin.Forms;

namespace VhfReceiver.Pages.VHF
{
    public partial class RawDataPage : ContentPage
    {
        private byte[] RawData;

        public RawDataPage()
        {
            InitializeComponent();

            Toolbar.SetData("Convert Raw Data (SD Card)", true);
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery
        }

        private void SelectFile_Tapped(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<FileInformation>(this, ValueCodes.FILE, (value) =>
            {
                Console.WriteLine(value.FilePath + " " + value.FileName);
                RawData = File.ReadAllBytes(value.FilePath);
                SelectFile.IsVisible = false;
                string[] fileName = value.FileName.Split('.');
                FileName.Text = fileName[0];
                FileDescription.Text = fileName[1] + " - " + (((float)(RawData.Length / 1024)) / 1000).ToString() + " MB";
                SelectedFile.IsVisible = true;
                ConvertRawData.Opacity = 1;
                ConvertRawData.IsEnabled = true;
            });

            var popMessage = new DocumentPicker("sdcard");
            _ = App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            SelectedFile.IsVisible = ConvertRawData.IsEnabled = false;
            SelectFile.IsVisible = true;
            ConvertRawData.Opacity = 0.6;
        }

        private void ConvertRawData_Clicked(object sender, EventArgs e)
        {
            ConvertingRawPercentage.Progress = 0;
            FileSource.IsVisible = FileConverted.IsVisible = false;
            FileConversion.IsVisible = ConvertingRawFile.IsVisible = true;
            //ConvertingRawPercentage.Progress = 0.2;

            string processData = Converters.ProcessData(RawData);
            byte[] data = Converters.ConvertToUTF8(processData);
            List<Snapshots> snapshotArray = new List<Snapshots>();
            Snapshots processed = new Snapshots(data.Length);
            processed.ProcessSnapshot(data);
            snapshotArray.Add(processed);
            //ConvertingRawPercentage.Progress = 0.8;

            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sdcard");
            bool result = Converters.PrintSnapshotFiles(root, snapshotArray);
            ConvertingRawPercentage.Progress = 1;
            if (result)
            {
                FileConverted.IsVisible = true;
                ConvertingRawFile.IsVisible = false;
                FileNameSaved.Text = "File saved as " + processed.GetFileName();
            }
        }

        private void CancelConversion_Clicked(object sender, EventArgs e)
        {
            FileSource.IsVisible = true;
            FileConversion.IsVisible = false;
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
