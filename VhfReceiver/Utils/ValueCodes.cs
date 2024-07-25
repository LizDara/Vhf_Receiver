using System;
namespace VhfReceiver.Utils
{
    public class ValueCodes
    {
        public const int CANCELLED = 1000;
        public const int RESULT_OK = 2000;

        public const int FREQUENCY_TABLE_NUMBER = 1001;
        public const int TABLES_NUMBER = 1002;
        public const int SCAN_RATE_SECONDS = 1003;
        public const int NUMBER_OF_ANTENNAS = 1004;
        public const int SCAN_TIMEOUT_SECONDS = 1005;
        public const int STORE_RATE = 1006;
        public const int REFERENCE_FREQUENCY = 1007;
        public const int REFERENCE_FREQUENCY_STORE_RATE = 1008;

        public const int PULSE_RATE_TYPE = 1009;
        public const int MATCHES_FOR_VALID_PATTERN = 1010;
        public const int CODED = 1011;
        public const int FIXED_PULSE_RATE = 1012;
        public const int VARIABLE_PULSE_RATE = 1013;
        public const int PULSE_RATE_1 = 1014;
        public const int PULSE_RATE_2 = 1015;
        public const int PULSE_RATE_3 = 1016;
        public const int PULSE_RATE_4 = 1017;
        public const int MAX_PULSE_RATE = 1018;
        public const int MIN_PULSE_RATE = 1019;
        public const int DATA_CALCULATION_TYPES = 1020;
        public const int NONE = 1021;
        public const int TEMPERATURE = 1022;
        public const int PERIOD = 1023;

        public const string VALUE = "value";
        public const string TX_TYPE = "state";
    }
}
