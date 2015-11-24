using NUnit.Framework;
using Net.DDP.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Net.DDP.Client.Queueing;

namespace Net.DDP.Client.UnitTest
{
    [TestFixture()]
    public class DefaultQueueProcessorTests
    {
        [Test()]
        public void QueueItem_AllItemsShouldGetProcess_WhenItemsGetFasterAddedThanProcessed()
        {
            /** ---- Arrange ---- */
            Mock<IDeserializer> mockDeserializer = new Mock<IDeserializer>();
            mockDeserializer.Setup(f => f.Deserialize(It.IsAny<string>()))
                   .Callback(() => Thread.Sleep(50));


            var systemUnderTest = new DefaultQueueProcessor<string>(mockDeserializer.Object.Deserialize);

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

            var systemUnderTest = new DefaultQueueProcessor<string>(mockDeserializer.Object.Deserialize);

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
        public void QueueItem_ThrowsNoException_WhenAlreadyDisposed()
        {
            /** ---- Arrange ---- */
            Mock<IDeserializer> mockDeserializer = new Mock<IDeserializer>();

            var systemUnderTest = new DefaultQueueProcessor<string>(mockDeserializer.Object.Deserialize);

            /** ---- Act ---- */
            systemUnderTest.Dispose();

            /** ---- Assert ---- */
            systemUnderTest.QueueItem("This should not throw an exception.");
        }
    }
}