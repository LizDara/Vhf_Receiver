using System;
namespace VhfReceiver.Utils
{
    public class VhfReceiverUuids
    {
        public static Guid UUID_SERVICE_DIAGNOSTIC = Guid.Parse("fab2d796-3364-4b54-b9a1-7735545814ad");
        public static Guid UUID_SERVICE_STORED_DATA = Guid.Parse("609d10ad-d22d-48f3-9e6e-d035398c3606");
        public static Guid UUID_SERVICE_SCAN = Guid.Parse("8d60a8bb-1f60-4703-92ff-411103c493e6");
        public static Guid UUID_SERVICE_SCREEN = Guid.Parse("26da3d0d-9119-48bb-af48-b0b96c665a66");
        public static Guid UUID_CHARACTERISTIC_DIAGNOSTIC_INFO = Guid.Parse("42d03a17-ebe1-4072-97a5-393f4a0515d7");
        public static Guid UUID_CHARACTERISTIC_BOARD_STATUS = Guid.Parse("cae6ad69-2a38-4285-b6f8-6a2f8517d1fd");
        public static Guid UUID_CHARACTERISTIC_FREQ_TABLE = Guid.Parse("ad0ea6e5-d93a-47a5-a6fc-a930552520dd");
        public static Guid UUID_CHARACTERISTIC_STUDY_DATA = Guid.Parse("91dced42-a8ee-4d9d-aecf-dbd22d390568");
        public static Guid UUID_CHARACTERISTIC_GPS = Guid.Parse("508cc6de-09ae-419a-81e7-c1546749b8a8");
        public static Guid UUID_CHARACTERISTIC_AERIAL = Guid.Parse("111584dd-b374-417c-a51d-9314eba66d6c");
        public static Guid UUID_CHARACTERISTIC_STATIONARY = Guid.Parse("6dd91f4d-b30b-46c4-b111-dd49cd1f952e");
        public static Guid UUID_CHARACTERISTIC_TX_TYPE = Guid.Parse("4d416911-153d-4eaa-b162-390b25d74e6d");
        public static Guid UUID_CHARACTERISTIC_SEND_SCREEN = Guid.Parse("2921e5a4-30f4-45c2-a556-bbfb7dd37002");
        public static Guid UUID_CHARACTERISTIC_SEND_LOG = Guid.Parse("7052b8df-95f9-4ba3-8324-0d8ff9232435");
    }
}
