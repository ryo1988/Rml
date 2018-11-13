using System;

namespace Rml.Communication.Direct
{
    /// <summary>
    /// 
    /// </summary>
    public class ProxyCommunicationService : MarshalByRefObject, ICommunicationService
    {
        /// <inheritdoc />
        public event EventHandler<ReceiveEventArgs> Receive;

        /// <inheritdoc />
        public event EventHandler<LogEventArgs> Loged;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ReceiveEventArgs> Sent;

        /// <inheritdoc />
        public bool Send(byte[] buffer)
        {
            Sent?.Invoke(this, new ReceiveEventArgs(buffer));

            return true;
        }

        /// <inheritdoc />
        public int GetConnectCount()
        {
            return Sent?.GetInvocationList().Length ?? 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        public void RaiseReceive(byte[] buffer)
        {
            Receive?.Invoke(this, new ReceiveEventArgs(buffer));
        }
    }
}