using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RuckusDiagnosticApp
{
    public partial class MainWindow : Window
    {
        private LogsPage logsPage;
        public MainWindow()
        {
            InitializeComponent();
            //MainFrame.Navigate(new ScanPage(logsPage)); // Default page
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Handle button click here based on the button content or tag
            Button button = sender as Button;
            if (button != null)
            {
                string buttonText = ((StackPanel)button.Content).Children.OfType<TextBlock>().FirstOrDefault()?.Text;
                switch (buttonText)
                {
                    case "  BLE Watcher  ":
                        // Navigate to ScanPage
                        ScanPage scanPage = new ScanPage(logsPage);
                        this.Content = scanPage;
                        break;
                    case "RemoteAccess":
                        // Navigate to LogsPage
                        ConnectPage connectPage = new ConnectPage();
                        this.Content = connectPage;
                        break;
                    case "Settings":
                        // Navigate to SettingsPage
                        break;
                }
            }
        }
            private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
/*
        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ScanPage(logsPage));
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ConnectPage());
        }

        private void LogsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LogsPage());
        }
*/
    }
}
