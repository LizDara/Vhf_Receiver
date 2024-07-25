using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Forms;
using System.IO;
using Plugin.BLE.Abstractions.Contracts;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace VhfReceiver.Pages
{
    public partial class ManageReceiverDataPage : ContentPage
    {
        private readonly char CR = (char)0x0D;
        private readonly char LF = (char)0x0A;
        private readonly ReceiverInformation ReceiverInformation;

        private List<byte[]> Packets;
        private List<Snapshots> SnapshotArray;
        private Snapshots RawDataCollector;
        private Snapshots ProcessDataCollector;
        private int FinalPageNumber;
        private int PageNumber;
        private int TotalPackagesNumber;
        private int PacketNumber;
        private bool Error;
        private bool IsCanceled;

        private string Root;
        private string FileName;

        public ManageReceiverDataPage(byte[] bytes)
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();

            SetData(bytes);
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (Menu.IsVisible)
                await Navigation.PopModalAsync(false);
            if (DeleteReceiverData.IsVisible || DeletionComplete.IsVisible)
                SetVisibility("menu");
        }

        private void SetData(byte[] bytes)
        {
            int numberPage = FindPageNumber(new byte[] { bytes[18], bytes[17], bytes[16], bytes[15] });
            int lastPage = FindPageNumber(new byte[] { bytes[22], bytes[21], bytes[20], bytes[19] });
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
            {
                SetVisibility("delete");
            }
            else
            {
                DisplayAlert("Erase Data", "There is no data to delete.", "OK");
            }
        }

        private async void DeleteReceiverData_Clicked(object sender, EventArgs e)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STUDY_DATA);
                    if (characteristic != null)
                    {
                        characteristic.ValueUpdated += (o, args) =>
                        {
                            byte[] resultBytes = args.Characteristic.Value;

                            DeleteReceiverData.IsEnabled = true;

                            if (Converters.GetHexValue(resultBytes).Equals("DD 00 BB EE "))
                                SetVisibility("deleted");
                            else
                                DisplayAlert("Erase Data", "Not completed.", "OK");
                        };

                        await characteristic.StartUpdatesAsync();

                        byte[] bytes = new byte[] { 0x93 };

                        bool result = await characteristic.WriteAsync(bytes);

                        if (result)
                            SetVisibility("deleting");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Service: " + ex.Message);
            }
        }

        private async void ReturnDataScreen_Clicked(object sender, EventArgs e)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_DIAGNOSTIC);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_DIAGNOSTIC_INFO);
                    if (characteristic != null)
                    {
                        byte[] bytes = await characteristic.ReadAsync();
                        SetData(bytes);

                        SetVisibility("menu");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Service: " + ex.Message);
            }
        }

        private async void BeginDownload_Clicked(object sender, EventArgs e)
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STUDY_DATA);
                    if (characteristic != null)
                    {
                        PageNumber = -1;
                        PacketNumber = 0; // 9 data packages of 228 bytes
                        Error = false;
                        IsCanceled = false;
                        Packets = new List<byte[]>();
                        SnapshotArray = new List<Snapshots>();

                        characteristic.ValueUpdated += (o, args) =>
                        {
                            if (FinalPageNumber > 0 && !IsCanceled)
                            {
                                var bytes = args.Characteristic.Value;

                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    if (bytes.Length > 4)
                                        DownloadData(bytes);
                                    else if (IsTransmissionDone(bytes))
                                        DownloadData(characteristic);
                                });
                            }
                        };
                        await characteristic.StartUpdatesAsync();

                        byte[] resultBytes = await characteristic.ReadAsync();
                        ReadData(resultBytes, characteristic);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Service: " + ex.Message);
            }
        }

        private void CancelDownload_Clicked(object sender, EventArgs e)
        {
            IsCanceled = true;
            SetVisibility("begin");
        }

        private void ReturnMenu_Clicked(object sender, EventArgs e)
        {
            SetVisibility("menu");
        }

        private void OpenFile_Clicked(object sender, EventArgs e)
        {
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
                    TitleToolbar.Text = "Manage Receiver Data";
                    break;
                case "begin":
                    Menu.IsVisible = false;
                    DeleteReceiverData.IsVisible = false;
                    DeletingData.IsVisible = false;
                    DeletionComplete.IsVisible = false;
                    BeginDownload.IsVisible = true;
                    DownloadingFile.IsVisible = false;
                    DownloadComplete.IsVisible = false;
                    TitleToolbar.Text = "Download Receiver Data";
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
                    TitleToolbar.Text = "Delete Receiver Data";
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

        private void InitDownloading()
        {
            DownloadingStatus.Source = "LightCircle";
            DownloadingData.TextColor = Color.FromRgb(123, 135, 148);
            Downloading.IsVisible = false;
            ProcessingStatus.Source = "LightCircle";
            ProcessingData.TextColor = Color.FromRgb(123, 135, 148);
            Processing.IsVisible = false;
            PreparingStatus.Source = "LightCircle";
            PreparingFile.TextColor = Color.FromRgb(123, 135, 148);
            Preparing.IsVisible = false;
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
                    packetNumber = 1;
                    break;
                case "33 33 ":
                    packetNumber = 1;
                    break;
                case "44 44 ":
                    packetNumber = 1;
                    break;
                case "55 55 ":
                    packetNumber = 1;
                    break;
                case "66 66 ":
                    packetNumber = 1;
                    break;
                case "77 77 ":
                    packetNumber = 1;
                    break;
                case "88 88 ":
                    packetNumber = 1;
                    break;
                case "99 99 ":
                    packetNumber = 1;
                    break;
            }
            return (9 * (PageNumber + 1)) + packetNumber;
        }

        private bool IsTransmissionDone(byte[] bytes)
        {
            return Converters.GetHexValue(bytes).Equals("DD 00 BB EE ");
        }

        private bool IsErrorPacket(byte[] bytes)
        {
            return Converters.GetHexValue(bytes).Equals("AA BB CC DD EE ");
        }

        private async void ReadData(byte[] bytes, ICharacteristic characteristic)
        {
            FinalPageNumber = FindPageNumber(new byte[] { bytes[3], bytes[2], bytes[1], bytes[0] });
            TotalPackagesNumber = FinalPageNumber * 9;
            Console.WriteLine("Total: " + FinalPageNumber);
            RawDataCollector = new Snapshots(FinalPageNumber * Snapshots.BYTES_PER_PAGE);

            if (FinalPageNumber == 0) // No data to download
            {
                SetVisibility("menu");
                Console.WriteLine("Downloading status");
                await DisplayAlert("Message", "No data to download.", "OK");
                await characteristic.StopUpdatesAsync();
            }
            else
            {
                InitDownloading();
                SetVisibility("downloading");
                DownloadingStatus.Source = "GreenCircle";
                DownloadingData.TextColor = Color.FromRgb(31, 41, 51);
                Downloading.IsVisible = true;

                _ = Task.Delay(4000 * FinalPageNumber).ContinueWith(async t =>
                {
                    //Console.WriteLine("PacketsCount: " + Packets.Count + " TotalPackagesNumber: " + TotalPackagesNumber + " IsFilled: " + RawDataCollector.IsFilled() + " Error: " + Error);
                    if ((Packets.Count != TotalPackagesNumber || !RawDataCollector.IsFilled()) && !Error)
                    {
                        Error = true;
                        await characteristic.StopUpdatesAsync();
                        PrintSnapshotFiles();
                    }
                });
            }
        }

        private void DownloadData(byte[] bytes)
        {
            if (!IsCanceled)
                Packets.Add(bytes);
        }

        private void DownloadData(ICharacteristic characteristic)
        {
            if (!IsCanceled)
                CheckPackets(characteristic);
        }

        private void CheckPackets(ICharacteristic characteristic)
        {
            characteristic.StopUpdatesAsync();
            foreach (byte[] packet in Packets)
            {
                if (packet.Length == 5 && IsErrorPacket(packet)) // Shows an error when the packet contains 5 bytes and stops downloading
                {
                    SetVisibility("menu");
                    DisplayAlert("Error", "Download Error (Packet error).", "OK");
                    Error = true;
                    return;
                }
                else if ((PacketNumber + 1) == FindPacketNumber(new byte[] { packet[228], packet[229] })) // Copy the downloaded package
                {
                    PacketNumber++;
                    byte[] finalPacket;
                    if (PacketNumber % 9 == 0)
                    {
                        if ((PageNumber + 1) == FindPageNumber(new byte[] { packet[224], packet[225], packet[226], packet[227] }) && (PageNumber + 1) < FinalPageNumber)
                        {
                            PageNumber++;
                            finalPacket = new byte[224];
                            Array.Copy(packet, 0, finalPacket, 0, 224);
                        }
                        else
                        {
                            SetVisibility("menu");
                            DisplayAlert("Error", "Download Error (Page number).", "OK");
                            Error = true;
                            return;
                        }
                    }
                    else
                    {
                        finalPacket = new byte[228];
                        Array.Copy(packet, 0, finalPacket, 0, 228);
                    }
                    RawDataCollector.ProcessSnapshotRaw(finalPacket);
                }
            }
            if (!Error)
            {
                SnapshotArray.Add(RawDataCollector);
                string processData = ProcessData(RawDataCollector.GetSnapshot());

                if (!Error)
                {
                    byte[] data = Converters.ConvertToUTF8(processData);
                    ProcessDataCollector = new Snapshots(data.Length);
                    ProcessDataCollector.ProcessSnapshot(data);
                    SnapshotArray.Add(ProcessDataCollector);
                }
                PrintSnapshotFiles();
            }
        }

        private string GetFrequency(int frequency)
        {
            return frequency.ToString().Substring(0, 3) + "." + frequency.ToString().Substring(3);
        }

        private string ProcessData(byte[] bytes)
        {
            DownloadingStatus.Source = "SmallChecked";
            Downloading.IsVisible = false;
            ProcessingStatus.Source = "GreenCircle";
            ProcessingData.TextColor = Color.FromRgb(31, 41, 51);
            Processing.IsVisible = true;
            Console.WriteLine("Processing status");

            int baseFrequency = Preferences.Get("BaseFrequency", 0) * 1000;
            string data = "Year, JulianDay, Hour, Min, Sec, Ant, Index, Freq, SS, Code, Mort, NumDet, Lat, Long, GpsAge, Date, PrmNum" + CR + LF;
            int index = 0;
            int frequency = 0;
            int frequencyTableIndex = 0;
            int year;
            DateTime dateTime = DateTime.Now;

            while (index < bytes.Length)
            {
                string format = Converters.GetHexValue(bytes[index]);
                if (format.Equals("83") || format.Equals("82"))
                {
                    year = bytes[6];
                    index += 8;

                    while (index < bytes.Length && !format.Equals("83") && !format.Equals("82") && !format.Equals("86"))
                    {
                        if (Converters.GetHexValue(bytes[index]).Equals("F0"))
                        {
                            frequency = baseFrequency + ((bytes[index + 1] * 256) + bytes[index + 2]);
                            frequencyTableIndex = bytes[index + 3];

                            int date = Converters.HexToDecimal(
                                Converters.GetHexValue(bytes[index + 4])
                                + Converters.GetHexValue(bytes[index + 5])
                                + Converters.GetHexValue(bytes[index + 6]));

                            int month = date / 1000000;
                            date = month % 1000000;
                            int day = date / 10000;
                            date %= 10000;
                            int hour = date / 100;
                            int minute = date % 100;
                            int seconds = bytes[index + 7];
                            dateTime = new DateTime(year + 2000, month, day, hour, minute, seconds);
                        }
                        else if (Converters.GetHexValue(bytes[index]).Equals("F1"))
                        {
                            int secondsOffset = bytes[index + 1];
                            int antenna = bytes[index + 2] > 128 ? bytes[index + 2] - 128 : bytes[index + 2];
                            int signalStrength = bytes[index + 4];
                            int code = bytes[index + 3];
                            int mort = bytes[index + 5];
                            int numberDetection = bytes[index + 7];
                            dateTime.AddSeconds(secondsOffset);

                            data += (dateTime.Year - 2000) + ", " + dateTime.DayOfYear + ", " + dateTime.Hour + ", " + dateTime.Minute + ", " + dateTime.Second
                                + ", " + antenna + ", " + frequencyTableIndex + ", " + GetFrequency(frequency) + ", " + signalStrength + ", " + code + ", " + mort
                                + ", " + numberDetection + ", 0, 0, 0, " + (dateTime.Month + "/" + dateTime.Day + "/" + (dateTime.Year - 2000)) + ", 0" + CR + LF;
                        }
                        else if (Converters.GetHexValue(bytes[index]).Equals("F2"))
                        {
                            int antenna = bytes[index + 2] > 128 ? bytes[index + 2] - 128 : bytes[index + 2];
                            int signalStrength = bytes[index + 4];
                            int code = bytes[index + 3];
                            int mort = (bytes[index + 6] * 256) + bytes[index + 5];
                            int numberDetection = (bytes[index + 1] * 256) + bytes[index + 7];

                            data += (dateTime.Year - 2000) + ", " + dateTime.DayOfYear + ", " + dateTime.Hour + ", " + dateTime.Minute + ", " + dateTime.Second
                                + ", " + antenna + ", " + frequencyTableIndex + ", " + GetFrequency(frequency) + ", " + signalStrength + ", " + code + ", " + mort
                                + ", " + numberDetection + ", 0, 0, 0, " + (dateTime.Month + "/" + dateTime.Day + "/" + (dateTime.Year - 2000)) + ", 0" + CR + LF;
                        }
                        else if (Converters.GetHexValue(bytes[index]).Equals("E1") || Converters.GetHexValue(bytes[index]).Equals("E2"))
                        {
                            int secondsOffset = bytes[index + 1];
                            int antenna = bytes[index + 2] % 10;
                            int signalStrength = bytes[index + 4];
                            dateTime.AddSeconds(secondsOffset);

                            data += (dateTime.Year - 2000) + ", " + dateTime.DayOfYear + ", " + dateTime.Hour + ", " + dateTime.Minute + ", " + dateTime.Second
                                + ", " + antenna + ", " + frequencyTableIndex + ", " + GetFrequency(frequency) + ", " + signalStrength + ", 0, 0, 0, 0, 0, 0, "
                                + (dateTime.Month + "/" + dateTime.Day + "/" + (dateTime.Year - 2000)) + ", 0" + CR + LF;
                        }
                        index += 8;
                    }
                }
                else if (format.Equals("86"))
                {
                    year = bytes[index + 2];
                    int month = bytes[index + 3];
                    int day = bytes[index + 4];
                    int hour = bytes[index + 5];
                    int minute = bytes[index + 6];
                    int seconds = bytes[index + 7];
                    dateTime = new DateTime(year + 2000, month, day, hour, minute, seconds);

                    if (Converters.GetHexValue(bytes[index + 8]).Equals("D0"))
                    {
                        frequency = baseFrequency + ((bytes[index + 9] * 256) + bytes[index + 10]);
                        int signalStrength = bytes[index + 11];
                        int code = bytes[index + 12];
                        int mort = bytes[index + 13];

                        data += (dateTime.Year - 2000) + ", " + dateTime.DayOfYear + ", " + dateTime.Hour + ", " + dateTime.Minute + ", " + dateTime.Second
                                + ", 0, " + frequencyTableIndex + ", " + GetFrequency(frequency) + ", " + signalStrength + ", " + code + ", " + mort
                                + ", 0, 0, 0, 0, " + (dateTime.Month + "/" + dateTime.Day + "/" + (dateTime.Year - 2000)) + ", 0" + CR + LF;
                    }
                    else if (Converters.GetHexValue(bytes[index + 8]).Equals("E0"))
                    {
                        frequency = baseFrequency + ((bytes[index + 9] * 256) + bytes[index + 10]);
                        int signalStrength = bytes[index + 11];

                        data += (dateTime.Year - 2000) + ", " + dateTime.DayOfYear + ", " + dateTime.Hour + ", " + dateTime.Minute + ", " + dateTime.Second
                                + ", 0, " + frequencyTableIndex + ", " + GetFrequency(frequency) + ", " + signalStrength + ", 0, 0, 0, 0, 0, 0, "
                                + (dateTime.Month + "/" + dateTime.Day + "/" + (dateTime.Year - 2000)) + ", 0" + CR + LF;
                    }
                    index += 16;
                }
                else
                {
                    index += 8;
                }
            }
            return data;
        }

        private void PrintSnapshotFiles()
        {
            ProcessingStatus.Source = "SmallChecked";
            Processing.IsVisible = false;
            PreparingStatus.Source = "GreenCircle";
            PreparingFile.TextColor = Color.FromRgb(31, 41, 51);
            Preparing.IsVisible = true;

            int index = 0;
            string message;
            try
            {
                Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "atstrack");
                if (!Directory.Exists(Root))
                {
                    var info = Directory.CreateDirectory(Root);
                    if (!info.Exists)
                        throw new Exception("Folder 'atstrack' can't be created.");
                }
                while (index < SnapshotArray.Count)
                {
                    FileName = SnapshotArray[index].GetFileName();
                    var file = File.Create(Path.Combine(Root, FileName));

                    file.Write(SnapshotArray[index].GetSnapshot(), 0, SnapshotArray[index].GetSnapshot().Length);
                    file.Flush();
                    file.Close();

                    index++;
                }

                if (index == SnapshotArray.Count)
                {
                    PreparingStatus.Source = "SmallChecked";
                    Preparing.IsVisible = false;
                    message = "Download finished: " + (Snapshots.BYTES_PER_PAGE * FinalPageNumber) + " byte(s) downloaded successfully.";
                    if (Error)
                    {
                        message += " No data found in bytes downloaded. No file was generated.";
                        if (Packets.Count != TotalPackagesNumber)
                            message += " Error timeout.";
                        DisplayAlert("Finished", message, "OK");
                    }
                    else
                    {
                        ShowDisplayAlert("Finished", message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " // " + ex.StackTrace);
                PreparingStatus.Source = "SmallChecked";
                Preparing.IsVisible = false;
                message = "Download finished: " + (Snapshots.BYTES_PER_PAGE * FinalPageNumber) + " byte(s) downloaded.";
                if (Error)
                {
                    message += " No data found in bytes downloaded. No file was generated.";
                    if (Packets.Count != TotalPackagesNumber)
                        message += " Error timeout.";
                    DisplayAlert("Finished", message, "OK");
                }
                else
                {
                    ShowDisplayAlert("Finished", message);
                }
            }
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
    }
}
