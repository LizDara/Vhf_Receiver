using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.IO;

namespace VhfReceiver.Pages
{
    public partial class ManageReceiverDataPage : ContentPage
    {
        private char CR = (char)0x0D;
        private char LF = (char)0x0A;
        private DeviceInformation ConnectedDevice;
        private List<Snapshots> SnapshotArray;
        private Snapshots RawDataCollector;
        private Snapshots ProcessDataCollector;
        private int FinalPageNumber;
        private int PageNumber;
        private int Percent;
        private bool Error;
        private string Root;
        private string FileName;

        public ManageReceiverDataPage(DeviceInformation device, byte[] bytes)
        {
            InitializeComponent();

            ConnectedDevice = device;
            Name.Text = ConnectedDevice.Name;
            Range.Text = ConnectedDevice.Range;
            Battery.Text = ConnectedDevice.Battery;

            SetData(bytes);
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private void SetData(byte[] bytes)
        {
            int numberPage = FindPageNumber(new byte[] { bytes[18], bytes[17], bytes[16], bytes[15] });
            int lastPage = FindPageNumber(new byte[] { bytes[22], bytes[21], bytes[20], bytes[19] });
            MemoryUsed.Text = ((int) (((float) numberPage / (float) lastPage) * 100)) + "%";
            MemoryUsedPercentage.Progress = (double) numberPage / lastPage;
            BytesStored.Text = "Memory Used (" + (numberPage * 2048) + " bytes stored)";
        }

        private int FindPageNumber(byte[] packet)
        {
            int pageNumber = packet[0];
            pageNumber = (packet[1] << 8) | pageNumber;
            pageNumber = (packet[2] << 16) | pageNumber;
            pageNumber = (packet[3] << 24) | pageNumber;

            return pageNumber;
        }

        private async void DeleteData_Clicked(object sender, EventArgs e)
        {
            var response = await DisplayAlert("Warning", "Are you sure you want to delete data?", "Delete", "Cancel");

            if (response)
            {
                var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_DIAGNOSTIC);
                if (service != null)
                {
                    var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_DIAGNOSTIC_INFO);
                    if (characteristic != null)
                    {
                        byte[] bytes = new byte[] { 0x93 };

                        _ = await characteristic.WriteAsync(bytes);
                    }
                }
            }
        }

        private async void ViewData_Clicked(object sender, EventArgs e)
        {

        }

        private async void DownloadData_Clicked(object sender, EventArgs e)
        {
            bool begin = true;
            var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
            if (service != null)
            {
                var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_STUDY_DATA);
                if (characteristic != null)
                {
                    characteristic.ValueUpdated += (o, args) =>
                    {
                        var bytes = args.Characteristic.Value;

                        Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            if (begin)
                            {
                                FinalPageNumber = FindPageNumber(new byte[] { bytes[0], bytes[1], bytes[2], bytes[3] });
                                PageNumber = FindPageNumber(new byte[] { bytes[4], bytes[5], bytes[6], bytes[7] });
                                Error = false;
                                Percent = 0;
                                begin = false;
                                Percentage.Text = Percent + "%";
                                SnapshotArray = new List<Snapshots>();
                                RawDataCollector = new Snapshots(FinalPageNumber * Snapshots.BYTES_PER_PAGE);

                                if (FinalPageNumber == 0)
                                {
                                    Information.IsVisible = true;
                                    Menu.IsVisible = true;
                                    Downloading.IsVisible = false;
                                    await DisplayAlert("Message", "No data to download.", "OK");
                                    await characteristic.StopUpdatesAsync();
                                }
                            }
                            else
                            {
                                DownloadData(bytes);
                                if (Error)
                                {
                                    Information.IsVisible = true;
                                    Menu.IsVisible = true;
                                    Downloading.IsVisible = false;
                                    await DisplayAlert("Error", "Download error.", "OK");
                                    await characteristic.StopUpdatesAsync();
                                }
                            }
                        });
                    };

                    Information.IsVisible = false;
                    Menu.IsVisible = false;
                    Downloading.IsVisible = true;

