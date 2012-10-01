using System;
using System.Collections.Concurrent;
using Chinchilla.Topologies.Model;
using RabbitMQ.Client.Events;

namespace Chinchilla
{
    public class DeliveryQueue : IDeliveryQueue
    {
        private const int ConsumerTakeTimeoutInMs = 10;

        private readonly IQueue queue;

        private readonly IModelReference modelReference;

        private readonly IFaultStrategy faultStrategy;

        private readonly BlockingCollection<BasicDeliverEventArgs> consumerQueue;

        public DeliveryQueue(
            IQueue queue,
            IModelReference modelReference,
            IFaultStrategy faultStrategy)
        {
            this.queue = queue;
            this.modelReference = modelReference;
            this.faultStrategy = faultStrategy;

            consumerQueue = modelReference.GetConsumerQueue(queue);
        }

        public string Name
        {
            get { return queue.Name; }
        }

        public long NumAcceptedMessages { get; set; }

        public long NumFailedMessages { get; private set; }

        public void OnAccept(IDelivery delivery)
        {
            ++NumAcceptedMessages;

            modelReference.Execute(
                m => m.BasicAck(delivery.Tag, false));
        }

        public void OnFailed(IDelivery delivery, Exception exception)
        {
            faultStrategy.ProcessFailedDelivery(delivery, exception);

            ++NumFailedMessages;
        }

        public bool TryTake(out BasicDeliverEventArgs item)
        {
            return consumerQueue.TryTake(out item, ConsumerTakeTimeoutInMs);
        }

        public void CompleteAdding()
        {
            consumerQueue.CompleteAdding();
        }

        public override string ToString()
        {
            return string.Format("[DeliveryQueue Name={0}]", Name);
        }

        public void Dispose()
        {
            consumerQueue.CompleteAdding();
            modelReference.Dispose();
        }
    }
}