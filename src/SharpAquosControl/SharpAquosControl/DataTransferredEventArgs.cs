using System;

namespace SharpAquosControl
{
    public class DataTransferredEventArgs: EventArgs
    {
        public DataTransferredEventArgs(bool sent, string message)
        {
            Sent = sent;
            Message = message;
        }

        public bool Sent { get; private set; }
        public string Message { get; private set; }
    }
}