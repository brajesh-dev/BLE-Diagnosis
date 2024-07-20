using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace RuckusDiagnosticApp
{
    public partial class App : Application
    {
        private StartupWindow startupWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
           // DisplayStartupPage();
        }

        private async void DisplayStartupPage()
        {
            // Create and show the StartupWindow
            startupWindow = new StartupWindow();
            startupWindow.Show();

        }
    }
}