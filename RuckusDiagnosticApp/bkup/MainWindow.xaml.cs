using System.Windows;

namespace RuckusDiagnosticApp
{
    public partial class MainWindow : Window
    {
        private LogsPage logsPage;
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new ScanPage(logsPage)); // Default page
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

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
    }
}
