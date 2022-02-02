using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;
using VhfReceiver.Utils;

using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class EditTable : PopupPage
    {
        private char LF = (char)0x0A;

        public EditTable(List<FrequencyInformation> frequencies)
        {
            InitializeComponent();

            SetData(frequencies);
        }

        private void SetData(List<FrequencyInformation> frequencies)
        {
            string value = "";

            for (int i = 0; i < frequencies.Count; i++)
            {
                if (i > 0)
                    value += "" + LF;
                value += frequencies[i].FrequencyNumber;
            }

            Frequencies.Text = value;
        }

        private void UndoChanges_Clicked(object sender, EventArgs e)
        {
            //MessagingCenter.Send("Cancel", "Frequencies");
            App.Current.MainPage.Navigation.PopPopupAsync(true);
        }

        private void Done_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(Frequencies.Text, "Frequencies");
            App.Current.MainPage.Navigation.PopPopupAsync(true);
        }
    }
}
