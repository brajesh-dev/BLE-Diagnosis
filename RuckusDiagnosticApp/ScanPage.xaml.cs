using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Linq;
using System.Windows.Media;

namespace RuckusDiagnosticApp
{
    public partial class ScanPage : Page
    {
        private BluetoothLEAdvertisementWatcher btwatcher;
        private BluetoothLEDevice device;
        private DeviceWatcher watcher;
        private string deviceName;
        private bool isDeviceConnected = false;
        private LogsPage logsPage;
        // Watcher configurations
        private readonly string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

        public ScanPage(LogsPage logsPage)
        {
            InitializeComponent();
            this.logsPage = logsPage;
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() => {
                LogTextBox.AppendText(message + Environment.NewLine);
                LogTextBox.ScrollToEnd();
            });
        }

        private void StartBluetoothWatcher()
        {
            btwatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
            btwatcher.Received += OnAdvertisementReceived;
            btwatcher.Start();
        }

        private void StopBluetoothWatcher()
        {
            if (btwatcher != null)
            {
                btwatcher.Stop();
                btwatcher.Received -= OnAdvertisementReceived;
                btwatcher = null;
                Log("Bluetooth scanning stopped.");
            }
        }
        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            string deviceName = "Unknown";
            if (!string.IsNullOrEmpty(args.Advertisement.LocalName))
            {
                deviceName = args.Advertisement.LocalName;
            }

            ulong address = args.BluetoothAddress;
            string bluetoothAddress = ConvertBluetoothAddressToString(address);

            StringBuilder advertisementData = new StringBuilder();
            foreach (var dataSection in args.Advertisement.DataSections)
            {
                var reader = DataReader.FromBuffer(dataSection.Data);
                byte[] data = new byte[dataSection.Data.Length];
                reader.ReadBytes(data);
                advertisementData.AppendFormat("Type: 0x{0:X2}, Data: {1}", dataSection.DataType, BitConverter.ToString(data)).AppendLine();
            }

            Log($"#RuckusDiagnostics: Device found: {deviceName}, RSSI: {args.RawSignalStrengthInDBm}, Address: {bluetoothAddress}");
            Log($"Advertisement Data:\n{advertisementData}");
        }

        private string ConvertBluetoothAddressToString(ulong address)
        {
            return string.Join(":", BitConverter.GetBytes(address).Reverse().Select(b => b.ToString("X2")));
        }

        private void StartWatcherButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StartBluetoothWatcher();
        }
        private void StopWatcherButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StopBluetoothWatcher();
        }
        private void ScanTypeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ScanTypeComboBox.SelectedIndex = 0;
        }

        private void ScanTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScanTypeComboBox.SelectedIndex == 0)
            {
                ScanTypeComboBox.Foreground = Brushes.Gray;
            }
            else
            {
                ScanTypeComboBox.Foreground = Brushes.Black;
            }
        }
        private void DiscTypeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            DiscTypeComboBox.SelectedIndex = 0;
        }

        private void DiscTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DiscTypeComboBox.SelectedIndex == 0)
            {
                DiscTypeComboBox.Foreground = Brushes.Gray;
            }
            else
            {
                DiscTypeComboBox.Foreground = Brushes.Black;
            }
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
