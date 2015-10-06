using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSocket4Net;

namespace Net.DDP.Client.UnitTest
{
    [TestClass]
    public class DdpClientUnitTest
    {
        class TestSendImpl : IDdpConnector
        {
            public event EventHandler OnConnecting;
            public event EventHandler OnOpen;
            public event EventHandler<DdpConnectionError> OnError;
            public event EventHandler OnClosed;
            public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
            public ConnectionState State { get; }
            public string AssertSendMessage { get; set; }

            public void Close()
            {
                throw new NotImplementedException();
            }

            public void Connect(string url, bool useSsl = true)
            {
                throw new NotImplementedException();
            }

            public void Send(string message)
            {
                message = RemoveWhitespaces(message);
                var expected = RemoveWhitespaces(AssertSendMessage);
                Assert.AreEqual(expected, message);
            }

            private static string RemoveWhitespaces(string message)
            {
                return Regex.Replace(message, @"\s+", "");
            }
        }



        [TestMethod]
        public void TestCall()
        {
            var connector = new TestSendImpl();

            var ddpClient = new DDPClient(connector);

            var dict = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            var id = ddpClient.GetCurrentRequestId();
            connector.AssertSendMessage = "{ \"msg\":\"method\",\"method\":\"testCall\",\"params\":[\"sampleString\",{\"key1\":\"value1\",\"key2\":\"value2\"}],\"id\":\"" + id + "\"}";
            ddpClient.Call("testCall", "sampleString", dict);

            id++;
            Assert.AreEqual(id, ddpClient.GetCurrentRequestId());
            // Switch order
            connector.AssertSendMessage = "{ \"msg\":\"method\",\"method\":\"testCall\",\"params\":[{\"key1\":\"value1\",\"key2\":\"value2\"}, \"123456789\"],\"id\":\"" + id + "\"}";
            ddpClient.Call("testCall", dict, "123456789");

            id++;
            Assert.AreEqual(id, ddpClient.GetCurrentRequestId());

            // Without params
            connector.AssertSendMessage = "{ \"msg\":\"method\",\"method\":\"testCallWithout\",\"params\":[ ],\"id\":\"" + id + "\"}";
            ddpClient.Call("testCallWithout");
        }
    }
}
