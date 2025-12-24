using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Essentials;

namespace VhfReceiver.Utils
{
    public class Converters
    {
        private static readonly char[] hexArray = "0123456789ABCDEF".ToCharArray();

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

        public static string GetDecimalValue(byte b)
        {
            string result = "";
            result += b & 0xff;

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

        public static string GetAsciiValue(int start, int end, byte[] data)
        {
            string value = "";
            for (int i = start; i < end; i++)
                value += (char)data[i];
            return value;
        }

        public static string GetFrequency(int frequency)
        {
            return frequency.ToString().Substring(0, 3) + "." + frequency.ToString().Substring(3);
        }

        public static int GetFrequencyNumber(string frequency)
        {
            return int.Parse(frequency.Replace(".", ""));
        }

        public static string GetDetectionFilter(string type)
        {
            if (type.Equals("C"))
                return " Coded,";
            else if (type.Equals("F"))
                return " Fixed PR,";
            else if (type.Equals("Vx"))
                return " Variable PR,";
            return "None";
        }

        public static string GetStatusVhfReceiver(byte[] scanRecord)
        {
            string status = GetHexValue(scanRecord[3]);
            switch (status)
            {
                case "00":
                    status = " Not Scanning";
                    break;
                case "80":
                case "81":
                case "82":
                    status = " Scanning, mobile";
                    break;
                case "83":
                    status = " Scanning, stationary";
                    break;
                case "86":
                    status = " Scanning, manual";
                    break;
                default:
                    status = "None";
                    break;
            }
            return status;
        }

        public static int GetPercentBatteryVhfReceiver(byte[] scanRecord)
        {
            return scanRecord[2];
        }

        public static bool IsDefaultEmpty(byte[] data)
        {
            bool isEmpty = true;
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] != 0xFF)
                {
                    isEmpty = false;
                    break;
                }
            }
            return isEmpty;
        }

        public static bool AreCoefficientsEmpty(byte[] data)
        {
            bool isEmpty = true;
            for (int i = 4; i < data.Length; i++)
            {
                if (data[i] != 0xFF)
                {
                    isEmpty = false;
                    break;
                }
            }
            return isEmpty;
        }

        public static string[] GetGpsData(byte[] bytes)
        {
            string[] coordinates = new string[2];
            float A, B1, B2, C, D;
            byte sign;
            int degrees, minutes;
            float latitude, longitude;

            //Latitude, byte 4 to 7
            A = (float)(bytes[4] & 0x7F);
            sign = (byte)(bytes[4] & 0x80);
            B1 = (float)(bytes[5] & 0x80);
            B2 = (float)(bytes[5] & 0x7F);
            C = (float)bytes[6];
            D = (float)bytes[7];
            if (GetHexValue(bytes[4]).Equals("FF") && GetHexValue(bytes[5]).Equals("FF")
                && GetHexValue(bytes[6]).Equals("FF") && GetHexValue(bytes[7]).Equals("FF"))
                latitude = 0;
            else
                latitude = (float)((1 + ((B2 + ((C + (D / 256)) / 256)) / 128)) * Math.Pow(2, (A * 2) + (B1 / 128) - 127));
            latitude = latitude / 1000000;
            degrees = (int)Math.Floor(Math.Abs(latitude));
            latitude = (latitude - degrees) * 100 / 60;
            latitude = latitude + degrees;
            if ((latitude * 1000000) == 0)
                coordinates[0] = "0";
            else
                coordinates[0] = sign == 0x80 ? "-" : "+";
            minutes = (int)((latitude - degrees) * 1000000);
            if (degrees < 10)
                coordinates[0] += "0";
            coordinates[0] += degrees.ToString() + ".";
            if (minutes < 100000)
                coordinates[0] += "0";
            if (minutes < 10000)
                coordinates[0] += "0";
            if (minutes < 1000)
                coordinates[0] += "0";
            if (minutes < 100)
                coordinates[0] += "0";
            if (minutes < 10)
                coordinates[0] += "0";
            coordinates[0] += minutes.ToString();

            //Longitude, byte 12 to 15
            A = (float)(bytes[12] & 0x7F);
            sign = (byte)(bytes[12] & 0x80);
            B1 = (float)(bytes[13] & 0x80);
            B2 = (float)(bytes[13] & 0x7F);
            C = (float)bytes[14];
            D = (float)bytes[15];
            if (GetHexValue(bytes[12]).Equals("FF") && GetHexValue(bytes[13]).Equals("FF")
                && GetHexValue(bytes[14]).Equals("FF") && GetHexValue(bytes[15]).Equals("FF"))
                longitude = 0;
            else
                longitude = (float)((1 + ((B2 + ((C + (D / 256)) / 256)) / 128)) * Math.Pow(2, (A * 2) + (B1 / 128) - 127));
            longitude = longitude / 1000000;
            degrees = (int)Math.Floor(Math.Abs(longitude));
            longitude = (longitude - degrees) * 100 / 60;
            longitude = longitude + degrees;
            if ((longitude * 1000000) == 0)
                coordinates[1] = "0";
            else
                coordinates[1] = sign == 0x80 ? "-" : "+";
            minutes = (int)((longitude - degrees) * 1000000);
            if (degrees < 100)
                coordinates[1] += "0";
            if (degrees < 10)
                coordinates[1] += "0";
            coordinates[1] += degrees.ToString() + ".";
            if (minutes < 100000)
                coordinates[1] += "0";
            if (minutes < 10000)
                coordinates[1] += "0";
            if (minutes < 1000)
                coordinates[1] += "0";
            if (minutes < 100)
                coordinates[1] += "0";
            if (minutes < 10)
                coordinates[1] += "0";
            coordinates[1] += minutes.ToString();

            return coordinates;
        }

        public static string ProcessData(byte[] bytes)
        {
            int baseFrequency = Preferences.Get("BaseFrequency", 0) * 1000;
            string data = "";
            int index = 0;
            int frequency = 0;
            int frequencyTableIndex = 0;
            int YY, MM, DD, hh, mm, ss;
            int antenna = 0;
            int sessionNumber = 1;
            DateTime dateTime = DateTime.Now;
            bool foundStop;

            while (index < bytes.Length)
            {
                string format = GetHexValue(bytes[index]);
                string[] coordinates = new string[] { "0", "0" };
                string gpsTimestamp = "0";
                byte detectionType;
                foundStop = false;
                if (format.Equals("83") || format.Equals("82")) //Mobile and Stationary Scan
                {
                    int matches;
                    YY = bytes[index + 6];
                    data += "[Header]" + ValueCodes.CR + ValueCodes.LF;
                    if (format.Equals("83")) //Stationary
                    {
                        data += "Scan Type: Stationary" + ValueCodes.CR + ValueCodes.LF;
                        data += "Scan Interval (seconds): " + bytes[index + 3] + ValueCodes.CR + ValueCodes.LF;
                        data += "Scan Timeout (seconds): " + bytes[index + 4] + ValueCodes.CR + ValueCodes.LF;
                        data += "Num of Antennas: " + (bytes[index + 1] + 1) + ValueCodes.CR + ValueCodes.LF;
                        data += "Store Interval (minutes): " + (bytes[index + 5] == 0 ? "Continuous" : bytes[index + 5].ToString()) + ValueCodes.CR + ValueCodes.LF;
                        int referenceFrequency = (bytes[index + 9] * 256) + bytes[index + 10] + baseFrequency;
                        data += "Reference Frequency: " + (referenceFrequency == baseFrequency ? "No" : GetFrequency(referenceFrequency)) + ValueCodes.CR + ValueCodes.LF;
                        data += "Reference Frequency Store Interval (minutes): " + (referenceFrequency == baseFrequency ? "No" : bytes[index + 11].ToString()) + ValueCodes.CR + ValueCodes.LF;
                        detectionType = (byte) (bytes[index + 2] & 0x0F);
                        matches = bytes[index + 2] / 16;
                        string detection = "Coded";
                        string details = "";
                        if (detectionType == 0x08)
                        {
                            detection = "Non Coded Fixed Pulse Rate";
                            details = matches + " matches required";
                        }
                        else if (detectionType == 0x07)
                        {
                            detection = "Non Coded Variable Pulse Rate";
                            details = matches + " matches required, " + bytes[index + 7] + " to " + bytes[index + 10] + " pulse rate range";
                        }
                        else if (detectionType == 0x06)
                            detection = "Non Coded Variable Pulse Rate";
                        data += "Transmitter Detection Type: " + detection + ValueCodes.CR + ValueCodes.LF;
                        data += "Transmitter Detection Details: " + details + ValueCodes.CR + ValueCodes.LF;
                        index += 24;
                    } else { //Mobile
                        data += "Scan type: Mobile" + ValueCodes.CR + ValueCodes.LF;
                        data += "Scan Interval (seconds): " + (bytes[index + 3] * 0.1) + ValueCodes.CR + ValueCodes.LF;
                        detectionType = (byte)(bytes[index + 4] & 0x0F);
                        matches = bytes[index + 4] / 16;
                        string detection = "Coded";
                        string details = "";
                        if (detectionType == 0x08)
                        {
                            detection = "Non Coded Fixed Pulse Rate";
                            details = matches + " matches required";
                        }
                        else if (detectionType == 0x07)
                        {
                            detection = "Non Coded Variable Pulse Rate";
                            details = matches + " matches required, " + bytes[index + 7] + " to " + bytes[index + 10] + " pulse rate range";
                        }
                        else if (detectionType == 0x06)
                            detection = "Non Coded Variable Pulse Rate";
                        data += "Transmitter Detection Type: " + detection + ValueCodes.CR + ValueCodes.LF;
                        data += "Transmitter Detection Details: " + details + ValueCodes.CR + ValueCodes.LF;
                        int gps = bytes[index + 2] >> 7 & 1;
                        data += "Gps: " + (gps == 1 ? "On" : "Off") + ValueCodes.CR + ValueCodes.LF;
                        index += 16;
                    }
                    data += "[Data]" + ValueCodes.CR + ValueCodes.LF;
                    data += (detectionType == 0x09 ? "Year, JulianDay, Hour, Min, Sec, Ant, Index, Freq, SS, Code, Mort, NumDet, Lat, Long, GpsTimestamp, Date, SessionNum" :
                        "Year, JulianDay, Hour, Min, Sec, Ant, Index, Freq, SS, PeriodHi, PeriodLo, NumDet, Lat, Long, GpsTimestamp, Date, SessionNum") + ValueCodes.CR + ValueCodes.LF;
                    while (index < bytes.Length && !GetHexValue(bytes[index]).Equals("83") && !GetHexValue(bytes[index]).Equals("82") && !GetHexValue(bytes[index]).Equals("86") && !foundStop)
                    {
                        if (GetHexValue(bytes[index]).Equals("F0")) //Header
                        {
                            frequency = baseFrequency + (bytes[index + 1] * 256) + bytes[index + 2];
                            frequencyTableIndex = (((bytes[index + 1] >> 6) & 1) * 256) + bytes[index + 3];
                            if (format.Equals("83"))
                            {
                                antenna = bytes[index + 1] >> 7;
                                if (antenna == 0)
                                    antenna = (bytes[index + 7] >> 6) + 1;
                            }
                            int date = HexToDecimal(GetHexValue(bytes[index + 4])
                                + GetHexValue(bytes[index + 5]) + GetHexValue(bytes[index + 6]));
                            MM = date / 1000000;
                            date = date % 1000000;
                            DD = date / 10000;
                            date = date % 10000;
                            hh = date / 100;
                            mm = date % 100;
                            ss = bytes[index + 7] & 0x3F;
                            dateTime = new DateTime(YY + 2000, MM, DD, hh, mm, ss);
                        }
                        else if (GetHexValue(bytes[index]).Equals("F1")) //Scan Coded
                        {
                            int secondsOffset = bytes[index + 1];
                            int signalStrength = bytes[index + 4];
                            int code = bytes[index + 3];
                            int mort = bytes[index + 5];
                            int numberDetection = bytes[index + 7];
                            dateTime.AddSeconds(secondsOffset);
                            if (bytes[index + 8] == 0xA1) //Gps
                            {
                                byte[] gpsData = new byte[16];
                                Array.Copy(bytes, index + 8, gpsData, 0, 16);
                                coordinates = GetGpsData(gpsData);
                                int year = gpsData[1];
                                MM = gpsData[2];
                                DD = gpsData[3];
                                hh = gpsData[4];
                                mm = gpsData[5];
                                ss = gpsData[6];
                                gpsTimestamp = MM + "/" + DD + "/" + year + " " + hh + ":" + mm + ":" + ss + ValueCodes.CR + ValueCodes.LF;
                                index += 16;
                            }

                            data += (dateTime.Year - 2000) + ", " + dateTime.DayOfYear + ", " + dateTime.Hour + ", " + dateTime.Minute + ", " + dateTime.Second
                                + ", " + (antenna == 0 && format.Equals("83") ? "All" : antenna.ToString()) + ", " + frequencyTableIndex + ", " + GetFrequency(frequency)
                                + ", " + signalStrength + ", " + code + ", " + mort + ", " + numberDetection + ", " + coordinates[0] + ", " + coordinates[1] + ", " + gpsTimestamp
                                + ", " + dateTime.Month + "/" + dateTime.Day + "/" + (dateTime.Year - 2000) + ", " + sessionNumber + ValueCodes.CR + ValueCodes.LF;
                        }
                        else if (GetHexValue(bytes[index]).Equals("F2")) //Scan Consolidated
                        {
                            int signalStrength = bytes[index + 4];
                            int code = bytes[index + 3];
                            int mort = (bytes[index + 6] * 256) + bytes[index + 5];
                            int numberDetection = (bytes[index + 1] * 256) + bytes[index + 7];

                            data += (dateTime.Year - 2000) + ", " + dateTime.DayOfYear + ", " + dateTime.Hour + ", " + dateTime.Minute + ", " + dateTime.Second
                                + ", " + (antenna == 0 && format.Equals("83") ? "All" : antenna.ToString()) + ", " + frequencyTableIndex + ", " + GetFrequency(frequency)
                                + ", " + signalStrength + ", " + code + ", " + mort + ", " + numberDetection + ", 0, 0, 0, " + dateTime.Month + "/"
                                + dateTime.Day + "/" + (dateTime.Year - 2000) + ", " + sessionNumber + ValueCodes.CR + ValueCodes.LF;
                        }
                        else if (GetHexValue(bytes[index]).Equals("E1") || GetHexValue(bytes[index]).Equals("E2")) //Non Coded
                        {
                            int secondsOffset = bytes[index + 1];
                            int signalStrength = bytes[index + 4];
                            int periodHi = bytes[index + 5];
                            int periodLo = bytes[index + 6];
                            dateTime.AddSeconds(secondsOffset);
                            if (bytes[index + 8] == 0xA1) //Gps
                            {
                                byte[] gpsData = new byte[16];
                                Array.Copy(bytes, index + 8, gpsData, 0, 16);
                                coordinates = GetGpsData(gpsData);
                                int year = gpsData[1];
                                MM = gpsData[2];
                                DD = gpsData[3];
                                hh = gpsData[4];
                                mm = gpsData[5];
                                ss = gpsData[6];
                                gpsTimestamp = MM + "/" + DD + "/" + year + " " + hh + ":" + mm + ":" + ss + ValueCodes.CR + ValueCodes.LF;
                                index += 16;
                            }

                            data += (dateTime.Year - 2000) + ", " + dateTime.DayOfYear + ", " + dateTime.Hour + ", " + dateTime.Minute + ", " + dateTime.Second
                                + ", " + (antenna == 0 && format.Equals("83") ? "All" : antenna.ToString()) + ", " + frequencyTableIndex + ", " + GetFrequency(frequency)
                                + ", " + signalStrength + ", " + periodHi + ", " + periodLo + ", 0, " + coordinates[0] + ", " + coordinates[1] + ", " + gpsTimestamp
                                + ", " + dateTime.Month + "/" + dateTime.Day + "/" + (dateTime.Year - 2000) + ", " + sessionNumber + ValueCodes.CR + ValueCodes.LF;
                        }
                        else if (GetHexValue(bytes[index]).Equals("87")) //End Scan
                        {
                            int scanSession = (bytes[index + 1] * 65536) + (bytes[index + 2] * 256) + bytes[index + 3];
                            data += "[Footer]" + ValueCodes.CR + ValueCodes.LF;
                            data += "Session Num: " + scanSession + ValueCodes.CR + ValueCodes.LF;
                            int date = HexToDecimal(GetHexValue(bytes[index + 12])
                                + GetHexValue(bytes[index + 13]) + GetHexValue(bytes[index + 14]));
                            MM = date / 1000000;
                            date = date % 1000000;
                            DD = date / 10000;
                            date = date % 10000;
                            hh = date / 100;
                            mm = date % 100;
                            ss = bytes[index + 15];
                            data += "Time Stamp: " + MM + "/" + DD + "/" + YY + " " + hh + ":" + mm + ":" + ss + ValueCodes.CR + ValueCodes.LF;
                            if (scanSession == sessionNumber)
                                sessionNumber++;
                            index += 8;
                            foundStop = true;
                        }
                        index += 8;
                    }
                }
                else if (format.Equals("86")) //Manual Scan
                {
                    data += "[Header]" + ValueCodes.CR + ValueCodes.LF;
                    data += "Scan Type: Manual" + ValueCodes.CR + ValueCodes.LF;
                    detectionType = (byte)(bytes[index + 1] > 0x80 ? bytes[index + 1] - 0x80 : bytes[index + 1]);
                    string detection = "Coded";
                    if (detectionType == 0x08)
                        detection = "Non Coded Fixed Pulse Rate";
                    else if (detectionType == 0x07)
                        detection = "Non Coded Variable Pulse Rate";
                    data += "Transmitter Detection Type: " + detection + ValueCodes.CR + ValueCodes.LF;
                    data += "Transmitter Detection Details: " + ValueCodes.CR + ValueCodes.LF;
                    data += "[Data]" + ValueCodes.CR + ValueCodes.LF;
                    YY = bytes[index + 2];
                    MM = bytes[index + 3];
                    DD = bytes[index + 4];
                    hh = bytes[index + 5];
                    mm = bytes[index + 6];
                    ss = bytes[index + 7];
                    dateTime = new DateTime(YY + 2000, MM, DD, hh, mm, ss);

                    if (GetHexValue(bytes[index + 8]).Equals("D0")) //Coded
                    {
                        data += "Year, JulianDay, Hour, Min, Sec, Ant, Index, Freq, SS, Code, Mort, NumDet, Lat, Long, GpsTimestamp, Date, SessionNum" + ValueCodes.CR + ValueCodes.LF;
                        frequency = baseFrequency + (bytes[index + 9] * 256) + bytes[index + 10];
                        int signalStrength = bytes[index + 11];
                        int code = bytes[index + 12];
                        int mort = bytes[index + 13];
                        if (bytes[index + 8] == 0xA1) //Gps
                        {
                            byte[] gpsData = new byte[16];
                            Array.Copy(bytes, index + 8, gpsData, 0, 16);
                            coordinates = GetGpsData(gpsData);
                            int year = gpsData[1];
                            MM = gpsData[2];
                            DD = gpsData[3];
                            hh = gpsData[4];
                            mm = gpsData[5];
                            ss = gpsData[6];
                            gpsTimestamp = MM + "/" + DD + "/" + year + " " + hh + ":" + mm + ":" + ss + ValueCodes.CR + ValueCodes.LF;
                            index += 16;
                        }

                        data += (dateTime.Year - 2000) + ", " + dateTime.DayOfYear + ", " + dateTime.Hour + ", " + dateTime.Minute + ", " + dateTime.Second
                                + ", 0, " + frequencyTableIndex + ", " + GetFrequency(frequency) + ", " + signalStrength + ", " + code + ", " + mort
                                + ", 0, " + coordinates[0] + ", " + coordinates[1] + ", " + gpsTimestamp + ", " + dateTime.Month + "/" + dateTime.Day + "/" + (dateTime.Year - 2000)
                                + ", " + sessionNumber + ValueCodes.CR + ValueCodes.LF;
                    }
                    else if (GetHexValue(bytes[index + 8]).Equals("E0")) //Non Coded
                    {
                        data += "Year, JulianDay, Hour, Min, Sec, Ant, Index, Freq, SS, PeriodHi, PeriodLo, NumDet, Lat, Long, GpsTimestamp, Date, SessionNum" + ValueCodes.CR + ValueCodes.LF;
                        frequency = baseFrequency + (bytes[index + 9] * 256) + bytes[index + 10];
                        int signalStrength = bytes[index + 11];
                        if (bytes[index + 8] == 0xA1) //Gps
                        {
                            byte[] gpsData = new byte[16];
                            Array.Copy(bytes, index + 8, gpsData, 0, 16);
                            coordinates = GetGpsData(gpsData);
                            int year = gpsData[1];
                            MM = gpsData[2];
                            DD = gpsData[3];
                            hh = gpsData[4];
                            mm = gpsData[5];
                            ss = gpsData[6];
                            gpsTimestamp = MM + "/" + DD + "/" + year + " " + hh + ":" + mm + ":" + ss + ValueCodes.CR + ValueCodes.LF;
                            index += 16;
                        }

                        data += (dateTime.Year - 2000) + ", " + dateTime.DayOfYear + ", " + dateTime.Hour + ", " + dateTime.Minute + ", " + dateTime.Second + ", 0, " + frequencyTableIndex
                            + ", " + GetFrequency(frequency) + ", " + signalStrength + ", 0, 0, 0, " + coordinates[0] + ", " + coordinates[1] + ", " + gpsTimestamp + ", "
                            + dateTime.Month + "/" + dateTime.Day + "/" + (dateTime.Year - 2000) + ", " + sessionNumber + ValueCodes.CR + ValueCodes.LF;
                    }
                    index += 16;
                    if (GetHexValue(bytes[index + 16]).Equals("87")) //End Scan
                    {
                        int scanSession = (bytes[index + 1] * 65536) + (bytes[index + 2] * 256) + bytes[index + 3];
                        data += "[Footer]" + ValueCodes.CR + ValueCodes.LF;
                        data += " Session Num: " + scanSession + ValueCodes.CR + ValueCodes.LF;
                        int date = HexToDecimal(GetHexValue(bytes[index + 12])
                            + GetHexValue(bytes[index + 13]) + GetHexValue(bytes[index + 14]));
                        MM = date / 1000000;
                        date = date % 1000000;
                        DD = date / 10000;
                        date = date % 10000;
                        hh = date / 100;
                        mm = date % 100;
                        ss = bytes[index + 15];
                        data += "Time Stamp: " + MM + "/" + DD + "/" + YY + " " + hh + ":" + mm + ":" + ss + ValueCodes.CR + ValueCodes.LF;
                        if (scanSession == sessionNumber)
                            sessionNumber++;
                        index += 16;
                    }
                    index += 16;
                }
                else
                {
                    index += 8;
                }
            }
            return data;
        }

        public static bool PrintSnapshotFiles(string root, List<Snapshots> snapshotArray)
        {
            int index = 0;
            FileStream file;
            try
            {
                if (!Directory.Exists(root))
                {
                    var info = Directory.CreateDirectory(root);
                    if (!info.Exists)
                        throw new Exception("Folder 'atstrack' can't be created.");
                }
                while (index < snapshotArray.Count)
                {
                    string fileName = snapshotArray[index].GetFileName();
                    file = File.Create(Path.Combine(root, fileName));

                    file.Write(snapshotArray[index].GetSnapshot(), 0, snapshotArray[index].GetSnapshot().Length);
                    file.Flush();
                    file.Close();

                    index++;
                }

                return index == snapshotArray.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " // " + ex.StackTrace);
            }
            return false;
        }
    }
}
