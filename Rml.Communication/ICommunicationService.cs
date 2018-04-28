using System;

namespace Rml.Communication
{
    public class ReceiveEventArgs : EventArgs
    {
        public byte[] Buffer;

        public ReceiveEventArgs(byte[] buffer)
        {
            Buffer = buffer;
        }
    }

    public interface ICommunicationService
    {
        event EventHandler<ReceiveEventArgs> Receive;
        bool Send(byte[] buffer);
        int GetConnectCount();
    }
}