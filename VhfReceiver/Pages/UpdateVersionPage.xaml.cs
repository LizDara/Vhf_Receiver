using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class UpdateVersionPage : ContentPage
    {
        private byte[] FirmwareFile;
        private readonly int MTU = 247;
        private int Index;

        public UpdateVersionPage()
        {
            InitializeComponent();

            Toolbar.SetData("Firmware Update", false);
            SetVisibility("menu");
        }

        private void Begin_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<FileInformation>(this, ValueCodes.FILE, (value) =>
            {
                ProcessStates.Initialize();
                SetVisibility("updating");
                ProcessStates.InitFirstState("Downloading File");
                Console.WriteLine(value.FilePath + " " + value.FileName);
                FirmwareFile = File.ReadAllBytes(value.FilePath);
                FileName.Text = value.FileName;
                CheckingFile();
            });

            var popMessage = new DocumentPicker("versions");
            _ = App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
        }

        private void CancelUpdate_Clicked(object sender, EventArgs e)
        {
            SetVisibility("menu");
        }

        private async void ReturnMenu_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(false);
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "menu":
                    UpdateMessage.IsVisible = true;
                    UpdatingProcess.IsVisible = false;
                    InstallationComplete.IsVisible = false;
                    break;
                case "updating":
                    UpdateMessage.IsVisible = false;
                    UpdatingProcess.IsVisible = true;
                    InstallationComplete.IsVisible = false;
                    break;
                case "complete":
                    UpdateMessage.IsVisible = false;
                    UpdatingProcess.IsVisible = false;
                    InstallationComplete.IsVisible = true;
                    break;
            }
        }

        private void CheckingFile()
        {
            ProcessStates.InitSecondState("Checking File");
            SetOtaBegin();
        }

        private async void SetOtaBegin()
        {
            byte[] b = new byte[] { 0x00 };
            bool result = await TransferBLEData.WriteOTA(b);
            if (result)
                RequestMTU();
        }

        private async void RequestMTU()
        {
            bool result = await TransferBLEData.RequestMtu(MTU + 3);
            if (result)
                OtaUpload();
        }

        private void OtaUpload()
        {
            ProcessStates.InitThirdState("Installing Firmware");
            Index = 0;
            Task.Run(async () => {
                bool last = false;
                int packageCount = 0;
                while (!last)
                {
                    byte[] payload;
                    if (Index + MTU >= FirmwareFile.Length)
                    {
                        int restSize = FirmwareFile.Length - Index;
                        payload = new byte[restSize];
                        Array.Copy(FirmwareFile, Index, payload, 0, restSize); // copy rest bytes
                        last = true;
                    }
                    else
                    {
                        payload = new byte[MTU];
                        Array.Copy(FirmwareFile, Index, payload, 0, MTU);
                    }
                    Console.WriteLine($"OTA index :{Index} firmware length:{FirmwareFile.Length}");
                    while (!await TransferBLEData.WriteOTA(payload)) // attempt to write until getting success
                    {
                        Thread.Sleep(5);
                    }

                    packageCount++;
                    Index += MTU;
                }
                Console.WriteLine("OTA UPLOAD SEND DONE");
                OtaEnd();
            });
        }

        private async void OtaEnd()
        {
            byte[] b = new byte[] { 0x03 };
            int i = 0;
            while (!await TransferBLEData.WriteOTA(b))
            {
                i++;
                Console.WriteLine("OTA", "Failed to write end 0x03 retry:" + i);
            }
            RebootTargetDevice();
        }

        private async void RebootTargetDevice()
        {
            ProcessStates.FinishProcess();
            byte[] b = new byte[] { 0x04 };
            bool result = await TransferBLEData.WriteOTA(b);
            if (result)
                SetVisibility("complete");
        }
    }
}
