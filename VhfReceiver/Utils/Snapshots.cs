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
            string month = (time.Month + 1 < 10) ? "0" + (time.Month + 1) : (time.Month + 1).ToString();
            string day = time.Day < 10 ? "0" + time.Day : time.Day.ToString();
            string hour = time.Hour < 10 ? "0" + time.Hour : time.Hour.ToString();
            string minute = time.Minute < 10 ? "0" + time.Minute : time.Minute.ToString();
            string second = time.Second < 10 ? "0" + time.Second : time.Second.ToString();

            //FileName = "D_" + (((time.Month + 1) < 10) ? "0" + (time.Month + 1) : time.Month + 1) + "_" + (((time.Day + 1) < 10) ? "0" + (time.Day + 1) : time.Day + 1) + "_" + time.Year + "_" + (((time.Hour + 1) < 10) ? "0" + (time.Hour + 1) : time.Hour + 1) + "_" + (((time.Minute + 1) < 10) ? "0" + (time.Minute + 1) : time.Minute + 1) + "_" + (((time.Second + 1) < 10) ? "0" + (time.Second + 1) : time.Second + 1) + (isRaw ? "Raw" : "") + ".txt";
            FileName = "D_" + month + "_" + day + "_" + time.Year + "_" + hour + "_" + minute + "_" + second + (isRaw ? "Raw" : "") + ".txt";
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
                FileName += " || error: " + e.Message.ToString();
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
                FileName += " || error: " + e.Message.ToString();
                Error = true;
            }
        }
    }
}
