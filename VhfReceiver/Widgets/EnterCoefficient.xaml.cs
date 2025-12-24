using System;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class EnterCoefficient : ContentView
    {
        private Button SaveCoefficient;
        private string Message;
        public string newCoefficient;

        public EnterCoefficient()
        {
            InitializeComponent();
        }

        public void SetData(string message, Button saveCoefficient)
        {
            NewCoefficient.Text = message;
            NewCoefficient.TextColor = Color.FromHex("#7B8794");
            Line.BackgroundColor = Color.FromHex("#BA2525");
            SaveCoefficient = saveCoefficient;
            Message = message;
        }

        private void Number_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (NewCoefficient.Text == Message)
            {
                Line.BackgroundColor = Color.FromHex("#CBD2D9");
                NewCoefficient.TextColor = Color.FromRgb(123, 135, 148);
                NewCoefficient.Text = button.Text;
                SaveCoefficient.IsEnabled = true;
                SaveCoefficient.Opacity = 1;
            }
            else if (NewCoefficient.Text.Length < 7)
            {
                string text = NewCoefficient.Text;
                NewCoefficient.Text = text + button.Text;
            }
            newCoefficient = NewCoefficient.Text;
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            if (!NewCoefficient.Text.Equals(""))
            {
                string previous = NewCoefficient.Text;
                NewCoefficient.Text = previous.Substring(0, previous.Length - 1);
                if (NewCoefficient.Text.Equals(""))
                {
                    SaveCoefficient.IsEnabled = false;
                    SaveCoefficient.Opacity = 0.6;
                }
            }
            newCoefficient = NewCoefficient.Text;
        }
    }
}
