using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rml.Communication.Direct.Test
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

            var a = new DirectCommunicationService();
            var b = new DirectCommunicationService();

            a.SetTarget(b);

            var recvA = false;
            var recvB = false;
            a.Receive += (s, e) =>
            {
                if (e.Buffer.SequenceEqual(data))
                {
                    recvA = true;
                }
            };
            b.Receive += (s, e) =>
            {
                if (e.Buffer.SequenceEqual(data))
                {
                    recvB = true;
                }
            };

            a.Send(data);
            b.Send(data);

            Assert.IsTrue(recvA);
            Assert.IsTrue(recvB);
        }
    }
}
