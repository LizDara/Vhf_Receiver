using VhfReceiver.Utils;
using Xamarin.Forms;

namespace VhfReceiver.Widgets
{
    public partial class ReceiverStatus : ContentView
    {
        public ReceiverInformation ReceiverInformation;
        private string status;
        public string Status
        {
            set
            {
                status = value;
                OnPropertyChanged(nameof(Status));
            }
            get { return status; }
        }

        public ReceiverStatus()
        {
            InitializeComponent();
            BindingContext = this;

            ReceiverInformation = ReceiverInformation.GetReceiverInformation();
            Range.Text = ReceiverInformation.GetDeviceRange();
            Battery.Text = ReceiverInformation.GetDeviceBattery();
            Status = ReceiverInformation.GetDeviceStatus();
        }
    }
}
