using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Forms;
using VhfReceiver.Widgets;
using Rg.Plugins.Popup.Extensions;
using Plugin.BLE.Abstractions.EventArgs;
using System.IO;

namespace VhfReceiver.Pages
{
    public partial class TablesPage : ContentPage
    {
        private readonly int BaseFrequency;
        private readonly int BaseRange;
        private byte[] OriginalData;
        private int[][] Tables;
        private readonly bool ReturnTables;

        public TablesPage(byte[] bytes, bool returnTables)
        {
            InitializeComponent();

            Toolbar.SetData("Edit Frequency Tables", true, Back_Clicked);
            OriginalData = bytes;
            BaseFrequency = bytes[13];
            BaseRange = bytes[14];
            Tables = new int[12][];
            ReturnTables = returnTables;

            SetData();
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery
        }

        private void SetData()
        {
            TablesList.ItemsSource = null;
            TablesList.ItemsSource = new List<TableInformation>() {
                new TableInformation(1, OriginalData[1]),
                new TableInformation(2, OriginalData[2]),
                new TableInformation(3, OriginalData[3]),
                new TableInformation(4, OriginalData[4]),
                new TableInformation(5, OriginalData[5]),
                new TableInformation(6, OriginalData[6]),
                new TableInformation(7, OriginalData[7]),
                new TableInformation(8, OriginalData[8]),
                new TableInformation(9, OriginalData[9]),
                new TableInformation(10, OriginalData[10]),
                new TableInformation(11, OriginalData[11]),
                new TableInformation(12, OriginalData[12])
            };
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (ReturnTables)
                MessagingCenter.Send(OriginalData, ValueCodes.TABLE_SCAN);
            await Navigation.PopModalAsync(false);
        }

        private async void TablesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            TableInformation selectedItem = e.Item as TableInformation;
            MessagingCenter.Subscribe<byte[]>(this, ValueCodes.TABLE, (value) =>
            {
                OriginalData = value;
                SetData();
                Tables[selectedItem.Table - 1] = null;
            });
            var bytes = await TransferBLEData.ReadDetectionFilter();
            if (bytes != null)
            {
                bool isTemperature = Converters.GetHexValue(bytes[1]).Equals("07") && Converters.GetHexValue(bytes[11]).Equals("06");
                if (Tables[selectedItem.Table - 1] == null)
                {
                    if (selectedItem.Frequencies > 0)
                    {
                        bytes = await TransferBLEData.ReadFrequencies(selectedItem.Table);
                        if (bytes != null)
                            await Navigation.PushModalAsync(new FrequenciesPage(bytes, selectedItem.Table, selectedItem.Frequencies, BaseFrequency, BaseRange, isTemperature), false);
                    }
                    else
                    {
                        await Navigation.PushModalAsync(new FrequenciesPage(new byte[] { }, selectedItem.Table, selectedItem.Frequencies, BaseFrequency, BaseRange, isTemperature), false);
                    }
                }
                else
                {
                    await Navigation.PushModalAsync(new FrequenciesPage(Tables[selectedItem.Table - 1], selectedItem.Table, Tables[selectedItem.Table - 1].Length, BaseFrequency, BaseRange, isTemperature), false);
                }
            }
        }

        private void LoadFromFile_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<FileInformation>(this, ValueCodes.FILE, (value) =>
            {
                var frequenciesList = File.ReadAllLines(value.FilePath);
                SetData(frequenciesList);
            });

            var popMessage = new DocumentPicker("atstrack");
            _ = App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
        }

        private void SetData(string[] frequencies)
        {
            int tableNumber = 0;
            List<int> frequenciesList = new List<int>();
            string line;
            string message = "";

            foreach (string frequency in frequencies)
            {
                line = frequency.Replace(" ", "");
                if (line.ToUpper().Contains("TABLE"))
                {
                    if (tableNumber > 0)
                    {
                        OriginalData[tableNumber] = (byte)frequenciesList.Count;
                        Tables[tableNumber - 1] = new int[frequenciesList.Count];
                        for (int i = 0; i < frequenciesList.Count; i++)
                            Tables[tableNumber - 1][i] = frequenciesList[i];
                        message += "Table " + tableNumber + ", " + frequenciesList.Count + " frequencies loaded" + ValueCodes.CR + ValueCodes.LF;
                    }
                    tableNumber = int.Parse(line.ToUpper().Replace("TABLE", ""));
                    frequenciesList = new List<int>();
                }
                else
                {
                    frequenciesList.Add(int.Parse(line));
                }
            }
            OriginalData[tableNumber] = (byte)frequenciesList.Count;
            Tables[tableNumber - 1] = new int[frequenciesList.Count];
            for (int i = 0; i < frequenciesList.Count; i++)
                Tables[tableNumber - 1][i] = frequenciesList[i];
            message += "Table " + tableNumber + ", " + frequenciesList.Count + " frequencies loaded." + ValueCodes.CR + ValueCodes.LF;

            SetData();
            DisplayAlert("Message", message + "You now must review each of these tables in order for each one to be stored.", "OK");
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