                    await characteristic.StartUpdatesAsync();
                }
            }
        }

        private void DownloadData(byte[] bytes)
        {
            if (bytes.Length == 4)
            {
                if ((PageNumber + 1) == FindPageNumber(bytes) && (PageNumber + 1) < FinalPageNumber)
                {
                    PageNumber = FindPageNumber(bytes);
                    Percent = (int)((((float)PageNumber / (float)FinalPageNumber)) * 100);
                    Percentage.Text = Percent + "%";
                }
                else
                {
                    Error = true;
                    Console.WriteLine("No correct number page.");
                    /*Information.IsVisible = true;
                    Menu.IsVisible = true;
                    Downloading.IsVisible = false;
                    DisplayAlert("Error", "Download error.", "OK");
                    return;*/
                }
            }
            else if (bytes.Length == 5)
            {
                Error = true;
                Console.WriteLine("Bytes error.");
                /*Information.IsVisible = true;
                Menu.IsVisible = true;
                Downloading.IsVisible = false;
                DisplayAlert("Error", "Download error.", "OK");
                return;*/
            }
            else
            {
                RawDataCollector.ProcessSnapshotRaw(bytes);
                if (RawDataCollector.IsFilled())
                {
                    Percent = 100;
                    Percentage.Text = Percent + "%";

                    //Task.Delay(1500).ContinueWith(t => {
                        SnapshotArray.Add(RawDataCollector);
                        string processData = ReadData(RawDataCollector.GetSnapshot());

                        if (!Error)
                        {
                            byte[] data = Converters.ConvertToUTF8(processData);
                            ProcessDataCollector = new Snapshots(data.Length);
                            ProcessDataCollector.ProcessSnapshot(data);
                            SnapshotArray.Add(ProcessDataCollector);
                        }
                        PrintSnapshotFiles();
                    //});
                }
            }
        }

        private string ReadData(byte[] bytes)
        {
            Error = false;
            string data = "";
            int index = 0;

            int baseFrequency = 150;
            int frequency = 0;
            int frequencyTableIndex = 0;
            int year = 0;
            int month = 0;
            int day = 0;
            int hour = 0;
            int minute = 0;
            int seconds = 0;
            int julianDay = 0;

            string format = Converters.GetHexValue(bytes[0]);
            if (format.Equals("83") || format.Equals("82"))
            {
                data += "Year, JulianDay, Hour, Min, Sec, Offset, Ant, Index, Freq, SS, Code, Mort, NumDet, Lat, Long, GpsAge, Date, PrmNum" + CR + LF;
                year = bytes[6];
                index += 8;

                while (index < bytes.Length)
                {
                    if (Converters.GetHexValue(bytes[index]).Equals("F0"))
                    {
                        frequency = (baseFrequency * 1000) + ((bytes[index + 1] * 256) + bytes[index + 2]);

                        frequencyTableIndex = bytes[index + 3];

                        int date = Converters.HexToDecimal(
                            Converters.GetHexValue(bytes[index + 4])
                            + Converters.GetHexValue(bytes[index + 5])
                            + Converters.GetHexValue(bytes[index + 6]));

                        month = date / 1000000;
                        date = month % 1000000;
                        day = date / 10000;
                        date = date % 10000;
                        hour = date / 100;
                        minute = date % 100;
                        seconds = bytes[index + 7];
                        DateTime dateTime = DateTime.Now;
                        dateTime.AddYears(year + 2000);
                        dateTime.AddMonths(month - 1);
                        dateTime.AddDays(day);
                        julianDay = dateTime.DayOfYear;
                    }
                    else if (Converters.GetHexValue(bytes[index]).Equals("F1"))
                    {
                        int secondsOffset = bytes[index + 1];
                        int antenna = bytes[index + 2] > 128 ? bytes[index + 2] - 128 : bytes[index + 2];
                        int signalStrength = bytes[index + 4] + 200;
                        int code = bytes[index + 3];
                        int mort = bytes[index + 5];
                        int numberDetection = bytes[index + 7];

                        data += year + ", " + julianDay + ", " + hour + ", " + minute + ", " + seconds + ", " + secondsOffset + ", "
                            + antenna + ", " + frequencyTableIndex + ", " +
                            (frequency.ToString().Substring(0, 3) + "." + frequency.ToString().Substring(3)) + ", " + signalStrength
                            + ", " + code + ", " + mort + ", " + numberDetection + ", 0, 0, 0, " + (month + "/" + day + "/" + year)
                            + ", 0" + CR + LF;
                    }
                    else if (Converters.GetHexValue(bytes[index]).Equals("E1") || Converters.GetHexValue(bytes[index]).Equals("E2"))
                    {
                        int secondsOffset = bytes[index + 1];
                        int antenna = bytes[index + 2] % 10;
                        int signalStrength = bytes[index + 4] + 200;

                        data += year + ", " + month + ", " + day + ", " + hour + ", " + minute + ", " + (seconds + secondsOffset)
                            + ", " + antenna + ", 0, 0, " + signalStrength + ", 0, 0, 0, 0, 0, 0, 0" + CR + LF;
                    }

                    index += 8;
                }
            }
            else
            {
                Error = true;
            }

            return data;
        }

        private void PrintSnapshotFiles()
        {
            int index = 0;
            string message;
            try
            {
                Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "atstrack");
                Console.WriteLine("Root: " + Root);
                if (!Directory.Exists(Root))
                {
                    var info = Directory.CreateDirectory(Root);
                    Console.WriteLine("Create: " + info.Exists);
                    if (!info.Exists)
                        throw new Exception("Folder 'atstrack' can't be created.");
                }
                while (index < SnapshotArray.Count)
                {
                    FileName = SnapshotArray[index].GetFileName();
                    var file = File.Create(Path.Combine(Root, FileName));
                    Console.WriteLine("Create file: " + file.Name);

                    file.Write(SnapshotArray[index].GetSnapshot(), 0, SnapshotArray[index].GetSnapshot().Length);
                    Console.WriteLine("Write end");
                    file.Flush();
                    file.Close();

                    index++;
                }

                if (index == SnapshotArray.Count)
                {
                    Information.IsVisible = true;
                    Menu.IsVisible = true;
                    Downloading.IsVisible = false;
                    message = "Download finished: " + (Snapshots.BYTES_PER_PAGE * FinalPageNumber) + " byte(s) downloaded successfully.";
                    if (Error)
                    {
                        message += " No data found in bytes downloaded. No file was generated.";
                        DisplayAlert("Finished", message, "OK");
                    }
                    else
                    {
                        ShowDisplayAlert("Finished", message);
                    }
                }
            }
            catch
            {
                Information.IsVisible = true;
                Menu.IsVisible = true;
                Downloading.IsVisible = false;
                message = "Download finished: " + (Snapshots.BYTES_PER_PAGE * FinalPageNumber) + " byte(s) downloaded.";
                if (Error)
                {
                    message += " No data found in bytes downloaded. No file was generated.";
                    DisplayAlert("Finished", message, "OK");
                }
                else
                {
                    ShowDisplayAlert("Finished", message);
                }
            }
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
