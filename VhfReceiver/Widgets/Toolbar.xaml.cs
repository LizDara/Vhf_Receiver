using Xamarin.Forms;
using System;

namespace VhfReceiver.Widgets
{
    public partial class Toolbar : ContentView
    {
        public Toolbar()
        {
            InitializeComponent();
        }

        public void SetData(string title, bool showState)
        {
            TitleToolbar.Text = title;
            State.IsVisible = showState;
            Back.Clicked += Back_Clicked;
        }

        public void SetData(string title, bool showState, EventHandler back_clicked)
        {
            TitleToolbar.Text = title;
            State.IsVisible = showState;
            Back.Clicked += back_clicked;
        }

        public void SetData(string title)
        {
            TitleToolbar.Text = title;
        }

        public string GetTitle()
        {
            return TitleToolbar.Text;
        }

        public void ChangeButton(bool isBack)
        {
            Back.Source = isBack ? "Back" : "Exit";
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(false);
        }
    }
}
