using System;

namespace Rml.Communication.Direct
{
    public class ProxyCommunicationService : MarshalByRefObject, ICommunicationService
    {
        public event EventHandler<ReceiveEventArgs> Receive;
        public event EventHandler<LogEventArgs> Loged;

        public event EventHandler<ReceiveEventArgs> Sent;

        public bool Send(byte[] buffer)
        {
            Sent(this, new ReceiveEventArgs(buffer));

            return true;
        }

        public int GetConnectCount()
        {
            return Sent.GetInvocationList().Length;
        }

        public void RaiseReceive(byte[] buffer)
        {
            Receive(this, new ReceiveEventArgs(buffer));
        }
    }
}