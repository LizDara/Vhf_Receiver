using System;
namespace VhfReceiver.Utils
{
    public class TagInformation
    {
        public string Code
        {
            get;set;
        }
        public string Detection
        {
            get;set;
        }
        public bool Visible
        {
            get; set;
        }
        public string Rssi
        {
            get;set;
        }
        public string TimeSince
        {
            get;set;
        }
        public string Temperature
        {
            get;set;
        }
        public string Voltage
        {
            get;set;
        }
        public double Height
        {
            get; set;
        }

        public TagInformation(byte[] value)
        {
            SetValues(value);
            SetVisibility(false);
            Detection = "1";
        }

        public void UpdateData(byte[] value)
        {
            SetValues(value);
            Detection = (int.Parse(Detection) + 1).ToString();
        }

        private void SetValues(byte[] value)
        {
            Code = Converters.GetAsciiValue(6, 14, value);
            Rssi = Converters.GetAsciiValue(24, 28, value);
            Temperature = Converters.GetAsciiValue(40, 44, value);
        }

        public void SetVisibility(bool isVisible)
        {
            Visible = isVisible;
            Height = isVisible ? 198 : 78;
        }
    }
}
