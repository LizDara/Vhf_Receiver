using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using VhfReceiver.Widgets;
using Rg.Plugins.Popup.Extensions;

using Xamarin.Forms;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.EventArgs;

namespace VhfReceiver.Pages
{
    public partial class FrequenciesPage : ContentPage
    {
        private List<FrequencyInformation> OriginalFrequencies;
        private List<FrequencyInformation> ActualFrequencies;
        private int Number;
        private int FrequenciesNumber;
        private int BaseFrequency;
        private bool IsTemperature;
        private bool SaveCoefficients;
        private Dictionary<string, string> OriginalCoefficients;

        private FrequencyInformation SelectedFrequency;
        private int SelectedIndex;
        private bool IsFile;

        public FrequenciesPage(byte[] bytes, int number, int frequenciesNumber, int baseFrequency, int baseRange, bool isTemperature)
        {
            Initialize(number, frequenciesNumber, baseFrequency, baseRange, isTemperature);
            SetData(bytes);
        }

        public FrequenciesPage(int[] frequencies, int number, int frequenciesNumber, int baseFrequency, int baseRange, bool isTemperature)
        {
            Initialize(number, frequenciesNumber, baseFrequency, baseRange, isTemperature);
            SetData(frequencies);
        }

        private void Initialize(int number, int frequenciesNumber, int baseFrequency, int baseRange, bool isTemperature)
        {
            InitializeComponent();

            Number = number;
            FrequenciesNumber = frequenciesNumber;
            BaseFrequency = baseFrequency * 1000;
            string title = "Table " + Number + " (" + FrequenciesNumber + " Frequencies)";
            IsTemperature = isTemperature;
            SaveCoefficients = !IsTemperature;

            Toolbar.SetData(title, true, Back_Clicked);
            EnterFrequency.SetData(baseFrequency, baseRange, SaveFrequency);
            _ = TransferBLEData.NotificationLog(ValueUpdateState); // Log sd card state and battery

            if (frequenciesNumber == 0)
                SetVisibility("none");
            else
                SetVisibility("overview");
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
            IsFile = false;
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
            IsFile = true;
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            if (FrequenciesOverview.IsVisible || NoFrequencies.IsVisible)
            {
                if ((IsFile || ExistChanges()) && SaveCoefficients)
                {
                    bool result = await SetFrequencies();
                    if (result)
                    {
                        byte[] bytes = await TransferBLEData.ReadTables();
                        while (bytes == null)
                            bytes = await TransferBLEData.ReadTables();
                        MessagingCenter.Send(bytes, ValueCodes.TABLE);
                        await Navigation.PopModalAsync(false);
                    }
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
                SelectedFrequency = null;
                SelectedIndex = -1;
                Toolbar.SetData("Add Frequency");
                if (IsTemperature)
                {
                    SetVisibility("temperature");
                    Frequency.Text = "";
                    CoefficientA.Text = "0";
                    CoefficientB.Text = "0";
                    Constant.Text = "0";
                    SaveChanges.IsEnabled = false;
                    SaveChanges.Opacity = 0.6;
                }
                else
                {
                    SetVisibility("edit");
                    EnterFrequency.Initialize();
                }
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

        private async void FrequenciesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            SelectedFrequency = (FrequencyInformation)e.Item;
            SelectedIndex = e.ItemIndex;

            if (!IsTemperature)
            {
                SetVisibility("edit");
                Toolbar.SetData("Edit Frequency " + SelectedFrequency.Frequency);
                EnterFrequency.Initialize();
            }
            else
            {
                bool result = await TransferBLEData.NotificationLog(ValueUpdateCoefficients);
                if (result)
                {
                    byte[] b = new byte[] { 0x7D, (byte)(SelectedIndex + 1), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    result = await TransferBLEData.WriteFrequencies(Number, b);
                }
            }
        }

        private void SaveFrequency_Clicked(object sender, EventArgs e)
        {
            if (!IsTemperature)
            {
                if (SelectedFrequency != null)
                {
                    if (!SelectedFrequency.Frequency.Equals(EnterFrequency.newFrequency.ToString()))
                        ChangeSelectedFrequency();
                }
                else
                {
                    AddFrequency();
                }
            }
            else
            {
                Frequency.Text = EnterFrequency.newFrequency.ToString();
                SaveChanges.IsEnabled = IsDataCorrect();
                SaveChanges.Opacity = SaveChanges.IsEnabled ? 1 : 0.6;
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

        private void Frequency_Tapped(object sender, EventArgs e)
        {
            string title = (SelectedFrequency == null) ? "Add Frequency" :
                "Edit Frequency " + OriginalCoefficients[ValueCodes.FREQUENCY];
            Toolbar.SetData(title);
            SetVisibility("edit");
            EnterFrequency.Initialize();
        }

        private void CoefficientA_Tapped(object sender, EventArgs e)
        {
            SetVisibility("coefficient");
            Toolbar.SetData("Coef-a");
            EnterCoefficient.SetData("Enter Coef-a Digits", SaveCoefficient);
        }

        private void CoefficientB_Tapped(object sender, EventArgs e)
        {
            SetVisibility("coefficient");
            Toolbar.SetData("Coef-b", true);
            EnterCoefficient.SetData("Enter Coef-b Digits", SaveCoefficient);
        }

        private void Constant_Tapped(object sender, EventArgs e)
        {
            SetVisibility("coefficient");
            Toolbar.SetData("Constant");
            EnterCoefficient.SetData("Enter Constant Digits", SaveCoefficient);
        }

        private void SaveCoefficient_Clicked(object sender, EventArgs e)
        {
            if (Toolbar.GetTitle().Contains("-a"))
                CoefficientA.Text = EnterCoefficient.newCoefficient;
            else if (Toolbar.GetTitle().Contains("-b"))
                CoefficientB.Text = EnterCoefficient.newCoefficient;
            else if (Toolbar.GetTitle().Equals("Constant"))
                Constant.Text = EnterCoefficient.newCoefficient;
            SetVisibility("temperature");
            SaveChanges.IsEnabled = IsDataCorrect();
            SaveChanges.Opacity = SaveChanges.IsEnabled ? 1 : 0.6;
        }

        private async void SaveChanges_Clicked(object sender, EventArgs e)
        {
            if (SelectedFrequency == null || ExistChangesInCoefficients())
            {
                bool result = await SendCoefficients();
                if (result)
                {
                    SaveCoefficients = true;
                    if (SelectedFrequency != null)
                        SetVisibility("overview");
                    else
                        AddFrequency();
                }
            }
        }

        private void SetVisibility(string value)
        {
            string title;
            switch (value)
            {
                case "overview":
                    FrequenciesOverview.IsVisible = true;
                    NoFrequencies.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    FrequenciesDelete.IsVisible = false;
                    EditFrequencyTemperature.IsVisible = false;
                    EditCoefficient.IsVisible = false;
                    title = "Table " + Number + " (" + FrequenciesNumber + " Frequencies)";
                    Toolbar.SetData(title);
                    break;
                case "none":
                    FrequenciesOverview.IsVisible = false;
                    NoFrequencies.IsVisible = true;
                    EditFrequency.IsVisible = false;
                    FrequenciesDelete.IsVisible = false;
                    EditFrequencyTemperature.IsVisible = false;
                    EditCoefficient.IsVisible = false;
                    title = "Table " + Number + " (" + FrequenciesNumber + " Frequencies)";
                    Toolbar.SetData(title);
                    break;
                case "edit":
                    FrequenciesOverview.IsVisible = false;
                    NoFrequencies.IsVisible = false;
                    EditFrequency.IsVisible = true;
                    FrequenciesDelete.IsVisible = false;
                    EditFrequencyTemperature.IsVisible = false;
                    EditCoefficient.IsVisible = false;
                    break;
                case "delete":
                    FrequenciesOverview.IsVisible = false;
                    NoFrequencies.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    FrequenciesDelete.IsVisible = true;
                    EditFrequencyTemperature.IsVisible = false;
                    EditCoefficient.IsVisible = false;
                    title = "Delete Frequencies";
                    Toolbar.SetData(title);
                    break;
                case "temperature":
                    FrequenciesOverview.IsVisible = false;
                    NoFrequencies.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    FrequenciesDelete.IsVisible = false;
                    EditFrequencyTemperature.IsVisible = true;
                    EditCoefficient.IsVisible = false;
                    title = "Edit Frequency";
                    Toolbar.SetData(title);
                    break;
                case "coefficient":
                    FrequenciesOverview.IsVisible = false;
                    NoFrequencies.IsVisible = false;
                    EditFrequency.IsVisible = false;
                    FrequenciesDelete.IsVisible = false;
                    EditFrequencyTemperature.IsVisible = false;
                    EditCoefficient.IsVisible = true;
                    break;
            }
        }

        private void ValueUpdateCoefficients(object o, CharacteristicUpdatedEventArgs args)
        {
            Console.WriteLine(Converters.GetHexValue(args.Characteristic.Value));
            byte[] value = args.Characteristic.Value;
            Frequency.Text = SelectedFrequency.Frequency;
            if (!Converters.AreCoefficientsEmpty(value))
            {
                int coefficientA = (value[5] * 256) + value[6];
                int coefficientB = (value[8] * 256) + value[9];
                int constant = (value[11] * 256) + value[12];
                CoefficientA.Text = Converters.GetHexValue(value[4]).Equals("80") ? "-" + coefficientA : coefficientA.ToString();
                CoefficientB.Text = Converters.GetHexValue(value[7]).Equals("80") ? "-" + coefficientB : coefficientB.ToString();
                Constant.Text = Converters.GetHexValue(value[10]).Equals("80") ? "-" + constant : constant.ToString();
                SaveChanges.IsEnabled = true;
                SaveChanges.Opacity = 1;
            }
            else
            {
                CoefficientA.Text = "0";
                CoefficientB.Text = "0";
                Constant.Text = "0";
                SaveChanges.IsEnabled = false;
                SaveChanges.Opacity = 0.6;
            }
            SetVisibility("temperature");
            OriginalCoefficients = new Dictionary<string, string>
            {
                { ValueCodes.FREQUENCY, Frequency.Text },
                { ValueCodes.COEFFICIENT_A, CoefficientA.Text },
                { ValueCodes.COEFFICIENT_B, CoefficientB.Text },
                { ValueCodes.COEFFICIENT_C, Constant.Text }
            };
        }

        private async Task<bool> SendCoefficients()
        {
            int frequency = int.Parse(Frequency.Text);
            int coefficientA = int.Parse(CoefficientA.Text.Replace("-", ""));
            int coefficientB = int.Parse(CoefficientB.Text.Replace("-", ""));
            int constant = int.Parse(Constant.Text.Replace("-", ""));
            //Coefficient D = 0
            byte formatA = CoefficientA.Text.Contains("-") ? (byte)0x80 : (byte)0;
            byte formatB = CoefficientB.Text.Contains("-") ? (byte)0x80 : (byte)0;
            byte formatC = Constant.Text.Contains("-") ? (byte)0x80 : (byte)0;

            byte[] b = new byte[] { 0x7D, (byte)(SelectedIndex == -1 ? (ActualFrequencies.Count + 1) : (SelectedIndex + 1)),
                (byte)((frequency - BaseFrequency) / 256), (byte)((frequency - BaseFrequency) % 256), formatA,
                (byte) (coefficientA / 256), (byte) (coefficientA % 256), formatB, (byte) (coefficientB / 256),
                (byte) (coefficientB % 256), formatC, (byte) (constant / 256), (byte) (constant % 256), 0, 0, 0 };
            bool result = await TransferBLEData.WriteFrequencies(Number, b);
            return result;
        }

        private bool IsWithinLimit()
        {
            return ActualFrequencies.Count < 100;
        }

        private async void ChangeSelectedFrequency()
        {
            FrequencyInformation frequencyInformation = new FrequencyInformation(EnterFrequency.newFrequency);
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
            FrequencyInformation frequencyInformation = new FrequencyInformation(EnterFrequency.newFrequency);
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

        private bool ExistChangesInCoefficients()
        {
            return !OriginalCoefficients[ValueCodes.FREQUENCY].Equals(Frequency.Text) ||
                !OriginalCoefficients[ValueCodes.COEFFICIENT_A].Equals(CoefficientA.Text) ||
                !OriginalCoefficients[ValueCodes.COEFFICIENT_B].Equals(CoefficientB.Text) ||
                !OriginalCoefficients[ValueCodes.COEFFICIENT_C].Equals(Constant.Text);
        }

        private bool IsDataCorrect()
        {
            return !Frequency.Text.Equals("") && !CoefficientA.Text.Equals("0") && !CoefficientB.Text.Equals("0")
                && !Constant.Text.Equals("0");
        }

        private async Task<bool> SetFrequencies()
        {
            DateTime currentDate = DateTime.Now;

            byte[] b = new byte[IsTemperature ? 10 : 244];
            b[1] = (byte)currentDate.Year;
            b[2] = (byte)currentDate.Month;
            b[3] = (byte)currentDate.Day;
            b[4] = (byte)currentDate.Hour;
            b[5] = (byte)currentDate.Minute;
            b[6] = (byte)currentDate.Second;
            b[7] = (byte)Number;
            b[8] = (byte)ActualFrequencies.Count;
            b[9] = (byte)(BaseFrequency / 1000);
            if (!IsTemperature)
            {
                b[0] = 0x7E;
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
            }
            else
            {
                b[0] = 0x7F;
            }

            bool result = await TransferBLEData.WriteFrequencies(Number, b);
            return result;
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
