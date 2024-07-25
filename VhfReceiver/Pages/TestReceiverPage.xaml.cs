using System;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class TestReceiverPage : ContentPage
    {
        public TestReceiverPage(byte[] bytes)
        {
            InitializeComponent();

            SetData(bytes);
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(false);
        }

        private void SetData(byte[] bytes)
        {
            int baseFrequency = bytes[23] * 1000;
            int frequencyRange = ((bytes[23] + bytes[24]) * 1000) - 1;

            string range = baseFrequency.ToString().Substring(0, 3) + "." + baseFrequency.ToString().Substring(3)
                + "-" + frequencyRange.ToString().Substring(0, 3) + "." + frequencyRange.ToString().Substring(3);

            FrequencyRange.Text = range.ToString();

            BatteryPercentage.Text = bytes[1].ToString();

            int numberPage = FindPageNumber(new byte[] { bytes[18], bytes[17], bytes[16], bytes[15] });
            int lastPage = FindPageNumber(new byte[] { bytes[22], bytes[21], bytes[20], bytes[19] });

            BytesStored.Text = (numberPage * 2048).ToString();

            MemoryUsed.Text = (numberPage * 100 / lastPage).ToString();

            FrequencyTables.Text = bytes[2].ToString();

            if (bytes[2] > 0)
            {
                if (bytes[3] > 0)
                {
                    Table1.Text = bytes[3].ToString();
                    TableOneInformation.IsVisible = true;
                }
                if (bytes[4] > 0)
                {
                    Table1.Text = bytes[4].ToString();
                    TableTwoInformation.IsVisible = true;
                }
                if (bytes[5] > 0)
                {
                    Table1.Text = bytes[5].ToString();
                    TableThreeInformation.IsVisible = true;
                }
                if (bytes[6] > 0)
                {
                    Table1.Text = bytes[6].ToString();
                    TableFourInformation.IsVisible = true;
                }
                if (bytes[7] > 0)
                {
                    Table1.Text = bytes[7].ToString();
                    TableFiveInformation.IsVisible = true;
                }
                if (bytes[8] > 0)
                {
                    Table1.Text = bytes[8].ToString();
                    TableSixInformation.IsVisible = true;
                }
                if (bytes[9] > 0)
                {
                    Table1.Text = bytes[9].ToString();
                    TableSevenInformation.IsVisible = true;
                }
                if (bytes[10] > 0)
                {
                    Table1.Text = bytes[10].ToString();
                    TableEightInformation.IsVisible = true;
                }
                if (bytes[11] > 0)
                {
                    Table1.Text = bytes[11].ToString();
                    TableNineInformation.IsVisible = true;
                }
                if (bytes[12] > 0)
                {
                    Table1.Text = bytes[12].ToString();
                    TableTenInformation.IsVisible = true;
                }
                if (bytes[13] > 0)
                {
                    Table1.Text = bytes[13].ToString();
                    TableElevenInformation.IsVisible = true;
                }
                if (bytes[14] > 0)
                {
                    Table1.Text = bytes[14].ToString();
                    TableTwelveInformation.IsVisible = true;
                }
            }
        }

        private int FindPageNumber(byte[] packet)
        {
            int pageNumber = packet[0];
            pageNumber = (packet[1] << 8) | pageNumber;
            pageNumber = (packet[2] << 16) | pageNumber;
            pageNumber = (packet[3] << 24) | pageNumber;

            return pageNumber;
        }

        private void UpdateReceiver_Clicked(object sender, EventArgs e)
        {
        }
    }
}
