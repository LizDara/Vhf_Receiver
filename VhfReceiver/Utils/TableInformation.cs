using System;
namespace VhfReceiver.Utils
{
    public class TableInformation
    {
        public int Table
        {
            get;
            set;
        }

        public int Frequencies
        {
            get;
            set;
        }

        public string TableNumber
        {
            get;
            set;
        }

        public string FrequencyNumber
        {
            get;
            set;
        }

        public bool IsChecked
        {
            get;
            set;
        }

        public string TableFrequency
        {
            get;
            set;
        }

        public bool IsEnabled
        {
            get;
            set;
        }

        public double Opacity
        {
            get;
            set;
        }

        public TableInformation(int table, int frequencies)
        {
            Table = table;
            Frequencies = frequencies;
            TableNumber = "Table " + Table;
            FrequencyNumber = Frequencies + " Frequencies";
            IsChecked = false;
            TableFrequency = "Table " + table + " (" + frequencies + " frequencies)";
            IsEnabled = frequencies != 0;
            Opacity = frequencies == 0 ? 0.6 : 1;
        }
    }
}
