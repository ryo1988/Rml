using System;

namespace Rml.Communication.Direct
{
    /// <summary>
    /// 
    /// </summary>
    public class DirectCommunicationService : ICommunicationService
    {
        private DirectCommunicationService _target;

        /// <inheritdoc />
        public event EventHandler<ReceiveEventArgs> Receive;

        /// <inheritdoc />
        public event EventHandler<LogEventArgs> Loged;

        /// <inheritdoc />
        public bool Send(byte[] buffer)
        {
            var targetOnRecv = _target?.Receive;
            if (targetOnRecv == null)
            {
                return false;
            }

            if (buffer.Length == 0)
            {
                return true;
            }

            targetOnRecv(this, new ReceiveEventArgs(buffer));


            return true;
        }

        /// <inheritdoc />
        public int GetConnectCount()
        {
            return _target == null ? 0 : 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(DirectCommunicationService target)
        {
            _target = target;
            target._target = this;
        }
    }
}