using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Forms;
using VhfReceiver.Widgets;
using Rg.Plugins.Popup.Extensions;

namespace VhfReceiver.Pages
{
    public partial class TablesPage : ContentPage
    {
        private readonly ReceiverInformation ReceiverInformation;
        private readonly int BaseFrequency;
        private readonly int BaseRange;
        private byte[] OriginalData;
        private int[][] Tables;
        private bool ReturnTables;

        public TablesPage(byte[] bytes, bool returnTables)
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();

            OriginalData = bytes;
            BaseFrequency = OriginalData[13];
            BaseRange = OriginalData[14];
            Tables = new int[12][];
            ReturnTables = returnTables;

            SetData();
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
                MessagingCenter.Send(OriginalData, "TableScan");
            await Navigation.PopModalAsync(false);
        }

        private async void TablesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            TableInformation selectedItem = e.Item as TableInformation;
            MessagingCenter.Subscribe<byte[]>(this, "Table", (value) =>
            {
                OriginalData = value;
                SetData();
            });
            try
            {
                if (Tables[selectedItem.Table] == null)
                {
                    if (selectedItem.Frequencies > 0)
                    {
                        var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                        if (service != null)
                        {
                            Guid uuid;
                            switch (selectedItem.Table)
                            {
                                case 1:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_1;
                                    break;
                                case 2:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_2;
                                    break;
                                case 3:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_3;
                                    break;
                                case 4:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_4;
                                    break;
                                case 5:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_5;
                                    break;
                                case 6:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_6;
                                    break;
                                case 7:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_7;
                                    break;
                                case 8:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_8;
                                    break;
                                case 9:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_9;
                                    break;
                                case 10:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_10;
                                    break;
                                case 11:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_11;
                                    break;
                                case 12:
                                    uuid = VhfReceiverUuids.UUID_CHARACTERISTIC_TABLE_12;
                                    break;
                            }
                            var characteristic = await service.GetCharacteristicAsync(uuid);
                            if (characteristic != null)
                            {
                                var bytes = await characteristic.ReadAsync();
                                if (bytes != null)
                                    await Navigation.PushModalAsync(new FrequenciesPage(bytes, selectedItem.Table, selectedItem.Frequencies, BaseFrequency, BaseRange), false);
                            }
                        }
                    }
                    else
                    {
                        await Navigation.PushModalAsync(new FrequenciesPage(new byte[] { }, selectedItem.Table, selectedItem.Frequencies, BaseFrequency, BaseRange), false);
                    }
                }
                else
                {
                    await Navigation.PushModalAsync(new FrequenciesPage(Tables[selectedItem.Table - 1], selectedItem.Table, Tables[selectedItem.Table - 1].Length, BaseFrequency, BaseRange), false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Service: " + ex.Message);
            }
        }

        private void LoadFromFile_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<string[]>(this, "Tables", (value) =>
            {
                SetData(value);
            });

            var popMessage = new DocumentPicker();
            _ = App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
        }

        private void SetData(string[] frequencies)
        {
            int tableNumber = 0;
            List<int> frequenciesList = new List<int>();
            string line;

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

            SetData();
        }
    }
}
