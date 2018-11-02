using System;

namespace Rml.Communication
{
    [Serializable]
    public class ReceiveEventArgs : EventArgs
    {
        public byte[] Buffer { get; set; }

        public ReceiveEventArgs(byte[] buffer)
        {
            Buffer = buffer;
        }
    }

    [Serializable]
    public class LogEventArgs : EventArgs
    {
        public string Log;

        public LogEventArgs(string log)
        {
            Log = log;
        }
    }

    public interface ICommunicationService
    {
        event EventHandler<ReceiveEventArgs> Receive;
        event EventHandler<LogEventArgs> Loged;
        bool Send(byte[] buffer);
        int GetConnectCount();
    }
}