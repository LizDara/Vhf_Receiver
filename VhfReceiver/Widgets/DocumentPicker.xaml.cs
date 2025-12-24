using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using System.IO;
using Xamarin.Forms;
using VhfReceiver.Utils;
using Rg.Plugins.Popup.Extensions;

namespace VhfReceiver.Widgets
{
    public partial class DocumentPicker : PopupPage
    {
        public List<FileInformation> Files;

        public DocumentPicker(string fileName)
        {
            InitializeComponent();

            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            if (!Directory.Exists(root))
            {
                var info = Directory.CreateDirectory(root);
                if (!info.Exists)
                    throw new Exception("Folder '" + fileName + "' can't be created.");
            }
            //File.WriteAllText(Path.Combine(root, "example.txt"), "Table9" + ValueCodes.CR + ValueCodes.LF + "150123");
            string[] files = Directory.GetFiles(root);
            if (files.Length > 0)
            {
                NoFiles.IsVisible = false;
                Files = new List<FileInformation>();

                foreach (string path in files)
                    Files.Add(new FileInformation(path));

                FilesList.ItemsSource = Files;
            }
            else
            {
                FilesList.IsVisible = false;
                NoFiles.Text = "No file was found in the '" + fileName + "' directory.";
            }
        }

        private void Close_Clicked(object sender, EventArgs e)
        {
            App.Current.MainPage.Navigation.PopPopupAsync(true);
        }

        private void FilesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            FileInformation fileInformation = e.Item as FileInformation;

            MessagingCenter.Send(fileInformation, ValueCodes.FILE);
            App.Current.MainPage.Navigation.PopPopupAsync(true);
        }
    }
}
