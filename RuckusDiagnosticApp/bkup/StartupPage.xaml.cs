using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RuckusDiagnosticApp
{
    /// <summary>
    /// Interaction logic for StartupPage.xaml
    /// </summary>
    public partial class StartupPage : Page
    {
        private LogsPage logsPage;

        public StartupPage()
        {
            InitializeComponent();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            // Optionally, close this StartupPage window
            if (this.NavigationService != null)
            {
                NavigationService.RemoveBackEntry(); // Remove the startup page from navigation history
             //   NavigationService.Navigate(null);   // Navigate away from the current page
            }
            // Navigate to ScanPage or any other page
//            this.NavigationService.Navigate(new ScanPage(logsPage));
            // Navigate to the main window after the delay
            //MainWindow mainWindow = MainWindow();
            //mainWindow.Show();
            //this.NavigationService.Navigate(mainWindow);
            //this.NavigationService.Navigate(mainWindow);

        }
    }
}
