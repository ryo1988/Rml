using System;

namespace Rml.Communication
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ReceiveEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        public ReceiveEventArgs(byte[] buffer)
        {
            Buffer = buffer;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public string Log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        public LogEventArgs(string log)
        {
            Log = log;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICommunicationService
    {
        /// <summary>
        /// 
        /// </summary>
        event EventHandler<ReceiveEventArgs> Receive;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<LogEventArgs> Loged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        bool Send(byte[] buffer);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetConnectCount();
    }
}