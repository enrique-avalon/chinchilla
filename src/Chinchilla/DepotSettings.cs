using System;
using System.Collections.Generic;

namespace Chinchilla
{
    public class DepotSettings
    {
        private const string AmqpProtocol = "amqp://";

        private const string RabbitMqProtocol = "rabbitmq://";

        private readonly List<IBusConcern> startupConcerns = new List<IBusConcern>();

        public DepotSettings()
        {
            ConnectionFactoryBuilder = () => new DefaultConnectionFactory();
            ConsumerFactoryBuilder = () => new DefaultConsumerFactory();
            MessageSerializers = new MessageSerializers();
        }

        public string ConnectionString { get; set; }

        public IMessageSerializers MessageSerializers { get; set; }

        public Func<IConnectionFactory> ConnectionFactoryBuilder { get; set; }

        public Func<IConsumerFactory> ConsumerFactoryBuilder { get; set; }

        public IEnumerable<IBusConcern> StartupConcerns
        {
            get { return startupConcerns; }
        }

        public void AddStartupConcern(IBusConcern concern)
        {
            startupConcerns.Add(concern);
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ChinchillaException("A connection string is required");
            }

            if (ConnectionFactoryBuilder == null)
            {
                throw new ChinchillaException("A connection factory builder is required");
            }

            if (ConsumerFactoryBuilder == null)
            {
                throw new ChinchillaException("A consumer factory builder is required");
            }

            if (ConnectionString.StartsWith(RabbitMqProtocol))
            {
                ConnectionString = ConnectionString.Replace(RabbitMqProtocol, AmqpProtocol);
            }

            if (!ConnectionString.StartsWith(AmqpProtocol))
            {
                ConnectionString = string.Concat(AmqpProtocol, ConnectionString);
            }
        }
    }
}