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
using System.Windows.Input;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace RuckusDiagnosticApp
{
    public partial class ConnectPage : Page
    {
        public ObservableCollection<Device> Devices { get; set; }
        private BluetoothLEAdvertisementWatcher watcher;
        static BluetoothLEDevice device;
        static string deviceName;
        static bool isDeviceConnected = false;
        private string targetDeviceName;
        private MainViewModel _viewModel;

        // Watcher configurations
        static readonly string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

        // UUIDs for SPP
        static Guid sppServiceUuid = new Guid("4880c12c-fdcb-4077-8920-a450d7f9b907");

        public ConnectPage()
        {
            InitializeComponent();
            _viewModel = new MainViewModel(sppServiceUuid);
            DataContext = _viewModel;
            DevicesListView.ItemsSource = _viewModel.Devices;
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
            Dispatcher.Invoke(() =>
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
                        // Add new device
                        var device = new Device
                        {
                            Name = deviceName,
                            Status = "Available",
                            SignalStrength = signalStrength,
                            BluetoothAddress = bluetoothAddress,
                            DeviceId = args.BluetoothAddress.ToString() // Store the Bluetooth address for connection
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
        // Gatt handlers
        static GattDeviceService sppServiceHandle;
        static GattCharacteristic sppCharacteristicHandle;
        static Guid sppCharacteristicUuid = new Guid("fec26ec4-6d71-4442-9f81-55bc21d658d6");


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
                    var bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(ulong.Parse(SelectedDevice.DeviceId));
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

        #region BLE Characteristic Callbakcs
        static void CharactisticUpdated(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            string str = reader.ReadString(args.CharacteristicValue.Length);
            Console.WriteLine(str);

            // Windows doesn't handle new line i.e. pressing ENTER key will make send '\r'. This if
            // statement is to handle a new line if the return key is every read.
            if (str.Contains("\r"))
            {
                Console.WriteLine('\n');
            }
        }
        #endregion
        static async Task SubscribeToDeviceSPP()
        {
            // -----------------------------------------------------------------------------
            // Find the spp_data characteristic in the service.

            // Get all of the characteristics in the selected service.
            GattCharacteristicsResult result = await sppServiceHandle.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
            if (result.Status == GattCommunicationStatus.Success)
            {

                // -------------------------------
                // Get the spp_data characteristic. Parse through the list.
                foreach (var charateristic in result.Characteristics)
                {
                    if (charateristic.Uuid == sppCharacteristicUuid)
                    {
                        sppCharacteristicHandle = charateristic;    // save the characteristic for later use

                        // -------------------------------
                        // Write to descriptor. Even though the descriptor does not exist in the BLE device's GATT database,
                        // this needs to be called to get notifications to work. (Do not know why...)
                        GattCommunicationStatus status = await sppCharacteristicHandle.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        if (status == GattCommunicationStatus.Success)
                        {
                            sppCharacteristicHandle.ValueChanged += CharactisticUpdated;    // Add callback whenever BLE device notifies windows
                        }

                    }
                }
            }

            // -----------------------------------------------------------------------------
            // Could not find service. User needs to restart program.
            if (sppCharacteristicHandle == null)
            {
                throw new Exception("Could not find characteristic on your device. Please make sure your device has the correct gatt UUID.");
            }
            else {
                HandleDeviceInputsToDevice();
            }
        }

        static async Task HandleDeviceInputsToDevice()
        {
            var writer = new DataWriter();
            while (true)
            {
                // -------------------------------
                // Nonblocking. This is important when the device disconnects. If readline was used instead of this method,
                // the program would be blocked until the user enters a key.
                if (Console.KeyAvailable)
                {
                    // Since we checked if key was available, this will be nonblocking
                    var input = Console.ReadKey();

                    // Make sure input is not -1
                    if (input.KeyChar >= 0)
                    {
                        // ReadKey doesn't handle new line i.e. pressing ENTER key will make input = '\r'. This if
                        // statement is to handle a new line.
                        if (input.KeyChar == '\r')
                        {
                            writer.WriteByte(Convert.ToByte('\n'));
                            Console.WriteLine('\n');
                        }

                        writer.WriteByte(Convert.ToByte(input.KeyChar));
                        GattCommunicationStatus status = await sppCharacteristicHandle.WriteValueAsync(writer.DetachBuffer());
                    }
                }
            }
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
                        MessageBox.Show("Service: " + service.Uuid);
                        sppServiceHandle = service;
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
            // Could not find service. User needs to restart program.
            if (sppServiceHandle == null)
            {
                throw new Exception("Could not find service on your device. Please make sure your device has the correct gatt UUID.");
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
