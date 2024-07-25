using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Rg.Plugins.Popup.Extensions;

using Xamarin.Forms;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace VhfReceiver.Pages
{
    public partial class FrequenciesPage : ContentPage
    {
        private ReceiverInformation ReceiverInformation;
        private List<FrequencyInformation> OriginalFrequencies;
        private List<FrequencyInformation> ActualFrequencies;
        private int Number;
        private int FrequenciesNumber;
        private int BaseFrequency;
        private int FrequencyRange;

        private FrequencyInformation SelectedFrequency;
        private int SelectedIndex;

        public FrequenciesPage(byte[] bytes, int number, int frequenciesNumber, int baseFrequency, int baseRange)
        {
            Initialize(number, frequenciesNumber, baseFrequency, baseRange);
            SetData(bytes);
        }

        public FrequenciesPage(int[] frequencies, int number, int frequenciesNumber, int baseFrequency, int baseRange)
        {
            Initialize(number, frequenciesNumber, baseFrequency, baseRange);
            SetData(frequencies);
        }

        private void Initialize(int number, int frequenciesNumber, int baseFrequency, int baseRange)
        {
            InitializeComponent();
            ReceiverInformation = ReceiverInformation.GetReceiverInformation();

            if (frequenciesNumber == 0)
                SetVisibility("none");
            else
                SetVisibility("overview");
            Number = number;
            FrequenciesNumber = frequenciesNumber;
            BaseFrequency = baseFrequency * 1000;
            FrequencyRange = ((baseFrequency + baseRange) * 1000) - 1;
            string message = "Frequency range is " + BaseFrequency + " to " + FrequencyRange;
            FrequencyBaseRange.Text = message;
            TitleToolbar.Text = "Table " + Number + " (" + FrequenciesNumber + " Frequencies)";

            CreateNumberButtons(baseRange);
        }

        private void SetData(byte[] bytes)
        {
            OriginalFrequencies = new List<FrequencyInformation>();
            ActualFrequencies = new List<FrequencyInformation>();
            int a = 10;
            int b = 0;
            while (b < FrequenciesNumber)
            {
                int frequency = bytes[a] * 256 + bytes[a + 1];
                OriginalFrequencies.Add(new FrequencyInformation(BaseFrequency + frequency));
                ActualFrequencies.Add(new FrequencyInformation(BaseFrequency + frequency));
                b++;
                a += 2;
            }

            FrequenciesList.ItemsSource = ActualFrequencies;
            FrequenciesDeleteList.ItemsSource = ActualFrequencies;
        }

        private void SetData(int[] frequencies)
        {
            OriginalFrequencies = new List<FrequencyInformation>();
            ActualFrequencies = new List<FrequencyInformation>();
            foreach (int frequency in frequencies)
            {
                OriginalFrequencies.Add(new FrequencyInformation(frequency));
                ActualFrequencies.Add(new FrequencyInformation(frequency));
            }

            FrequenciesList.ItemsSource = ActualFrequencies;
            FrequenciesDeleteList.ItemsSource = ActualFrequencies;
        }

        private void CreateNumberButtons(int baseRange)
        {
            int baseNumber = BaseFrequency / 1000;
            ButtonsNumber.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonsNumber.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonsNumber.ColumnDefinitions.Add(new ColumnDefinition());
            ButtonsNumber.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < baseRange / 4; i++)
            {
                ButtonsNumber.RowDefinitions.Add(new RowDefinition());
                for (int j = 0; j < 4; j++)
                {
                    Button button = WidgetCreation.NewBaseButton(baseNumber);
                    int finalBaseNumber = baseNumber;
                    button.Clicked += (object sender, EventArgs e) =>
                    {
                        if (NewFrequency.Text.Equals("") || NewFrequency.Text.Length > 6)
                        {
                            NewFrequency.Text = finalBaseNumber.ToString();
                            NewFrequency.TextColor = Color.FromRgb(31, 41, 51);
                        }
                    };
                    ButtonsNumber.Children.Add(button, j, i);
                    baseNumber++;
                }
            }
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (FrequenciesOverview.IsVisible || NoFrequencies.IsVisible)
            {
                if (ExistChanges())
                {
                    bool result = await SetFrequencies();
                    if (result)
                        await Navigation.PopModalAsync(false);
                }
                else
                {
                    await Navigation.PopModalAsync(false);
                }
            }
            else if (EditFrequency.IsVisible || FrequenciesDelete.IsVisible)
            {
                SetVisibility("overview");
                if (FrequenciesDelete.IsVisible)
                    ChangeAllCheckBox(false);
            }
        }

        private void AddNewFrequency_Clicked(object sender, EventArgs e)
        {
            if (IsWithinLimit())
            {
                SetVisibility("edit");
                TitleToolbar.Text = "New Frequency";
                NewFrequency.Text = "Enter Frequency Digits";
                NewFrequency.TextColor = Color.FromRgb(123, 135, 148);
                Line.BackgroundColor = Color.FromHex("#CBD2D9");
                FrequencyBaseRange.TextColor = Color.FromRgb(123, 135, 148);
                SaveFrequency.Text = "Add Frequency";
                SaveFrequency.Opacity = 0.6;
                SaveFrequency.IsEnabled = false;
                SelectedFrequency = null;
                SelectedIndex = -1;
            }
            else
            {
                DisplayAlert("Exceeded Table Limit", "Please enter no more than 100 frequencies.", "OK");
            }
        }

        private void DeleteFrequencies_Clicked(object sender, EventArgs e)
        {
            SetVisibility("delete");

            DeleteFrequencies.IsEnabled = false;
            DeleteFrequencies.Opacity = 0.6;
        }

        private void FrequenciesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            SelectedFrequency = (FrequencyInformation)e.Item;
            SelectedIndex = e.ItemIndex;

            SetVisibility("edit");
            TitleToolbar.Text = "Edit Frequency " + SelectedFrequency.Frequency;
            NewFrequency.Text = "Enter Frequency Digits";
            NewFrequency.TextColor = Color.FromRgb(123, 135, 148);
            Line.BackgroundColor = Color.FromHex("#CBD2D9");
            FrequencyBaseRange.TextColor = Color.FromRgb(123, 135, 148);
            SaveFrequency.Text = "Save Changes";
            SaveFrequency.Opacity = 0.6;
            SaveFrequency.IsEnabled = false;
        }

        private void TextListener_Changed(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (NewFrequency.Text.Length == 6)
            {
                int frequency = int.Parse(NewFrequency.Text);
                if (frequency >= BaseFrequency && frequency <= FrequencyRange)
                {
                    SaveFrequency.IsEnabled = true;
                    SaveFrequency.Opacity = 1;
                    Line.BackgroundColor = Color.FromHex("#CBD2D9");
                    FrequencyBaseRange.TextColor = Color.FromRgb(123, 135, 148);
                }
            }
            else
            {
                SaveFrequency.IsEnabled = false;
                SaveFrequency.Opacity = 0.6;
                Line.BackgroundColor = Color.FromHex("#BA2525");
                FrequencyBaseRange.TextColor = Color.FromRgb(186, 37, 37);
            }
        }

        private void Number_Clicked(object sender, EventArgs e)
        {
            if (NewFrequency.Text.Length >= 3 && NewFrequency.Text.Length < 6)
            {
                Button button = (Button)sender;
                string text = NewFrequency.Text;
                NewFrequency.Text = text + button.Text;
            }
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            if (!NewFrequency.Text.Equals(""))
            {
                string previous = NewFrequency.Text;
                NewFrequency.Text = previous.Substring(0, previous.Length - 1);
            }
        }

        private void SaveFrequency_Clicked(object sender, EventArgs e)
        {
            if (SelectedFrequency != null)
            {
                if (!SelectedFrequency.Frequency.Equals(NewFrequency.Text))
                    ChangeSelectedFrequency();
            }
            else
            {
                AddFrequency();
            }
        }

        private void AllFrequencies_Changed(object sender, CheckedChangedEventArgs e)
        {
            ChangeAllCheckBox(e.Value);
        }

        private void FrequenciesDeleteList_Tapped(object sender, ItemTappedEventArgs e)
        {
            ActualFrequencies[e.ItemIndex].IsChecked = !ActualFrequencies[e.ItemIndex].IsChecked;
            FrequenciesDeleteList.ItemsSource = null;
            FrequenciesDeleteList.ItemsSource = ActualFrequencies;

            int index = 0;
            int count = 0;
            while (index < ActualFrequencies.Count)
            {
                if (ActualFrequencies[index].IsChecked)
                    count++;
                index++;
            }
            if (count == ActualFrequencies.Count)
            {
                AllFrequencies.IsChecked = true;
            }
            else if (count == ActualFrequencies.Count - 1)
            {

                AllFrequencies.CheckedChanged -= AllFrequencies_Changed;
                AllFrequencies.IsChecked = false;
                AllFrequencies.CheckedChanged += AllFrequencies_Changed;
            }
            else if (count == 0)
            {
                DeleteFrequencies.IsEnabled = false;
                DeleteFrequencies.Opacity = 0.6;
            }
            else
            {
                DeleteFrequencies.IsEnabled = true;
                DeleteFrequencies.Opacity = 1;
            }
        }

        private void DeleteSelectedFrequencies_Clicked(object sender, EventArgs e)
        {
            DeleteSelectedFrequencies();
        }

        private void SetVisibility(string value)
        {
            switch (value)
            {
                case "overview":
                    FrequenciesOverview.IsVisible = true;
                    NoFrequencies.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    FrequenciesDelete.IsVisible = false;
                    TitleToolbar.Text = "Table " + Number + " (" + FrequenciesNumber + " Frequencies)";
                    break;
                case "none":
                    FrequenciesOverview.IsVisible = false;
                    NoFrequencies.IsVisible = true;
                    EditFrequency.IsVisible = false;
                    FrequenciesDelete.IsVisible = false;
                    TitleToolbar.Text = "Table " + Number + " (" + FrequenciesNumber + " Frequencies)";
                    break;
                case "edit":
                    FrequenciesOverview.IsVisible = false;
                    NoFrequencies.IsVisible = false;
                    EditFrequency.IsVisible = true;
                    FrequenciesDelete.IsVisible = false;
                    break;
                case "delete":
                    FrequenciesOverview.IsVisible = false;
                    NoFrequencies.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    FrequenciesDelete.IsVisible = true;
                    TitleToolbar.Text = "Delete Frequencies";
                    break;
            }
        }

        private bool IsWithinLimit()
        {
            return ActualFrequencies.Count < 100;
        }

        private async void ChangeSelectedFrequency()
        {
            FrequencyInformation frequencyInformation = new FrequencyInformation(int.Parse(NewFrequency.Text));
            ActualFrequencies[SelectedIndex] = frequencyInformation;

            FrequenciesList.ItemsSource = null;
            FrequenciesDeleteList.ItemsSource = null;
            FrequenciesList.ItemsSource = ActualFrequencies;
            FrequenciesDeleteList.ItemsSource = ActualFrequencies;

            var popMessage = new FrequencyMessage("Frequency Saved");
            await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

            SetVisibility("overview");
        }

        private async void AddFrequency()
        {
            FrequencyInformation frequencyInformation = new FrequencyInformation(int.Parse(NewFrequency.Text));
            ActualFrequencies.Add(frequencyInformation);

            FrequenciesList.ItemsSource = null;
            FrequenciesDeleteList.ItemsSource = null;
            FrequenciesList.ItemsSource = ActualFrequencies;
            FrequenciesDeleteList.ItemsSource = ActualFrequencies;
            FrequenciesNumber = ActualFrequencies.Count;

            var popMessage = new FrequencyMessage("Frequency Added");
            await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

            SetVisibility("overview");
        }

        private void ChangeAllCheckBox(bool isChecked)
        {
            for (int i = 0; i < ActualFrequencies.Count; i++)
                ActualFrequencies[i].IsChecked = isChecked;
            FrequenciesDeleteList.ItemsSource = null;
            FrequenciesDeleteList.ItemsSource = ActualFrequencies;

            DeleteFrequencies.IsEnabled = isChecked;
            DeleteFrequencies.Opacity = isChecked ? 1 : 0.6;
        }

        private async void DeleteSelectedFrequencies()
        {
            int index = 0;
            while (index < ActualFrequencies.Count)
            {
                if (ActualFrequencies[index].IsChecked == true)
                    ActualFrequencies.RemoveAt(index);
                else
                    index++;
            }

            FrequenciesList.ItemsSource = null;
            FrequenciesDeleteList.ItemsSource = null;
            FrequenciesList.ItemsSource = ActualFrequencies;
            FrequenciesDeleteList.ItemsSource = ActualFrequencies;
            FrequenciesNumber = ActualFrequencies.Count;

            var popMessage = new FrequencyMessage("Frequencies Deleted");
            await App.Current.MainPage.Navigation.PushPopupAsync(popMessage, true);

            if (FrequenciesNumber == 0)
                SetVisibility("none");
            else
                SetVisibility("overview");
        }

        private bool ExistChanges()
        {
            if (OriginalFrequencies.Count != ActualFrequencies.Count)
                return true;
            for (int i = 0; i < OriginalFrequencies.Count; i++)
            {
                if (!OriginalFrequencies[i].Frequency.Equals(ActualFrequencies[i].Frequency))
                    return true;
            }
            return false;
        }

        private async Task<bool> SetFrequencies()
        {
            try
            {
                var service = await ReceiverInformation.GetDevice().GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
                if (service != null)
                {
                    Guid uuid;
                    switch (Number)
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
                        DateTime currentDate = DateTime.Now;

                        byte[] b = new byte[244];
                        b[0] = 0x7E;
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
                            int frequency = ActualFrequencies[i].FrequencyNumber - BaseFrequency;
                            b[index] = (byte)(frequency / 256);
                            b[index + 1] = (byte)(frequency % 256);
                            index += 2;
                            i++;
                        }

                        bool result = await characteristic.WriteAsync(b);

                        byte[] bytes = await GetTables(service);
                        if (bytes != null)
                        {
                            MessagingCenter.Send(bytes, "Table");
                            return result;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return false;
        }

        private async Task<byte[]> GetTables(IService service)
        {
            try
            {
                var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_FREQ_TABLE);
                if (characteristic != null)
                {
                    byte[] bytes = await characteristic.ReadAsync();
                    return bytes;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Service: " + e.Message);
            }
            return null;
        }
    }
}
