using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Moq;
using Net.DDP.Client.Queueing;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Net.DDP.Client.UnitTest
{
    [TestFixture()]
    public class DdpClientUnitTest
    {

        private static string RemoveWhitespaces(string message)
        {
            return Regex.Replace(message, @"\s+", "");
        }

        [Test()]
        public void Call_WithTwoParameters_FormsCorrectJsonString()
        {
            /* ---- Arrange ---- */
            var mock = new Mock<IDdpConnector>();

            // Calling with two parameters
            const string callWithTwoName = "testCallWithTwo";
            string callWithTwoResult = String.Empty;

            mock.Setup(foo => foo.Send(It.Is<string>(sendMessage => sendMessage.Contains(callWithTwoName))))
                .Callback<string>(((message) => { callWithTwoResult = RemoveWhitespaces(message); }));

            // Calling with two parameters in reversed order
            const string testCallWithTwoReversedName = "testCallWithTwoReversed";
            string testCallWithTwoReversedResult = String.Empty;


            mock.Setup(foo => foo.Send(It.Is<string>(sendMessage => sendMessage.Contains(testCallWithTwoReversedName))))
                .Callback<string>(((message) => { testCallWithTwoReversedResult = RemoveWhitespaces(message); }));


            // Setting up SUT
            var connector = mock.Object;
            var ddpClient = new DDPClient(connector);

            // Parameters
            var parameter1 = "sampleString";

            var parameter2 = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            var id = ddpClient.GetCurrentRequestId();

            /* ---- Act ---- */
            ddpClient.Call(callWithTwoName, parameter1, parameter2);
            ddpClient.Call(testCallWithTwoReversedName, parameter2, parameter1);


            /* ---- Assert ---- */
            var expectedResultForCallWithTwo = RemoveWhitespaces(
                    "{ \"msg\":\"method\",\"method\":\"testCallWithTwo\",\"params\":[\"sampleString\",{\"key1\":\"value1\",\"key2\":\"value2\"}],\"id\":\"" + id + "\"}");
            Assert.AreEqual(expectedResultForCallWithTwo, callWithTwoResult);

            var expectedResultForReversed = RemoveWhitespaces(
                    "{ \"msg\":\"method\",\"method\":\"testCallWithTwoReversed\",\"params\":[{\"key1\":\"value1\",\"key2\":\"value2\"}, \"sampleString\"],\"id\":\"" + (id + 1) + "\"}");
            Assert.AreEqual(expectedResultForReversed, testCallWithTwoReversedResult);

            mock.Verify(ddpConnector => ddpConnector.Send(It.IsAny<string>()), Times.Exactly(2));

        }

        [Test()]
        public void Call_WithoutParameters_FormsCorrectJsonString()
        {
            /* ---- Arrange ---- */
            var mock = new Mock<IDdpConnector>();

            // Calling with without parameter
            const string callWithoutName = "testCallWithout";
            string callWithoutResult = String.Empty;

            mock.Setup(foo => foo.Send(It.Is<string>(sendMessage => sendMessage.Contains(callWithoutName))))
                .Callback<string>(((message) => { callWithoutResult = RemoveWhitespaces(message); }));

            // Setting up SUT
            var connector = mock.Object;
            var ddpClient = new DDPClient(connector);


            var id = ddpClient.GetCurrentRequestId();

            /* ---- Act ---- */
            ddpClient.Call(callWithoutName);


            /* ---- Assert ---- */
            var expectedResultForCallWithout = RemoveWhitespaces(
                    "{ \"msg\":\"method\",\"method\":\"testCallWithout\",\"params\":[ ],\"id\":\"" + id + "\"}");
            Assert.AreEqual(expectedResultForCallWithout, callWithoutResult);


            mock.Verify(ddpConnector => ddpConnector.Send(It.IsAny<string>()), Times.Once);
        }

        [Test()]
        public void Subscribe_WithTwoParameters_FormsCorrectJsonString()
        {
            /* ---- Arrange ---- */
            var mock = new Mock<IDdpConnector>();

            // Subscribe with two parameters
            const string subscribeWithTwoName = "testSubWithParams";
            string subscribeWithTwoResult = String.Empty;

            mock.Setup(ddpConnector => ddpConnector.Send(It.Is<string>(sendMessage => sendMessage.Contains(subscribeWithTwoName))))
                .Callback<string>(((message) => { subscribeWithTwoResult = RemoveWhitespaces(message); }));


            // Setting up SUT
            var connector = mock.Object;
            var ddpClient = new DDPClient(connector);

            // Parameters
            var parameter1 = "sampleString";

            var parameter2 = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            var id = ddpClient.GetCurrentRequestId();

            /* ---- Act ---- */
            ddpClient.Subscribe(subscribeWithTwoName, parameter1, parameter2);

            /* ---- Assert ---- */
            var expectedResultForSubscribeWithTwo = RemoveWhitespaces(
                    "{ \"msg\":\"sub\",\"name\":\"testSubWithParams\",\"params\":[\"sampleString\",{\"key1\":\"value1\",\"key2\":\"value2\"}],\"id\":\"" + id + "\"}");
            Assert.AreEqual(expectedResultForSubscribeWithTwo, subscribeWithTwoResult);

            mock.Verify(ddpConnector => ddpConnector.Send(It.IsAny<string>()), Times.Once);

        }

        /// <summary>
        /// We will subscribe on a <see cref="DDPClient"/> without parameters and compare the output sent by an <see cref="IDdpConnector"/> mockup.
        /// </summary>
        [Test()]
        public void Subscribe_WithoutParameters_FormsCorrectJsonString()
        {
            /* ---- Arrange ---- */
            var mock = new Mock<IDdpConnector>();

            // Calling with without parameter
            const string subscribeWithoutName = "testSubWithout";
            string subscribeWithoutResult = String.Empty;

            mock.Setup(foo => foo.Send(It.Is<string>(sendMessage => sendMessage.Contains(subscribeWithoutName))))
                .Callback<string>(((message) => { subscribeWithoutResult = RemoveWhitespaces(message); }));


            // Setting up SUT
            var connector = mock.Object;
            var ddpClient = new DDPClient(connector);


            var id = ddpClient.GetCurrentRequestId();

            /* ---- Act ---- */
            ddpClient.Subscribe(subscribeWithoutName);


            /* ---- Assert ---- */
            var expectedResultForSubscribeWithout = RemoveWhitespaces(
                   "{ \"msg\":\"sub\",\"name\":\"testSubWithout\",\"params\":[ ],\"id\":\"" + id + "\"}");
            Assert.AreEqual(expectedResultForSubscribeWithout, subscribeWithoutResult);


            mock.Verify(ddpConnector => ddpConnector.Send(It.IsAny<string>()), Times.Once);
        }


        [Test()]
        public void ConnectAndDispose_ConnectsAndClosesAllThreadsAndConnectionsAfterwards()
        {
            /* ---- Arrange ---- */
            var mockConnector = new Mock<IDdpConnector>();
            var mockQueueProcessor = new Mock<IQueueProcessor<string>>();

            var systemUnderTest = new DDPClient(connector: mockConnector.Object, queueProcessor: mockQueueProcessor.Object);

            /* ---- Act ---- */
            systemUnderTest.Connect("example.com:1337", false);
            systemUnderTest.Dispose();

            /* ---- Assert ---- */

            // The connector must be returned as the state tracker to avoid inconsistencies.
            Assert.That(systemUnderTest.StateTracker, Is.SameAs(mockConnector.Object));

            // For Connect
            mockConnector.Verify(connector => connector.Connect("example.com:1337", false),Times.Once);

            // For Dispose
            mockConnector.Verify(connector => connector.Close(), Times.Once);
            mockQueueProcessor.Verify(queueProcessor => queueProcessor.Dispose(), Times.Once);


        }

    }
}
