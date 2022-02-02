using System;
using System.Text;
namespace VhfReceiver.Utils
{
    public class Converters
    {
        private static char[] hexArray = "0123456789ABCDEF".ToCharArray();

        public static string GetHexValue(byte[] value)
        {
            if (value == null) return "";

            char[] hexChars = new char[value.Length * 3];
            int v;
            for (int j = 0; j < value.Length; j++)
            {
                v = value[j] & 0xFF;
                hexChars[j * 3] = hexArray[v >> 4];
                hexChars[j * 3 + 1] = hexArray[v & 0x0F];
                hexChars[j * 3 + 2] = ' ';
            }

            return new string(hexChars);
        }

        public static string GetHexValue(byte b)
        {
            char[] hexChars = new char[2];
            int v;
            v = b & 0xFF;
            hexChars[0] = hexArray[v >> 4];
            hexChars[1] = hexArray[v & 0x0F];
            return new string(hexChars);
        }

        public static string GetDecimalValue(byte[] value)
        {
            if (value == null) return "";

            string result = "";
            foreach (byte b in value)
            {
                result += (b & 0xFF) + " ";
            }

            return result;
        }

        public static byte[] ConvertToUTF8(string input)
        {
            byte[] returnVal;
            try
            {
                returnVal = Encoding.UTF8.GetBytes(input);
            }
            catch
            {
                returnVal = Encoding.UTF8.GetBytes(input);
            }

            return returnVal;
        }

        public static int HexToDecimal(string input)
        {
            int total = 0;
            int pot;
            int multiple = 0;
            int z;

            char[] hexadecimal = input.ToCharArray();

            for (int x = hexadecimal.Length - 1; x >= 0; x--)
            {
                z = hexadecimal.Length - x - 1;
                pot = 1;
                for (int y = 0; y < z; y++)
                {
                    pot *= 16;
                }

                char letter = hexadecimal[x];

                switch (letter)
                {
                    case '0':
                        multiple = 0;
                        break;
                    case '1':
                        multiple = 1;
                        break;
                    case '2':
                        multiple = 2;
                        break;
                    case '3':
                        multiple = 3;
                        break;
                    case '4':
                        multiple = 4;
                        break;
                    case '5':
                        multiple = 5;
                        break;
                    case '6':
                        multiple = 6;
                        break;
                    case '7':
                        multiple = 7;
                        break;
                    case '8':
                        multiple = 8;
                        break;
                    case '9':
                        multiple = 9;
                        break;
                    case 'A':
                        multiple = 10;
                        break;
                    case 'B':
                        multiple = 11;
                        break;
                    case 'C':
                        multiple = 12;
                        break;
                    case 'D':
                        multiple = 13;
                        break;
                    case 'E':
                        multiple = 14;
                        break;
                    case 'F':
                        multiple = 15;
                        break;
                }
                total += pot * multiple;
            }

            return total;
        }
    }
}
