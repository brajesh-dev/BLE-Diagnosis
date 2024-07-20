using System;
using System.Windows.Controls;

namespace RuckusDiagnosticApp
{
    public partial class LogsPage : Page
    {
        public LogsPage()
        {
            InitializeComponent();
        }

        public void Log(string message)
        {
            LogTextBox.Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText(message + Environment.NewLine);
                LogTextBox.ScrollToEnd();
            });
        }
    }
}
