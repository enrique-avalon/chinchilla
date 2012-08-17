using System;
using Chinchilla.Topologies;

namespace Chinchilla
{
    public class SubscriptionConfiguration : ISubscriptionConfiguration, ISubscriptionBuilder
    {
        private Func<IDeliveryProcessor, IDeliveryStrategy> strategyBuilder = handler => new ImmediateDeliveryStrategy();

        public SubscriptionConfiguration()
        {
            MessageTopologyBuilder = new DefaultMessageTopologyBuilder();
        }

        public IMessageTopologyBuilder MessageTopologyBuilder { get; set; }

        public string QueueName { get; private set; }

        public ISubscriptionBuilder DeliverUsing<TStrategy>(params Action<TStrategy>[] configurations)
            where TStrategy : IDeliveryStrategy, new()
        {
            strategyBuilder = handler =>
            {
                var strategy = new TStrategy();
                foreach (var configuration in configurations)
                {
                    configuration(strategy);
                }
                return strategy;
            };

            return this;
        }

        public IDeliveryStrategy BuildDeliveryStrategy(IDeliveryProcessor processor)
        {
            var consumer = strategyBuilder(processor);
            consumer.ConnectTo(processor);
            return consumer;
        }

        public ISubscriptionBuilder SetTopology(IMessageTopologyBuilder messageTopologyBuilder)
        {
            MessageTopologyBuilder = messageTopologyBuilder;
            return this;
        }

        public ISubscriptionBuilder SubscribeOn(string subscriptionQueueName)
        {
            QueueName = subscriptionQueueName;
            return this;
        }

        public IMessageTopology BuildTopology(IEndpoint endpoint)
        {
            return MessageTopologyBuilder.Build(endpoint);
        }

        public override string ToString()
        {
            return string.Format("[SubscriptionConfiguration]");
        }
    }
}