using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public class WidgetCreation
    {
        public static Button NewBaseButton(int baseNumber)
        {
            Button button = new Button();
            button.BorderColor = Color.FromRgb(203, 210, 217);
            button.BorderWidth = 1;
            button.CornerRadius = 15;
            button.FontSize = 16;
            button.TextColor = Color.FromRgb(31, 41, 51);
            button.FontAttributes = FontAttributes.Bold;
            button.HeightRequest = 32;
            button.Text = baseNumber.ToString();
            return button;
        }

        public static Label CreateLabel()
        {
            Label label = new Label();
            label.HorizontalTextAlignment = TextAlignment.Center;
            label.VerticalTextAlignment = TextAlignment.Center;
            label.TextColor = Color.FromHex("1F2933");
            label.FontSize = 16;
            return label;
        }
    }
}
