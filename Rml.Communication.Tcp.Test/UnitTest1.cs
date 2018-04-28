using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rml.Communication.Tcp.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var data = new byte[]
            {
                1, 2,
            };

            var a = new TcpCommunicationService();
            var b = new TcpCommunicationService();

            a.StartListener("127.0.0.1", 1988);
            b.StartClient("127.0.0.1", 1988);

            while (a.GetConnectCount() != 1 || b.GetConnectCount() != 1)
            {
            }

            var recvA = false;
            var recvB = false;
            var recvedA = false;
            var recvedB = false;
            a.Receive += (s, e) =>
            {
                if (e.Buffer.SequenceEqual(data))
                {
                    recvA = true;
                }
                recvedA = true;
            };
            b.Receive += (s, e) =>
            {
                if (e.Buffer.SequenceEqual(data))
                {
                    recvB = true;
                }
                recvedB = true;
            };

            a.Send(data);
            b.Send(data);

            while (recvedA == false || recvedB == false)
            {
            }

            Assert.IsTrue(recvA);
            Assert.IsTrue(recvB);
        }
    }
}
