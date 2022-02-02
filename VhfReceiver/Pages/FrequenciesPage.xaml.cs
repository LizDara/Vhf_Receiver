using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class FrequenciesPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;
        private List<FrequencyInformation> OriginalFrequencies;
        private List<FrequencyInformation> ActualFrequencies;
        private int Number;
        private int BaseFrequency;
        private int BaseRange;
        private int Limit;
        private bool Correct;
        private bool IsChanged = false;
        private char LF = (char)0x0A;

        public FrequenciesPage(DeviceInformation device, byte[] bytes, int baseFrequency, int baseRange)
        {
            InitializeComponent();

            ConnectedDevice = device;
            Name.Text = ConnectedDevice.Name;
            Range.Text = ConnectedDevice.Range;
            Battery.Text = ConnectedDevice.Battery;

            TableNumber.Text = "Table " + bytes[7];
            Number = bytes[7];
            BaseFrequency = baseFrequency;
            BaseRange = baseRange;

            Console.WriteLine("BYTES: " + bytes[7] + " " + bytes[10] + " " + bytes[11] + " " + bytes[12] + " " + bytes[13] + " " + bytes[14] + " " + bytes[15]);
            SetData(bytes);
        }

        private void SetData(byte[] bytes)
        {
            List<FrequencyInformation> frequencies = new List<FrequencyInformation>();
            int a = 10;
            int b = 0;
            while (b < bytes[8])
            {
                int frequency = bytes[a] * 256 + bytes[a + 1];
                frequencies.Add(new FrequencyInformation(BaseFrequency * 1000 + frequency));

                b++;
                a += 2;
            }

            FrequenciesList.ItemsSource = frequencies;
            OriginalFrequencies = frequencies;
            ActualFrequencies = frequencies;
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("BACK");
            await Navigation.PopModalAsync();
        }

        private void EditTable_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<string>(this, "Frequencies", async (value) => {
                if (CheckData(value))
                {
                    if (CheckLimit())
                    {
                        SaveData(value);

                        if (IsChanged)
                        {
                            Menu.IsVisible = true;
                            EditTable.IsVisible = false;
                            SaveChanges.IsVisible = true;
                            SaveChanges.IsEnabled = true;
                            SaveChanges.Opacity = 1;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Exceeded Table Limit", "Please enter no more than 100 frequencies..", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Invalid Format or Values", "Please enter valid frequency values, each on a separate line.", "OK");
                }
            });

            var popMessage = new EditTable(ActualFrequencies);
            _ = App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);
        }

        private void UndoChanges_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("UNDO CHANGES");
            FrequenciesList.ItemsSource = null;
            FrequenciesList.ItemsSource = OriginalFrequencies;
        }

        private bool CheckData(string value)
        {
            Limit = 0;
            Correct = true;
            var frequencies = value.Split(LF);
            foreach (string frequency in frequencies)
            {
                Console.WriteLine("F: " + frequency + " " + frequency.Length);
                Limit++;
                if (frequency.Length != 6)
                    Correct = false;
                else if ((int.Parse(frequency) < (BaseFrequency * 1000)) || (int.Parse(frequency) > (BaseFrequency + BaseRange) * 1000))
                    Correct = false;
            }

            return Correct;
        }

        private bool CheckLimit()
        {
            return Limit <= 100;
        }

        private void SaveData(string value)
        {
            var list = value.Split(LF);
            List<FrequencyInformation> frequencies = new List<FrequencyInformation>();

            if (list.Length != OriginalFrequencies.Count)
                IsChanged = true;

            for (int i = 0; i < list.Length; i++)
            {
                if (!IsChanged && int.Parse(list[i]) != OriginalFrequencies[i].FrequencyNumber)
                    IsChanged = true;
                frequencies.Add(new FrequencyInformation(int.Parse(list[i])));
            }

            FrequenciesList.ItemsSource = null;
            FrequenciesList.ItemsSource = frequencies;
            ActualFrequencies = frequencies;
        }

        private async void SaveChanges_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("SAVE CHANGES");
            DateTime currentDate = DateTime.Now;

            byte[] b = new byte[244];
            b[0] = (byte)0x7E;
            b[1] = (byte)currentDate.Year;
            b[2] = (byte)currentDate.Month;
            b[3] = (byte)currentDate.Day;
            b[4] = (byte)currentDate.Hour;
            b[5] = (byte)currentDate.Minute;
            b[6] = (byte)currentDate.Second;
            b[7] = (byte)Number;
            b[8] = (byte)ActualFrequencies.Count;
            b[9] = (byte)BaseFrequency;

            int index = 10;
            int i = 0;
            while (i < ActualFrequencies.Count)
            {
                int frequency = ActualFrequencies[i].FrequencyNumber % (BaseFrequency * 1000);
                b[index] = (byte)(frequency / 256);
                b[index] = (byte)(frequency % 256);
                index += 2;
                i++;
            }

            var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
            if (service != null)
            {
                var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_FREQ_TABLE);
                if (characteristic != null)
                {
                    Console.WriteLine("SEND: " + Converters.GetDecimalValue(b));
                    await characteristic.WriteAsync(b);

                    var bytes = await characteristic.ReadAsync();
                    MessagingCenter.Send(bytes, "Table");
                }
            }

            await Navigation.PopModalAsync();
        }
    }
}
