using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Forms;
using System.IO;
using Plugin.BLE.Abstractions.Contracts;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Plugin.BLE.Abstractions.EventArgs;

namespace VhfReceiver.Pages
{
    public partial class ManageReceiverDataPage : ContentPage
    {
        private List<byte[]> Packets;
        private List<byte[]> PagePackets;
        private List<Snapshots> SnapshotArray;
        private Snapshots RawDataCollector;
        private Snapshots ProcessDataCollector;
        private int FinalPageNumber;
        private int PageNumber;
        private int TotalPackagesNumber;
        private int PacketNumber;
        private bool Error;
        private bool IsDownloading;

        private string Root;
        private string FileName;

        public ManageReceiverDataPage(byte[] bytes)
        {
            InitializeComponent();
            BindingContext = this;

            Toolbar.SetData("Manage Receiver Data", true, Back_Clicked);
            SetData(bytes);
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (Menu.IsVisible)
                await Navigation.PopModalAsync(false);
            else
                SetVisibility("menu");
        }

        private void SetData(byte[] bytes)
        {
            int numberPage = FindPageNumber(new byte[] { bytes[4], bytes[3], bytes[2], bytes[1] });
            int lastPage = FindPageNumber(new byte[] { bytes[8], bytes[7], bytes[6], bytes[5] });
            MemoryUsed.Text = ((int) ((double)(numberPage / lastPage) * 100)) + "%";
            MemoryUsedPercentage.Progress = (double) numberPage / lastPage;
            BytesStored.Text = "Memory Used (" + (numberPage * 2048) + " bytes stored)";
        }

        private void DownloadData_Tapped(object sender, EventArgs e)
        {
            SetVisibility("begin");
        }

        private void ViewData_Tapped(object sender, EventArgs e)
        {
        }

        private void DeleteReceiverData_Tapped(object sender, EventArgs e)
        {
            if (!BytesStored.Text.Contains("(0 bytes"))
                SetVisibility("delete");
            else
                DisplayAlert("Erase Data", "There is no data to delete.", "OK");
        }

        private async void DeleteReceiverData_Clicked(object sender, EventArgs e)
        {
            bool result = await TransferBLEData.DownloadResponse(ValueUpdateDelete);
            if (result)
            {
                byte[] bytes = new byte[] { 0x93 };
                result = await TransferBLEData.WriteResponse(bytes);
                if (result)
                    SetVisibility("deleting");
            }
        }

        private async void ReturnDataScreen_Clicked(object sender, EventArgs e)
        {
            byte[] bytes = await TransferBLEData.ReadDataInfo();
            SetData(bytes);

            SetVisibility("menu");
        }

        private async void BeginDownload_Clicked(object sender, EventArgs e)
        {
            PageNumber = 0;
            PacketNumber = 1; // 9 data packages of 230 bytes
            Error = false;
            Packets = new List<byte[]>();
            PagePackets = new List<byte[]>();
            SnapshotArray = new List<Snapshots>();

            bool result = await TransferBLEData.DownloadResponse(ValueUpdateDownload);

            byte[] resultBytes = await TransferBLEData.ReadPageNumber();
            ReadData(resultBytes);
            if (IsDownloading)
            {
                byte[] b = new byte[] { 0x94 };
                result = await TransferBLEData.WriteResponse(b);
            }
        }

        private void CancelDownload_Clicked(object sender, EventArgs e)
        {
            IsDownloading = false;
            SetVisibility("begin");
        }

        private void ReturnMenu_Clicked(object sender, EventArgs e)
        {
            SetVisibility("menu");
        }

        private void OpenFile_Clicked(object sender, EventArgs e)
        {
        }

        private void ValueUpdateDelete(object o, CharacteristicUpdatedEventArgs args)
        {
            byte[] resultBytes = args.Characteristic.Value;
            DeleteReceiverData.IsEnabled = true;

            if (Converters.GetHexValue(resultBytes).Equals("DD 00 BB EE "))
                SetVisibility("deleted");
            else
                DisplayAlert("Erase Data", "Not completed.", "OK");
        }

