using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;

namespace SharpAquosControlService
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();
            var logInstaller = new EventLogInstaller();

            // Service will run under system account
            processInstaller.Account = ServiceAccount.LocalSystem;

            // Service will have Start Type of Manual
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = Constants.ServiceName;

            // EventLog
            logInstaller.Log = "Application";
            logInstaller.Source = Constants.ServiceName;


            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
            Installers.Add(logInstaller);
        }

        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            var service = new ServiceController(Constants.ServiceName);
            service.Start();
        }
    }
}
