using System;
using System.Collections.Generic;
namespace VhfReceiver.Utils
{
    public class ItemList
    {
        public static List<ValueInformation> ScanTimeMobile()
        {
            List<ValueInformation> values = new List<ValueInformation>();
            values.Add(new ValueInformation(15, "1.5"));

            for (int i = 2; i <= 25; i++)
            {
                int number = i * 10;
                string text = i.ToString() + ".0";
                values.Add(new ValueInformation(number, text));

                number = (i * 10) + 5;
                text = i.ToString() + ".5";
                values.Add(new ValueInformation(number, text));
            }

            return values;
        }

        public static List<ValueInformation> ScanTimeStationary()
        {
            List<ValueInformation> values = new List<ValueInformation>();

            for (int i = 3; i <= 255; i++)
            {
                values.Add(new ValueInformation(i, i.ToString()));
            }

            return values;
        }

        public static List<ValueInformation> Timeout()
        {
            List<ValueInformation> values = new List<ValueInformation>();

            for (int i = 2; i <= 200; i++)
            {
                values.Add(new ValueInformation(i, i.ToString()));
            }

            return values;
        }

        public static List<ValueInformation> ReferenceFrequencyStoreRate()
        {
            List<ValueInformation> values = new List<ValueInformation>();

            for (int i = 0; i <= 24; i++)
            {
                values.Add(new ValueInformation(i, i.ToString()));
            }

            return values;
        }

        public static List<ValueInformation> Antenna()
        {
            List<ValueInformation> values = new List<ValueInformation>();

            for (int i = 1; i <= 4; i++)
            {
                values.Add(new ValueInformation(i, i.ToString()));
            }

            return values;
        }
    }
}
