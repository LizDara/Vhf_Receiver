using System;
namespace VhfReceiver.Utils
{
    public class ValueInformation
    {
        public string ValueNumber
        {
            get;
            set;
        }

        public int Value
        {
            get;
            set;
        }

        public ValueInformation(int value, string text)
        {
            Value = value;
            ValueNumber = text;
        }
    }
}
