using SharpAquosControl;

namespace SharpAquosControlService
{
    static class ConfigurationBuilder
    {
        public static void LoadConfiguration(Configuration configuration)
        {
            configuration.Connection = new Connection
                {
                    Host = Properties.Settings.Default.Host,
                    Port = Properties.Settings.Default.Port,
                    Login = Properties.Settings.Default.Login,
                    Password = Properties.Settings.Default.Password,
                };
            configuration.OnSequence = new Sequence
                {
                    Commands = Properties.Settings.Default.OnSequence.Split(','),
                    Options = new MacroOptions
                        {
                            CommandRetryCount = Properties.Settings.Default.OnRetryCount,
                            CommandRetryTime = Properties.Settings.Default.OnRetryTime,
                        }
                };
            configuration.OffSequence = new Sequence
                {
                    Commands = Properties.Settings.Default.OffSequence.Split(','),
                    Options = new MacroOptions
                        {
                            CommandRetryCount = Properties.Settings.Default.OffRetryCount,
                            CommandRetryTime = Properties.Settings.Default.OffRetryTime,
                        }
                };
        }
    }
}
