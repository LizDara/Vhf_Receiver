using System;
using System.Collections.Generic;
using VhfReceiver.Utils;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace VhfReceiver.Pages
{
    public partial class TablesPage : ContentPage
    {
        private DeviceInformation ConnectedDevice;
        private int BaseFrequency;
        private int BaseRange;

        public TablesPage(DeviceInformation device, byte[] bytes)
        {
            InitializeComponent();

            ConnectedDevice = device;
            Name.Text = ConnectedDevice.Name;
            Range.Text = ConnectedDevice.Range;
            Battery.Text = ConnectedDevice.Battery;

            SetData(bytes);
        }

        private void SetData(byte[] bytes)
        {
            TablesList.ItemsSource = null;

            TablesList.ItemsSource = new List<TableInformation>() {
                new TableInformation(1, bytes[1]),
                new TableInformation(2, bytes[2]),
                new TableInformation(3, bytes[3]),
                new TableInformation(4, bytes[4]),
                new TableInformation(5, bytes[5]),
                new TableInformation(6, bytes[6]),
                new TableInformation(7, bytes[7]),
                new TableInformation(8, bytes[8]),
                new TableInformation(9, bytes[9]),
                new TableInformation(10, bytes[10]),
                new TableInformation(11, bytes[11]),
                new TableInformation(12, bytes[12])
            };

            BaseFrequency = bytes[13];
            BaseRange = bytes[14];
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("BACK TABLES");
            await Navigation.PopModalAsync();
        }

        private async void TablesList_Tapped(object sender, ItemTappedEventArgs e)
        {
            TableInformation selectedItem = e.Item as TableInformation;
            MessagingCenter.Subscribe<byte[]>(this, "Table", (value) => {
                SetData(value);
            });

            byte[] bytes = new byte[] { 0x7E, 0x7E, 0x7E, 0x7E, 0x7E, 0x7E, 0x7E, (byte) selectedItem.Table };
            var service = await ConnectedDevice.Device.GetServiceAsync(VhfReceiverUuids.UUID_SERVICE_STORED_DATA);
            if (service != null)
            {
                var characteristic = await service.GetCharacteristicAsync(VhfReceiverUuids.UUID_CHARACTERISTIC_FREQ_TABLE);
                if (characteristic != null)
                {
                    await characteristic.WriteAsync(bytes);
                    //bytes = new byte[] { 0, 0, 0, 0, 0, 0, 0, (byte) selectedItem.Table, (byte) selectedItem.Frequencies, 0, (byte) 0, (byte) 123, (byte) 1, (byte) 200, (byte) 3, (byte) 21 };
                    bytes = new byte[] { 0 };

                    characteristic.ValueUpdated += async (o, args) =>
                    {
                        bytes = args.Characteristic.Value;
                        Console.WriteLine("OMG: " + Converters.GetDecimalValue(bytes));
                        await Navigation.PushModalAsync(new FrequenciesPage(ConnectedDevice, bytes, BaseFrequency, BaseRange));
                    };

                    await characteristic.StartUpdatesAsync();
                }
            }
        }

        private void LoadFromFile_Clicked(object sender, EventArgs e)
        {

        }
    }
}
