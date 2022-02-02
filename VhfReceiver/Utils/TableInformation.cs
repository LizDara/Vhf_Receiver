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

        public TableInformation(int table, int frequencies)
        {
            Table = table;
            Frequencies = frequencies;
            TableNumber = "Table " + Table;
            FrequencyNumber = Frequencies + " Frequencies";
        }
    }
}