        private void ValueUpdateDownload(object o, CharacteristicUpdatedEventArgs args)
        {
            if (FinalPageNumber > 0 && IsDownloading)
            {
                var bytes = args.Characteristic.Value;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DownloadData(bytes, args.Characteristic);
                });
            }
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "menu":
                    Menu.IsVisible = true;
                    DeleteReceiverData.IsVisible = false;
                    DeletingData.IsVisible = false;
                    DeletionComplete.IsVisible = false;
                    BeginDownload.IsVisible = false;
                    DownloadingFile.IsVisible = false;
                    DownloadComplete.IsVisible = false;
                    Toolbar.SetData("Manage Receiver Data");
                    break;
                case "begin":
                    Menu.IsVisible = false;
                    DeleteReceiverData.IsVisible = false;
                    DeletingData.IsVisible = false;
                    DeletionComplete.IsVisible = false;
                    BeginDownload.IsVisible = true;
                    DownloadingFile.IsVisible = false;
                    DownloadComplete.IsVisible = false;
                    Toolbar.SetData("Download Receiver Data");
                    break;
                case "downloading":
                    Menu.IsVisible = false;
                    DeleteReceiverData.IsVisible = false;
                    DeletingData.IsVisible = false;
                    DeletionComplete.IsVisible = false;
                    BeginDownload.IsVisible = false;
                    DownloadingFile.IsVisible = true;
                    DownloadComplete.IsVisible = false;
                    break;
                case "downloaded":
                    Menu.IsVisible = false;
                    DeleteReceiverData.IsVisible = false;
                    DeletingData.IsVisible = false;
                    DeletionComplete.IsVisible = false;
                    BeginDownload.IsVisible = false;
                    DownloadingFile.IsVisible = false;
                    DownloadComplete.IsVisible = true;
                    break;
                case "delete":
                    Menu.IsVisible = false;
                    DeleteReceiverData.IsVisible = true;
                    DeletingData.IsVisible = false;
                    DeletionComplete.IsVisible = false;
                    BeginDownload.IsVisible = false;
                    DownloadingFile.IsVisible = false;
                    DownloadComplete.IsVisible = false;
                    Toolbar.SetData("Delete Receiver Data");
                    break;
                case "deleting":
                    Menu.IsVisible = false;
                    DeleteReceiverData.IsVisible = false;
                    DeletingData.IsVisible = true;
                    DeletionComplete.IsVisible = false;
                    BeginDownload.IsVisible = false;
                    DownloadingFile.IsVisible = false;
                    DownloadComplete.IsVisible = false;
                    Loading.IsRunning = true;
                    break;
                case "deleted":
                    Menu.IsVisible = false;
                    DeleteReceiverData.IsVisible = false;
                    DeletingData.IsVisible = false;
                    DeletionComplete.IsVisible = true;
                    BeginDownload.IsVisible = false;
                    DownloadingFile.IsVisible = false;
                    DownloadComplete.IsVisible = false;
                    Loading.IsRunning = false;
                    break;
            }
        }

        private int FindPageNumber(byte[] packet)
        {
            int pageNumber = packet[0];
            pageNumber = (packet[1] << 8) | pageNumber;
            pageNumber = (packet[2] << 16) | pageNumber;
            pageNumber = (packet[3] << 24) | pageNumber;

            return pageNumber;
        }

        private int FindPacketNumber(byte[] bytes)
        {
            string number = Converters.GetHexValue(bytes);
            int packetNumber = 0;
            switch (number)
            {
                case "11 11 ":
                    packetNumber = 1;
                    break;
                case "22 22 ":
                    packetNumber = 2;
                    break;
                case "33 33 ":
                    packetNumber = 3;
                    break;
                case "44 44 ":
                    packetNumber = 4;
                    break;
                case "55 55 ":
                    packetNumber = 5;
                    break;
                case "66 66 ":
                    packetNumber = 6;
                    break;
                case "77 77 ":
                    packetNumber = 7;
                    break;
                case "88 88 ":
                    packetNumber = 8;
                    break;
                case "99 99 ":
                    packetNumber = 9;
                    break;
            }
            return packetNumber;
        }

        private bool IsTransmissionDone(byte[] bytes)
        {
            return Converters.GetHexValue(bytes).Equals("DD 00 BB EE ");
        }

        private bool IsErrorPacket(byte[] bytes)
        {
            return Converters.GetHexValue(bytes).Equals("AA BB CC DD EE ");
        }

        private async void ReadData(byte[] bytes)
        {
            FinalPageNumber = FindPageNumber(new byte[] { bytes[3], bytes[2], bytes[1], bytes[0] });
            TotalPackagesNumber = FinalPageNumber * 9;
            Console.WriteLine("Total: " + FinalPageNumber);
            RawDataCollector = new Snapshots(FinalPageNumber * Snapshots.BYTES_PER_PAGE);
            IsDownloading = FinalPageNumber > 0;

            if (IsDownloading)
            {
                ProcessStates.SetPercent(0);
                ProcessStates.Initialize();
                SetVisibility("downloading");
                ProcessStates.InitFirstState("Downloading Data");
            }
            else // No data to download
            {
                SetVisibility("menu");
                await DisplayAlert("Message", "No data to download.", "OK");
            }
        }

        private void DownloadData(byte[] bytes, ICharacteristic characteristic)
        {
            if (bytes.Length == 4 && IsDownloading && IsTransmissionDone(bytes))
            {
                ProcessStates.InitSecondState("Processing Data");
                CheckPackets(characteristic);
            }
            if (IsDownloading)
            {
                if (PagePackets.Count == 0)
                {
                    Task.Delay(ValueCodes.DOWNLOAD_PERIOD).ContinueWith(async t =>
                    {
                        byte[] b = new byte[] { 0x96 };
                        if (PagePackets.Count >= 9 && IsDownloading)
                        {
                            if (FindPacketNumber(new byte[] { PagePackets[PagePackets.Count - 1][228], PagePackets[PagePackets.Count - 1][229] }) == 9)
                            {
                                int number = FindPageNumber(new byte[] { PagePackets[PagePackets.Count - 1][224], PagePackets[PagePackets.Count - 1][225], PagePackets[PagePackets.Count - 1][226], PagePackets[PagePackets.Count - 1][227] });
                                if (number == PageNumber)
                                {
                                    PageNumber++;
                                    foreach (byte[] pagePacket in PagePackets)
                                    {
                                        number = FindPacketNumber(new byte[] { pagePacket[228], pagePacket[229] });
                                        if (number == PacketNumber)
                                            PacketNumber++;
                                    }
                                    if (PacketNumber == 10)
                                    {
                                        PacketNumber = 1;
                                        foreach (byte[] pagePacket in PagePackets)
                                        {
                                            number = FindPacketNumber(new byte[] { pagePacket[228], pagePacket[229] });
                                            if (number == PacketNumber - 1)
                                            {
                                                Packets[number - 1] = pagePacket;
                                            }
                                            else
                                            {
                                                Packets.Add(pagePacket);
                                                PacketNumber++;
                                            }
                                        }
                                        b[0] = 0x95;
                                        int percent = (int)(((float)PageNumber / (float)FinalPageNumber) * 100);
                                        ProcessStates.SetPercent(percent);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Packet number " + PacketNumber + " no es igual a 10");
                                        PageNumber--;
                                    }
                                }
                                else
                                    Console.WriteLine("Number " + number + " no es igual a PageNumber " + PageNumber);
                            }
                            else
                                Console.WriteLine("El ultimo Packet number no es igual a 9.");
                        }
                        else
                            Console.WriteLine("Cantidad no es mayor o igual a 9");
                        PacketNumber = 1;
                        PagePackets = new List<byte[]>();
                        bool result = await characteristic.WriteAsync(b);
                    });
                }
                PagePackets.Add(bytes);
                Console.WriteLine(Converters.GetHexValue(bytes));
            }
        }

        private void CheckPackets(ICharacteristic characteristic)
        {
            characteristic.ValueUpdated -= ValueUpdateDownload;
            foreach (byte[] packet in Packets)
            {
                byte[] finalPacket;
                int extraNumbers = 2;
                if (packet.Length == 5 && IsErrorPacket(packet)) // Shows an error when the packet contains 5 bytes and stops downloading
                {
                    SetVisibility("menu");
                    DisplayAlert("Error", "Download Error (Packet error).", "OK");
                    IsDownloading = false;
                    Error = true;
                    return;
                }
                else //Copy the downloaded package
                {
                    if (PacketNumber % 9 == 0)
                        extraNumbers = 6;
                    finalPacket = new byte[packet.Length - extraNumbers];
                    Array.Copy(packet, 0, finalPacket, 0, packet.Length - extraNumbers);
                    RawDataCollector.ProcessSnapshotRaw(finalPacket);
                    PacketNumber++;
                }
            }
            if (!Error)
            {
                SnapshotArray.Add(RawDataCollector);
                string processData = Converters.ProcessData(RawDataCollector.GetSnapshot());
                byte[] data = Converters.ConvertToUTF8(processData);
                ProcessDataCollector = new Snapshots(data.Length);
                ProcessDataCollector.ProcessSnapshot(data);
                SnapshotArray.Add(ProcessDataCollector);

                ProcessStates.InitThirdState("Preparing File");
                SaveFiles();
            }
        }

        private void SaveFiles()
        {
            Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "atstrack");
            FileName = SnapshotArray[SnapshotArray.Count - 1].GetFileName();
            bool result = Converters.PrintSnapshotFiles(Root, SnapshotArray);

            ProcessStates.FinishProcess();
            if (result)
            {
                string message = "Download finished: " + (Snapshots.BYTES_PER_PAGE * FinalPageNumber) + " byte(s) downloaded.";
                if (Error)
                {
                    message += " No data found in bytes downloaded. No file was generated. Total Number of Packages: " + Packets.Count + ". Expected: " + (FinalPageNumber * 9);
                    if (Packets.Count != TotalPackagesNumber)
                        message += ". Timeout.";
                    else if (Snapshots.BYTES_PER_PAGE * PageNumber == Snapshots.BYTES_PER_PAGE * FinalPageNumber)
                        message += ". Not successfully";
                    DisplayAlert("Finished", message, "OK");
                }
                else
                {
                    ShowDisplayAlert("Finished", message);
                }
            }
            IsDownloading = false;
            SetVisibility("downloaded");
        }

        private async void ShowDisplayAlert(string title, string message)
        {
            await DisplayAlert(title, message, "OK");

            var response = await DisplayAlert(title, "Do you want to send the file to the cloud?", "OK", "Cancel");

            if (response)
            {
                RequestSignIn();
            }
        }

        private void RequestSignIn()
        {
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
