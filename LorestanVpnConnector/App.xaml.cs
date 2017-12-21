using System.Diagnostics;
using System.Windows;

namespace LorestanVpnConnector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                MessageBox.Show("Application is already running.");
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
