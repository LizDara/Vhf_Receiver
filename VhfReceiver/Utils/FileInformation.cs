using System;
namespace VhfReceiver.Utils
{
    public class FileInformation
    {
        public string FileName
        {
            get;
            set;
        }

        public string FilePath
        {
            get;
            set;
        }

        public FileInformation(string path)
        {
            var splitName = path.Split(new char[] { '/' });
            FileName = splitName[splitName.Length - 1];
            FilePath = path;
        }
    }
}
