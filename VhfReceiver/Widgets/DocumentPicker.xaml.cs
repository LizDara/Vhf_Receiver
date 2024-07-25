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

        public DocumentPicker()
        {
            InitializeComponent();

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string finalPath = Path.Combine(documentsPath, "atstrack");
            string[] files = Directory.GetFiles(finalPath);
            Files = new List<FileInformation>();

            foreach (string path in files)
                Files.Add(new FileInformation(path));

            FilesList.ItemsSource = Files;
        }

        private void FilesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            FileInformation fileInformation = e.Item as FileInformation;
            var frequenciesList = File.ReadAllLines(fileInformation.FilePath);

            MessagingCenter.Send(frequenciesList, "Tables");
            App.Current.MainPage.Navigation.PopPopupAsync(true);
        }
    }
}
