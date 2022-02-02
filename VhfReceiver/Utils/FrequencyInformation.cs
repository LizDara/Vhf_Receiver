using System;
namespace VhfReceiver.Utils
{
    public class FrequencyInformation
    {
        public string Frequency
        {
            get;
            set;
        }

        public int FrequencyNumber
        {
            get;
            set;
        }

        public FrequencyInformation(int frequency)
        {
            FrequencyNumber = frequency;

            Frequency = frequency.ToString().Substring(0, 3) + "." + frequency.ToString().Substring(3);
        }
    }
}
