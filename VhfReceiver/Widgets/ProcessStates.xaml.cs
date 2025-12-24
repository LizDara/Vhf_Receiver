using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class ProcessStates : ContentView
    {
        private string downloadPercent;
        public string DownloadPercent
        {
            set
            {
                downloadPercent = value;
                OnPropertyChanged(nameof(DownloadPercent));
            }
            get { return downloadPercent; }
        }

        public ProcessStates()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            FirstStatus.Source = "LightCircle";
            FirstMessage.TextColor = Color.FromRgb(123, 135, 148);
            FirstIndicator.IsVisible = false;
            SecondStatus.Source = "LightCircle";
            SecondMessage.TextColor = Color.FromRgb(123, 135, 148);
            SecondIndicator.IsVisible = false;
            ThirdStatus.Source = "LightCircle";
            ThirdMessage.TextColor = Color.FromRgb(123, 135, 148);
            ThirdIndicator.IsVisible = false;
        }

        public void InitFirstState(string message)
        {
            FirstStatus.Source = "GreenCircle";
            FirstMessage.Text = message;
            FirstMessage.TextColor = Color.FromRgb(31, 41, 51);
            FirstIndicator.IsVisible = true;
        }

        public void InitSecondState(string message)
        {
            FirstStatus.Source = "SmallChecked";
            FirstIndicator.IsVisible = false;
            SecondStatus.Source = "GreenCircle";
            SecondMessage.Text = message;
            SecondMessage.TextColor = Color.FromRgb(31, 41, 51);
            SecondIndicator.IsVisible = true;
        }

        public void InitThirdState(string message)
        {
            SecondStatus.Source = "SmallChecked";
            SecondIndicator.IsVisible = false;
            ThirdStatus.Source = "GreenCircle";
            ThirdMessage.Text = message;
            ThirdMessage.TextColor = Color.FromRgb(31, 41, 51);
            ThirdIndicator.IsVisible = true;
        }

        public void FinishProcess()
        {
            ThirdStatus.Source = "SmallChecked";
            ThirdIndicator.IsVisible = false;
        }

        public void SetPercent(int percent)
        {
            DownloadPercent = " - " + percent.ToString() + "%";
        }
    }
}
