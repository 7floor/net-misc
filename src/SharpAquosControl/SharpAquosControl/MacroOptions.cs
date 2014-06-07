namespace SharpAquosControl
{
    public class MacroOptions
    {
        public MacroOptions()
        {
            CommandRetryCount = 1;
            CommandRetryTime = 1000;
        }

        public int CommandRetryCount { get; set; }
        public int CommandRetryTime { get; set; }
    }
}
