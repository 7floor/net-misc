using System;
using System.Windows.Forms;
using SharpAquosControl;

namespace DemoApplication
{
    public partial class DemoForm : Form
    {
        // RSPW0/1/2 - standby levels
        public DemoForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Microsoft.Win32.SystemEvents.SessionEnded += SystemEvents_SessionEnded;
            Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Microsoft.Win32.SystemEvents.SessionEnded -= SystemEvents_SessionEnded;
            Microsoft.Win32.SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            logTextBox.AppendText("*** " + e.Mode.ToString() + Environment.NewLine);

            if (e.Mode == Microsoft.Win32.PowerModes.Suspend)
            {
                Power(false);
            }
            if (e.Mode == Microsoft.Win32.PowerModes.Resume)
            {
                Power(true);
            }
            
        }

        void SystemEvents_SessionEnded(object sender, Microsoft.Win32.SessionEndedEventArgs e)
        {
            logTextBox.AppendText("*** " + e.Reason.ToString() + Environment.NewLine);
        }

        void SendCommands(int commandRetryCount, params string[] commands)
        {
            try
            {
                using (var sharp = new SharpAquosControl.SharpAquosControl(
                    Properties.Settings.Default.TvAddress,
                    Properties.Settings.Default.TvPort,
                    Properties.Settings.Default.TvLogin,
                    Properties.Settings.Default.TvPassword))
                {
                    sharp.DataTransferred += SharpDataTransferred;
                    sharp.MacroOptions = new MacroOptions
                        {
                            CommandRetryCount = commandRetryCount,
                            CommandRetryTime = Properties.Settings.Default.CommandRetryTime,
                        };
                    sharp.PlayMacro(commands);
                }
            }
            catch (Exception ex)
            {
                logTextBox.AppendText("Exception " + ex.GetType().Name + ": " + ex.Message + Environment.NewLine);
            }
        }

        void SharpDataTransferred(object sender, DataTransferredEventArgs e)
        {
            logTextBox.AppendText(string.Format("{0} {1} \r\n", e.Sent ? "> " : "< ", e.Message));
        }

        private void Power(bool on)
        {
            logTextBox.AppendText("Sending Power " + (on ? "On" : "Off") + " sequence" + Environment.NewLine);
            SendCommands(Properties.Settings.Default.CommandRetryCount,
                (on
                  ? Properties.Settings.Default.SequencePowerOn
                  : Properties.Settings.Default.SequencePowerOff)
                .Split(','));
            logTextBox.AppendText("Done" + Environment.NewLine);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            SendCommands(1, commandTextBox.Text);
        }

        private void onButton_Click(object sender, EventArgs e)
        {
            Power(true);
        }

        private void offButton_Click(object sender, EventArgs e)
        {
            Power(false);
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            logTextBox.Clear();
        }
    }

}
