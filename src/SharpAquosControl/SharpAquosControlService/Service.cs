using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using SharpAquosControl;

namespace SharpAquosControlService
{
    public partial class Service : ServiceBase
    {
        private readonly Configuration _configuration = new Configuration();
        private StringBuilder _logLines;
        private bool _loggedError;

        public Service()
        {
            ApplyPoweEventPatch();

            InitializeComponent();
            eventLog.Source = Constants.ServiceName;
            ServiceName = Constants.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
            StartLog();
            Log("Start");
            try
            {
                ConfigurationBuilder.LoadConfiguration(_configuration);
                SwitchPower(true);
            }
            catch (Exception ex)
            {
                Log(ex);
                throw;
            }
            finally
            {
                EndLog();
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {

            switch (powerStatus)
            {
                case PowerBroadcastStatus.ResumeSuspend:
                    StartLog();
                    Log(powerStatus.ToString());
                    SwitchPower(true);
                    EndLog();
                    break;
                case PowerBroadcastStatus.Suspend:
                    StartLog();
                    Log(powerStatus.ToString());
                    SwitchPower(false);
                    EndLog();
                    break;
            }

            return true;
        }

        protected override void OnShutdown()
        {
            StartLog();
            Log("Shutdown");
            SwitchPower(false);
            EndLog();
        }

        private void ResolveHost()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    System.Net.Dns.GetHostAddresses(_configuration.Connection.Host);
                    var ping = new Ping();
                    var result = ping.Send(_configuration.Connection.Host);
                    if (result != null && result.Status == IPStatus.Success)
                        break;
                }
                catch (SocketException)
                {
                }
                Log("Unknown or unreachable host, retrying resolution...");
                Thread.Sleep(1000);
            }
        }

        private void SwitchPower(bool on)
        {
            ResolveHost();
            try
            {
                using (var sharp = new SharpAquosControl.SharpAquosControl(
                    _configuration.Connection.Host,
                    _configuration.Connection.Port,
                    _configuration.Connection.Login,
                    _configuration.Connection.Password))
                {
                    sharp.DataTransferred += SharpDataTransferred;
                    sharp.MacroOptions = on ? _configuration.OnSequence.Options : _configuration.OffSequence.Options;
                    sharp.PlayMacro(on ?_configuration.OnSequence.Commands : _configuration.OffSequence.Commands);
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        private void StartLog()
        {
            _logLines = new StringBuilder();
            _loggedError = false;
        }

        private void EndLog()
        {
            if (_logLines == null)
                return;

            eventLog.WriteEntry(_logLines.ToString(), _loggedError
                                                          ? EventLogEntryType.Error
                                                          : EventLogEntryType.Information);

            _logLines = null;
        }

        private void Log(Exception exception)
        {
            _loggedError = true;
            var text = "Exception " + exception.GetType().Name + ": " + exception.Message;
            Log(text);
        }

        private void Log(string message)
        {
            var line = string.Format("{0:HH:mm:ss} {1}", DateTime.Now, message);
            _logLines.AppendLine(line);
        }

        private void SharpDataTransferred(object sender, DataTransferredEventArgs e)
        {
            Log(string.Format("{0} {1}", e.Sent ? "> " : "< ", e.Message));
        }
    }
}
