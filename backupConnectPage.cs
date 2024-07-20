using InTheHand.Net;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Windows.Input;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace RuckusDiagnosticApp
{
    public partial class ConnectPage : Page
    {
        public ObservableCollection<Device> Devices { get; set; }
        private BluetoothLEAdvertisementWatcher watcher;
        static BluetoothLEDevice device;
        static DeviceWatcher btwatcher;
        static string deviceName;
        static bool isDeviceConnected = false;
        private string targetDeviceName;
        private MainViewModel _viewModel;

        // Define your service UUID
        private readonly Guid sppServiceUuid = new Guid("YOUR-UUID-HERE"); // Replace with your service UUID

        // Watcher configurations
        static readonly string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

        public ConnectPage()
        {
            InitializeComponent();
            _viewModel = new MainViewModel(sppServiceUuid);
            DataContext = _viewModel;
            DevicesListView.ItemsSource = _viewModel.Devices;
        }

        async static void DeviceAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            // If the random device matches our device name, connect to it.
            Console.WriteLine("Device found: " + deviceInfo.Name);
            if (deviceInfo.Name.CompareTo(deviceName) == 0)
            {
                // Get the bluetooth object and save it. This function will connect to the device
                device = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
                Console.WriteLine("Connected to " + device.Name);

                isDeviceConnected = true;
            }
        }

        private void InitializeBluetoothWatcher()
        {
            // Create the Bluetooth LE advertisement watcher
            watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            watcher.Received += OnAdvertisementReceived;
            watcher.Start();
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            Dispatcher.Invoke(async () =>
            {
                var deviceName = args.Advertisement.LocalName;
                var bluetoothAddress = args.BluetoothAddress.ToString("X");
                var signalStrength = args.RawSignalStrengthInDBm;

                if (!string.IsNullOrEmpty(deviceName) && deviceName.ToLower().Contains(targetDeviceName.ToLower()))
                {
                    var existingDevice = _viewModel.Devices.FirstOrDefault(d => d.BluetoothAddress == bluetoothAddress);
                    if (existingDevice != null)
                    {
                        existingDevice.SignalStrength = signalStrength;
                    }
                    else
                    {
                        // Get the device ID for later use
                        var deviceInfo = await DeviceInformation.CreateFromIdAsync(bluetoothAddress);
                        var device = new Device
                        {
                            Name = deviceName,
                            Status = "Available",
                            SignalStrength = signalStrength,
                            BluetoothAddress = bluetoothAddress,
                            DeviceId = deviceInfo.Id // Store the device ID
                        };

                        _viewModel.Devices.Add(device);
                    }
                }
            });
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the target device name for filtering
            targetDeviceName = "empty";

            InitializeBluetoothWatcher();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            watcher.Stop();
            Application.Current.Shutdown();
        }
    }

    public class Device : INotifyPropertyChanged
    {
        private string _name;
        private string _bluetoothAddress;
        private double _signalStrength;
        private string _deviceId; // Add DeviceId property
        private string _status;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string BluetoothAddress
        {
            get { return _bluetoothAddress; }
            set
            {
                if (_bluetoothAddress != value)
                {
                    _bluetoothAddress = value;
                    OnPropertyChanged(nameof(BluetoothAddress));
                }
            }
        }
        public string DeviceId
        {
            get { return _deviceId; }
            set
            {
                if (_deviceId != value)
                {
                    _deviceId = value;
                    OnPropertyChanged(nameof(DeviceId));
                }
            }
        }
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public double SignalStrength
        {
            get { return _signalStrength; }
            set
            {
                if (_signalStrength != value)
                {
                    _signalStrength = value;
                    OnPropertyChanged(nameof(SignalStrength));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private Device _selectedDevice;
        public ObservableCollection<Device> Devices { get; set; } = new ObservableCollection<Device>();

        private readonly Guid sppServiceUuid;

        public Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
                ConnectToRuckusAPCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand ConnectToRuckusAPCommand { get; }

        public MainViewModel(Guid serviceUuid)
        {
            sppServiceUuid = serviceUuid;
            ConnectToRuckusAPCommand = new RelayCommand(async (param) => await ConnectToRuckusAP(), CanConnectToRuckusAP);
        }

        private async Task ConnectToRuckusAP()
        {
            MessageBox.Show($"Connecting...");

            if (SelectedDevice != null)
            {
                try
                {
                    var bleDevice = await BluetoothLEDevice.FromIdAsync(SelectedDevice.DeviceId);
                    if (bleDevice != null)
                    {
                        MessageBox.Show($"Connected to {bleDevice.Name}");
                        await FetchGattServicesAsync(bleDevice);
                    }
                    else
                    {
                        MessageBox.Show("Failed to connect to device.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error connecting to device: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a device to connect.");
            }
        }

        private bool CanConnectToRuckusAP(object parameter)
        {
            return SelectedDevice != null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task FetchGattServicesAsync(BluetoothLEDevice bleDevice)
        {
            if (bleDevice == null)
            {
                MessageBox.Show("No device connected.");
                return;
            }

            try
            {
                GattDeviceServicesResult serviceResult = await bleDevice.GetGattServicesForUuidAsync(sppServiceUuid, BluetoothCacheMode.Uncached);
                if (serviceResult.Status == GattCommunicationStatus.Success)
                {
                    var services = serviceResult.Services;
                    foreach (var service in services)
                    {
                        Console.WriteLine("Service: " + service.Uuid);
                    }
                }
                else
                {
                    MessageBox.Show("Error fetching services: " + serviceResult.Status);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
