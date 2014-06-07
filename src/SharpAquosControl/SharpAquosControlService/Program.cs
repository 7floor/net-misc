using System;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace SharpAquosControlService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                
                var parameter = args.Length == 1 ? args[0].ToLowerInvariant() : "";
                var fileName = Assembly.GetExecutingAssembly().Location;
                try
                {

                switch (parameter)
                {
                    case Constants.InstallParameter:
                        ManagedInstallerClass.InstallHelper(new[] {fileName});
                        MessageBox.Show("Service has been installed", Constants.ServiceName, MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                        break;
                    case Constants.UninstallParameter:
                        ManagedInstallerClass.InstallHelper(new[] {"/u", fileName});
                        MessageBox.Show("Service has been uninstalled", Constants.ServiceName, MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                        break;
                    default:
                        ShowUsage();
                        break;
                }
                }
                catch (Exception ex)
                {
                    var messages = new StringBuilder();
                    var currentEx = ex;
                    while (currentEx != null)
                    {
                        messages.AppendLine(currentEx.Message);
                        currentEx = currentEx.InnerException;
                    }
                    MessageBox.Show(messages.ToString(), Constants.ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                ServiceBase.Run(new Service());
            }
        }

        private static void ShowUsage()
        {
            var message = string.Format(
                "To install service:\r\n" +
                "  {0} {1}\r\n" +
                "To uninstall service:\r\n" +
                "  {0} {2}",
                Path.GetFileName(Assembly.GetExecutingAssembly().Location),
                Constants.InstallParameter,
                Constants.UninstallParameter);

            MessageBox.Show(message, Constants.ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
