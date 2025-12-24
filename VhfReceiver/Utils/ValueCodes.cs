using System;
namespace VhfReceiver.Utils
{
    public class ValueCodes
    {
        public const int CANCELLED = 1000;
        public const int RESULT_OK = 2000;

        //Codes
        public const int TABLE_NUMBER_CODE = 1001;
        public const int TABLES_NUMBER_CODE = 1002;
        public const int SCAN_RATE_SECONDS_CODE = 1003;
        public const int NUMBER_OF_ANTENNAS_CODE = 1004;
        public const int SCAN_TIMEOUT_SECONDS_CODE = 1005;
        public const int STORE_RATE_CODE = 1006;
        public const int REFERENCE_FREQUENCY_CODE = 1007;
        public const int REFERENCE_FREQUENCY_STORE_RATE_CODE = 1008;

        public const int PULSE_RATE_TYPE_CODE = 1009;
        public const int MATCHES_FOR_VALID_PATTERN = 1010;
        public const int CODED = 1011;
        public const int FIXED_PULSE_RATE = 1012;
        public const int VARIABLE_PULSE_RATE = 1013;
        public const int PULSE_RATE_1_CODE = 1014;
        public const int PULSE_RATE_2_CODE = 1015;
        public const int PULSE_RATE_3_CODE = 1016;
        public const int PULSE_RATE_4_CODE = 1017;
        public const int MAX_PULSE_RATE_CODE = 1018;
        public const int MIN_PULSE_RATE_CODE = 1019;
        public const int DATA_CALCULATION_TYPES = 1020;
        public const int PERIOD = 1023;
        public const int GPS_CODE = 1024;
        public const int AUTO_RECORD_CODE = 1025;

        public const string VHF = "ATSvr";
        public const string ACOUSTIC = "ATSar";
        public const string WILDLINK = "ATSwl";
        public const string BLUETOOTH_RECEIVER = "UART";//ATSbr
        public const string TAGS = "ATSbt";

        //Values
        public const string VALUE = "value";
        public const string DETECTION_TYPE = "detectionType";
        public const string DETECTION_SCAN = "Go to Detection Filter Settings";
        public const string TABLE_SCAN = "Go to Frequency Tables";
        public const string DEFAULTS_SCAN = "Go to Receiver Settings";
        public const string MOBILE = "mobile";
        public const string STATIONARY = "stationary";
        public const string FREQUENCY = "frequency";
        public const string TABLE = "table";
        public const string FILE = "file";

        //Original Data
        public const string TABLE_NUMBER = "TableNumber";
        public const string GPS = "Gps";
        public const string AUTO_RECORD = "AutoRecord";
        public const string SCAN_RATE = "ScanRate";
        public const string FIRST_TABLE_NUMBER = "FirstTableNumber";
        public const string SECOND_TABLE_NUMBER = "SecondTableNumber";
        public const string THIRD_TABLE_NUMBER = "ThirdTableNumber";
        public const string ANTENNA_NUMBER = "AntennaNumber";
        public const string SCAN_TIMEOUT = "ScanTimeout";
        public const string STORE_RATE = "StoreRate";
        public const string EXTERNAL_DATA_TRANSFER = "ExternalDataTransfer";
        public const string REFERENCE_FREQUENCY = "ReferenceFrequency";
        public const string REFERENCE_FREQUENCY_STORE_RATE = "ReferenceFrequencyStoreRate";
        public const string PULSE_RATE_TYPE = "PulseRateType";
        public const string MATCHES = "Matches";
        public const string PULSE_RATE_1 = "PulseRate1";
        public const string PULSE_RATE_2 = "PulseRate2";
        public const string PULSE_RATE_3 = "PulseRate3";
        public const string PULSE_RATE_4 = "PulseRate4";
        public const string PULSE_RATE_TOLERANCE_1 = "PulseRateTolerance1";
        public const string PULSE_RATE_TOLERANCE_2 = "PulseRateTolerance2";
        public const string PULSE_RATE_TOLERANCE_3 = "PulseRateTolerance3";
        public const string PULSE_RATE_TOLERANCE_4 = "PulseRateTolerance4";
        public const string MAX_PULSE_RATE = "MaxPulseRate";
        public const string MIN_PULSE_RATE = "MinPulseRate";
        public const string DATA_CALCULATION = "DataCalculation";
        public const string COEFFICIENT_A = "coefficientA";
        public const string COEFFICIENT_B = "coefficientB";
        public const string COEFFICIENT_C = "coefficientC";
        public const string COEFFICIENT_D = "coefficientD";

        //Periods
        public const int DISCONNECTION_MESSAGE_PERIOD = 1000;
        public const int WAITING_PERIOD = 180;
        public const int MESSAGE_PERIOD = 700;
        public const int DOWNLOAD_PERIOD = 280;
        public const int REQUEST_CODE_SIGN_IN = 1;
        public const int SCAN_PERIOD = 2000;
        public const int BRANDING_PERIOD = 2000;
        public const int CONNECT_PERIOD = 1900;
        public const int CONNECT_TIMEOUT = 5000;
        public const char CR = (char)0x0D;
        public const char LF = (char)0x0A;
        public const int REQUEST_ENABLE_BT = 1;
        public const int REQUEST_CODE_OPEN_STORAGE = 3;
    }
}
