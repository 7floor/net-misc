using SharpAquosControl;

namespace SharpAquosControlService
{
    internal class Configuration
    {
        public Connection Connection { get; set; }
        public Sequence OnSequence { get; set; }
        public Sequence OffSequence { get; set; }
    }

    internal class Sequence
    {
        public string[] Commands { get; set; }
        public MacroOptions Options { get; set; } 
    }

    internal class Connection
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
