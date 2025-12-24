using System;
namespace VhfReceiver.Utils
{
    public class Snapshots
    {
        public static int BYTES_PER_PAGE = 2048;
        private string FileName;
        private bool Error;
        private bool Filled;
        private byte[] Snapshot;
        public int ByteIndex;
        private int Size;

        public Snapshots(int size)
        {
            Snapshot = new byte[size];
            FileName = "";

            ByteIndex = 0;
            Filled = false;
            Error = false;
            Size = size;
        }

        public string GetFileName()
        {
            return FileName;
        }

        public byte[] GetSnapshot()
        {
            return Snapshot;
        }

        public bool IsFilled()
        {
            return Filled;
        }

        public void SetFileName(bool isRaw)
        {
            DateTime time = DateTime.Now;
            string month = (time.Month < 10) ? "0" + time.Month : time.Month.ToString();
            string day = time.Day < 10 ? "0" + time.Day : time.Day.ToString();
            string hour = time.Hour < 10 ? "0" + time.Hour : time.Hour.ToString();
            string minute = time.Minute < 10 ? "0" + time.Minute : time.Minute.ToString();
            string second = time.Second < 10 ? "0" + time.Second : time.Second.ToString();

            FileName = "D" + ReceiverInformation.GetInstance().GetSerialNumber() + "_" + month + day + (time.Year - 2000).ToString() + hour + minute + second + (isRaw ? "Raw" : "") + ".txt";
        }

        public void ProcessSnapshotRaw(byte[] packRead)
        {
            try
            {
                if (ByteIndex == 0) SetFileName(true);

                Array.Copy(packRead, 0, Snapshot, ByteIndex, packRead.Length);
                ByteIndex += packRead.Length;
                if (ByteIndex == Size) Filled = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Error = true;
            }
        }

        public void ProcessSnapshot(byte[] packRead)
        {
            try
            {
                if (packRead.Length > 0)
                {
                    if (ByteIndex == 0) SetFileName(false);

                    Array.Copy(packRead, 0, Snapshot, ByteIndex, packRead.Length);
                    ByteIndex += packRead.Length;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Error = true;
            }
        }

        public void ReplaceSnapshotRaw(byte[] packRead)
        {
            try
            {
                Array.Copy(packRead, 0, Snapshot, ByteIndex - packRead.Length, packRead.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Error = true;
            }
        }
    }
}
