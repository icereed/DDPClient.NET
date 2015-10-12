using NUnit.Framework;
using Net.DDP.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace Net.DDP.Client.UnitTest
{
    [TestFixture()]
    public class ResultQueueTests
    {
        [Test()]
        public void QueueItem_AllItemsShouldGetProcess_WhenItemsGetFasterAddedThanProcessed()
        {
            /** ---- Arrange ---- */
            Mock<IDeserializer> mockDeserializer = new Mock<IDeserializer>();
            mockDeserializer.Setup(f => f.Deserialize(It.IsAny<string>()))
                   .Callback(() => Thread.Sleep(50));


            var systemUnderTest = new ResultQueue(mockDeserializer.Object);

            var testElements = 100;

            /** ---- Act ---- */

            for (var i = 0; i < testElements; i++)
            {
                systemUnderTest.QueueItem(i.ToString());
            }

            systemUnderTest.BlockUntilNothingLeftInQueue();

            /** ---- Assert ---- */
            mockDeserializer.Verify(deserializer => deserializer.Deserialize(It.IsAny<string>()),Times.Exactly(testElements));

            systemUnderTest.Dispose();
        }

        [Test()]
        public void QueueItem_AllItemsShouldGetProcess_WhenItemsGetSlowerAddedThanProcessed()
        {
            /** ---- Arrange ---- */
            Mock<IDeserializer> mockDeserializer = new Mock<IDeserializer>();

            var systemUnderTest = new ResultQueue(mockDeserializer.Object);

            var testElements = 100;

            /** ---- Act ---- */

            for (var i = 0; i < testElements; i++)
            {
                systemUnderTest.QueueItem(i.ToString());
                systemUnderTest.BlockUntilNothingLeftInQueue();
            }


            /** ---- Assert ---- */
            mockDeserializer.Verify(deserializer => deserializer.Deserialize(It.IsAny<string>()), Times.Exactly(testElements));

            systemUnderTest.Dispose();
        }

        [Test()]
        [ExpectedException("System.InvalidOperationException")]
        public void QueueItem_ThrowsException_WhenAlreadyDisposed()
        {
            /** ---- Arrange ---- */
            Mock<IDeserializer> mockDeserializer = new Mock<IDeserializer>();

            var systemUnderTest = new ResultQueue(mockDeserializer.Object);

            /** ---- Act ---- */
            systemUnderTest.Dispose();

            /** ---- Assert ---- */
            systemUnderTest.QueueItem("This should throw an exception.");
        }
    }
}