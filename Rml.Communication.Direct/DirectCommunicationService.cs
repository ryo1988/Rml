using System;

namespace Rml.Communication.Direct
{
    public class DirectCommunicationService : ICommunicationService
    {
        private DirectCommunicationService _target;

        public event EventHandler<ReceiveEventArgs> Receive;

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

        public int GetConnectCount()
        {
            return _target == null ? 0 : 1;
        }

        public void SetTarget(DirectCommunicationService target)
        {
            _target = target;
            target._target = this;
        }
    }
}