using System.Windows;
using Windows.Devices.Bluetooth;
using InTheHand.Net.Sockets; // Namespace from 32feet.NET
using System.Threading.Tasks;
using System;
using InTheHand.Net.Bluetooth;

namespace RuckusDiagnosticApp
{
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (BluetoothRadio.IsSupported)
            {
                BluetoothRadio primaryRadio = BluetoothRadio.PrimaryRadio;
                if (primaryRadio != null && primaryRadio.LocalAddress != null)
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close(); // Close the startup window
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Bluetooth is currently disabled. Do you want to enable it?", "Enable Bluetooth", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Guide the user to enable Bluetooth manually
                        MessageBox.Show("Please enable Bluetooth manually through system settings.", "Bluetooth Not Enabled", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Handle the case where user chooses not to enable Bluetooth
                        MessageBox.Show("Bluetooth is required to proceed. Please enable Bluetooth and try again.", "Bluetooth Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please Check Bluetooth on this device.", "Enable Bluetooth", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
