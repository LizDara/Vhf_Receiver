using System;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class EnterFrequency : ContentView
    {
        private int BaseFrequency;
        private int FrequencyRange;
        private Button SaveFrequency;
        public int newFrequency;

        public EnterFrequency()
        {
            InitializeComponent();
        }

        public void SetData(int baseFrequency, int baseRange, Button saveFrequency)
        {
            BaseFrequency = baseFrequency * 1000;
            FrequencyRange = ((baseFrequency + baseRange) * 1000) - 1;
            string message = "Frequency range is " + BaseFrequency + " to " + FrequencyRange;
            FrequencyBaseRange.Text = message;
            SaveFrequency = saveFrequency;
            NewFrequency.PropertyChanged += TextListener_Changed;

            CreateNumberButtons(baseRange);
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

        private void TextListener_Changed(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (NewFrequency.Text.Length == 6)
            {
                newFrequency = int.Parse(NewFrequency.Text);
                if (newFrequency >= BaseFrequency && newFrequency <= FrequencyRange)
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

        public void Initialize()
        {
            NewFrequency.Text = "Enter Frequency Digits";
            NewFrequency.TextColor = Color.FromRgb(123, 135, 148);
            Line.BackgroundColor = Color.FromHex("#CBD2D9");
            FrequencyBaseRange.TextColor = Color.FromRgb(123, 135, 148);
            SaveFrequency.Text = "Add Frequency";
            SaveFrequency.Opacity = 0.6;
            SaveFrequency.IsEnabled = false;
        }
    }
}
