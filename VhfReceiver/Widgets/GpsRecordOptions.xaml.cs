using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class GpsRecordOptions : ContentView
    {
        private bool IsMobile;
        public bool IsRecord; //This can change during scanning
        public string firstColor;
        public string FirstColor
        {
            set
            {
                firstColor = value;
                OnPropertyChanged(nameof(FirstColor));
            }
            get { return firstColor; }
        }
        public string secondColor;
        public string SecondColor
        {
            set
            {
                secondColor = value;
                OnPropertyChanged(nameof(SecondColor));
            }
            get { return secondColor; }
        }

        public GpsRecordOptions()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public void SetData(bool isMobile)
        {
            IsMobile = isMobile;
            IsRecord = false;
        }

        private void SetManualRecording()
        {
            RecordData.Text = "Saving targets ...";
            RecordData.IsEnabled = false;
            RecordData.Opacity = 0.6;
        }

        public void SetMobileRecording()
        {
            IsRecord = true;
            RecordData.Text = "Stop Recording";
            FirstColor = "#BA2525";
            SecondColor = "#BA2525";
        }

        private void SetRecord()
        {
            RecordData.Text = "Record Data";
            RecordData.IsEnabled = true;
            RecordData.Opacity = 1;
            FirstColor = "#1BA786";
            SecondColor = "#147D64";
        }

        public void RemoveRecord()
        {
            IsRecord = false;
            RecordData.Text = "Record Data";
            FirstColor = "#1BA786";
            SecondColor = "#147D64";
        }

        public void SetGpsOff()
        {
            GPSLocation.Source = "GpsOff";
            GPSState.Text = "GPS: Off";
        }

        public void SetGpsSearching()
        {
            GPSLocation.Source = "GpsSearching";
            GPSState.Text = "GPS: Searching";
        }

        public void SetGpsFailed()
        {
            GPSLocation.Source = "GpsFailed";
            GPSState.Text = "GPS: Failed";
        }

        public void SetGpsValid()
        {
            GPSLocation.Source = "GpsValid";
            GPSState.Text = "GPS: Valid";
        }

        private async void RecordData_Clicked(object sender, EventArgs e)
        {
            if (IsMobile)
            {
                bool result = await TransferBLEData.WriteRecord(!IsRecord, false);
                if (result)
                {
                    IsRecord = !IsRecord;
                    if (IsRecord) SetMobileRecording();
                    else RemoveRecord();
                }
            }
            else
            {
                SetManualRecording();
                bool result = await TransferBLEData.WriteRecord(true, true);
                if (result)
                {
                    SetRecord();
                    MessagingCenter.Send("OK", ValueCodes.AUTO_RECORD);
                }
            }
        }
    }
}
