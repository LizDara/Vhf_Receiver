using System;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class Scanning : ContentView
    {
        public ViewDetectionFilter DetectionFilter;

        public Scanning()
        {
            InitializeComponent();
        }

        private async void ViewDetection_Tapped(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PushPopupAsync(DetectionFilter, true);
        }

        public void UpdateVisibility(byte detectionType)
        {
            bool visibility = Converters.GetHexValue(detectionType).Equals("09");
            Code.IsVisible = visibility;
            Mortality.IsVisible = visibility;
            Period.IsVisible = !visibility;
            PulseRate.IsVisible = !visibility;
            ViewDetectionFilter.IsVisible = !visibility;
        }

        public void LogScanCoded(int signalStrength, int codeNumber, int mortality)
        {
            int position = GetPositionNumber(codeNumber, 0);
            if (position > 0)
            {
                RefreshCodedPosition(position, signalStrength, mortality > 0);
            }
            else if (position < 0)
            {
                CreateDetail();
                AddNewCodedDetailInPosition(-position, codeNumber, signalStrength, 1, mortality > 0);
            }
            else
            {
                CreateDetail();
                AddNewCodedDetail(ScanDetails.Children.Count - 2, codeNumber, signalStrength, 1, mortality > 0);
            }
        }

        public void LogScanNonCodedFixed(int period, int signalStrength, int type)
        {
            int pulseRate = 60000 / period;
            int position = GetPositionNumber(type, 4);
            if (position > 0)
            {
                RefreshNonCodedPosition(position, signalStrength, period, pulseRate);
            }
            else if (position < 0)
            {
                CreateDetail();
                AddNewNonCodedDetailInPosition(-position, pulseRate, signalStrength, 1, period, type);
            }
            else
            {
                CreateDetail();
                AddNewNonCodedFixedDetail(ScanDetails.Children.Count - 2, pulseRate, signalStrength, 1, period, type);
            }
        }

        public void LogScanNonCodedVariable(int period, int signalStrength)
        {
            int pulseRate = 60000 / period;
            RefreshNonCodedVariable(period, pulseRate, signalStrength);
        }

        private int GetPositionNumber(int number, int position)
        {
            for (int i = 2; i < ScanDetails.Children.Count - 1; i += 2)
            {
                Grid grid = (Grid)ScanDetails.Children[i];
                Label label = (Label)grid.Children[position];

                if (int.Parse(label.Text) == number) return i;
                else if (number < int.Parse(label.Text)) return -i;
            }
            return 0;
        }

        private void RefreshCodedPosition(int position, int signalStrength, bool isMort)
        {
            Grid grid = (Grid)ScanDetails.Children[position];
            Label detectionsLabel = (Label)grid.Children[1];
            Label mortalityLabel = (Label)grid.Children[2];
            Label signalStrengthLabel = (Label)grid.Children[3];
            Label mortLabel = (Label)grid.Children[4];

            int detectionsNumber = int.Parse(detectionsLabel.Text) + 1;
            if (detectionsNumber > 1000) detectionsNumber = 1;
            int mortNumber = isMort ? int.Parse(mortLabel.Text) + 1 : int.Parse(mortLabel.Text);
            detectionsLabel.Text = detectionsNumber.ToString();
            mortalityLabel.Text = isMort ? "M" : "-";
            signalStrengthLabel.Text = signalStrength.ToString();
            mortLabel.Text = mortNumber.ToString();
        }

        private void RefreshNonCodedPosition(int position, int signalStrength, int period, int pulseRate)
        {
            Grid grid = (Grid)ScanDetails.Children[position];
            Label periodLabel = (Label)grid.Children[0];
            Label detectionsLabel = (Label)grid.Children[1];
            Label pulseRateLabel = (Label)grid.Children[2];
            Label signalStrengthLabel = (Label)grid.Children[3];

            int detectionsNumber = int.Parse(detectionsLabel.Text) + 1;
            if (detectionsNumber > 1000) detectionsNumber = 1;
            periodLabel.Text = period.ToString();
            detectionsLabel.Text = detectionsNumber.ToString();
            pulseRateLabel.Text = pulseRate.ToString();
            signalStrengthLabel.Text = signalStrength.ToString();
        }

        private void AddNewCodedDetail(int position, int code, int signalStrength, int detections, bool isMort)
        {
            Grid grid = (Grid)ScanDetails.Children[position];
            Label newCodeLabel = (Label)grid.Children[0];
            Label newDetectionsLabel = (Label)grid.Children[1];
            Label newMortalityLabel = (Label)grid.Children[2];
            Label newSignalStrengthLabel = (Label)grid.Children[3];
            Label newMortLabel = (Label)grid.Children[4];

            newCodeLabel.Text = code.ToString();
            newDetectionsLabel.Text = detections.ToString();
            newMortalityLabel.Text = isMort ? "M" : "-";
            newSignalStrengthLabel.Text = signalStrength.ToString();
            newMortLabel.Text = isMort ? "1" : "0";
        }

        private void AddNewNonCodedFixedDetail(int position, int pulseRate, int signalStrength, int detections, int period, int type)
        {
            Grid grid = (Grid)ScanDetails.Children[position];
            Label newPeriodLabel = (Label)grid.Children[0];
            Label newDetectionsLabel = (Label)grid.Children[1];
            Label newPulseRateLabel = (Label)grid.Children[2];
            Label newSignalStrengthLabel = (Label)grid.Children[3];
            Label newTypeLabel = (Label)grid.Children[4];

            newPeriodLabel.Text = period.ToString();
            newDetectionsLabel.Text = detections.ToString();
            newPulseRateLabel.Text = pulseRate.ToString();
            newSignalStrengthLabel.Text = signalStrength.ToString();
            newTypeLabel.Text = type.ToString();
        }

        private void AddNewNonCodedVariableDetail(int position, int pulseRate, int signalStrength, int period)
        {
            Grid grid = (Grid)ScanDetails.Children[position];
            Label newPeriodLabel = (Label)grid.Children[0];
            Label newDetectionsLabel = (Label)grid.Children[1];
            Label newPulseRateLabel = (Label)grid.Children[2];
            Label newSignalStrengthLabel = (Label)grid.Children[3];

            newPeriodLabel.Text = period.ToString();
            newDetectionsLabel.Text = "-";
            newPulseRateLabel.Text = pulseRate.ToString();
            newSignalStrengthLabel.Text = signalStrength.ToString();
        }

        private void AddNewCodedDetailInPosition(int position, int code, int signalStrength, int detections, bool isMort)
        {
            for (int i = ScanDetails.Children.Count - 2; i > position; i -= 2)
            {
                Grid lastGrid = (Grid)ScanDetails.Children[i];
                Grid penultimateGrid = (Grid)ScanDetails.Children[i - 2];

                Label lastCodeLabel = (Label)lastGrid.Children[0];
                Label penultimateCodeLabel = (Label)penultimateGrid.Children[0];
                lastCodeLabel.Text = penultimateCodeLabel.Text;

                Label lastDetectionsLabel = (Label)lastGrid.Children[1];
                Label penultimateDetectionsLabel = (Label)penultimateGrid.Children[1];
                lastDetectionsLabel.Text = penultimateDetectionsLabel.Text;

                Label lastMortalityLabel = (Label)lastGrid.Children[2];
                Label penultimateMortalityLabel = (Label)penultimateGrid.Children[2];
                lastMortalityLabel.Text = penultimateMortalityLabel.Text;

                Label lastSignalStrengthLabel = (Label)lastGrid.Children[3];
                Label penultimateSignalStrengthLabel = (Label)penultimateGrid.Children[3];
                lastSignalStrengthLabel.Text = penultimateSignalStrengthLabel.Text;

                Label lastMortLabel = (Label)lastGrid.Children[4];
                Label penultimateMortLabel = (Label)penultimateGrid.Children[4];
                lastMortLabel.Text = penultimateMortLabel.Text;
            }
            AddNewCodedDetail(position, code, signalStrength, detections, isMort);
        }

        private void AddNewNonCodedDetailInPosition(int position, int pulseRate, int signalStrength, int detections, int period, int type)
        {
            for (int i = ScanDetails.Children.Count - 2; i > position; i -= 2)
            {
                Grid lastGrid = (Grid)ScanDetails.Children[i];
                Grid penultimateGrid = (Grid)ScanDetails.Children[i - 2];

                Label lastPeriodLabel = (Label)lastGrid.Children[0];
                Label penultimatePeriodLabel = (Label)penultimateGrid.Children[0];
                lastPeriodLabel.Text = penultimatePeriodLabel.Text;

                Label lastDetectionsLabel = (Label)lastGrid.Children[1];
                Label penultimateDetectionsLabel = (Label)penultimateGrid.Children[1];
                lastDetectionsLabel.Text = penultimateDetectionsLabel.Text;

                Label lastPulseRateLabel = (Label)lastGrid.Children[2];
                Label penultimatePulseRateLabel = (Label)penultimateGrid.Children[2];
                lastPulseRateLabel.Text = penultimatePulseRateLabel.Text;

                Label lastSignalStrengthLabel = (Label)lastGrid.Children[3];
                Label penultimateSignalStrengthLabel = (Label)penultimateGrid.Children[3];
                lastSignalStrengthLabel.Text = penultimateSignalStrengthLabel.Text;

                Label lastTypeLabel = (Label)lastGrid.Children[4];
                Label penultimateTypeLabel = (Label)penultimateGrid.Children[4];
                lastTypeLabel.Text = penultimateTypeLabel.Text;
            }
            AddNewNonCodedFixedDetail(position, pulseRate, signalStrength, detections, period, type);
        }

        private void RefreshNonCodedVariable(int period, int pulseRate, int signalStrength)
        {
            CreateDetail();
            for (int i = ScanDetails.Children.Count - 2; i > 3; i -= 2)
            {
                Grid lastGrid = (Grid)ScanDetails.Children[i];
                Grid penultimateGrid = (Grid)ScanDetails.Children[i - 2];

                Label lastPeriodLabel = (Label)lastGrid.Children[0];
                Label penultimatePeriodLabel = (Label)penultimateGrid.Children[0];
                lastPeriodLabel.Text = penultimatePeriodLabel.Text;

                Label lastDetectionsLabel = (Label)lastGrid.Children[1];
                Label penultimateDetectionsLabel = (Label)penultimateGrid.Children[1];
                lastDetectionsLabel.Text = penultimateDetectionsLabel.Text;

                Label lastPulseRateLabel = (Label)lastGrid.Children[2];
                Label penultimatePulseRateLabel = (Label)penultimateGrid.Children[2];
                lastPulseRateLabel.Text = penultimatePulseRateLabel.Text;

                Label lastSignalStrengthLabel = (Label)lastGrid.Children[3];
                Label penultimateSignalStrengthLabel = (Label)penultimateGrid.Children[3];
                lastSignalStrengthLabel.Text = penultimateSignalStrengthLabel.Text;
            }
            AddNewNonCodedVariableDetail(2, pulseRate, signalStrength, period);
        }

        private void CreateDetail()
        {
            Grid newGrid = new Grid();
            newGrid.ColumnDefinitions.Add(new ColumnDefinition());
            newGrid.ColumnDefinitions.Add(new ColumnDefinition());
            newGrid.ColumnDefinitions.Add(new ColumnDefinition());
            newGrid.ColumnDefinitions.Add(new ColumnDefinition());
            newGrid.Padding = new Thickness(0, 8, 0, 8);

            Label firstLabel = WidgetCreation.CreateLabel();
            Label detectionsLabel = WidgetCreation.CreateLabel();
            Label secondLabel = WidgetCreation.CreateLabel();
            Label signalStrengthLabel = WidgetCreation.CreateLabel();
            Label extraLabel = new Label();
            extraLabel.IsVisible = false;

            newGrid.Children.Add(firstLabel, 0, 0);
            newGrid.Children.Add(detectionsLabel, 1, 0);
            newGrid.Children.Add(secondLabel, 2, 0);
            newGrid.Children.Add(signalStrengthLabel, 3, 0);
            newGrid.Children.Add(extraLabel, 3, 0);

            StackLayout line = new StackLayout();
            line.BackgroundColor = Color.FromHex("E5E5E5");
            line.HorizontalOptions = LayoutOptions.Fill;
            line.HeightRequest = 1;

            ScanDetails.Children.Add(newGrid);
            ScanDetails.Children.Add(line);
        }

        public void Clear()
        {
            int count = ScanDetails.Children.Count;
            while (count > 2)
            {
                ScanDetails.Children.RemoveAt(2);
                count--;
            }
        }
    }
}
