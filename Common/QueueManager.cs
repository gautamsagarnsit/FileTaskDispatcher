using log4net.Core;
using log4net.Repository.Hierarchy;
using RabbitMQ.Client;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using log4net;
using Common;

namespace AutoFileDispatcher.Common
{
    public class QueueManager
    {
        public RabbitMQ.Client.IChannel channel;
        private static readonly ILog logger = LogManager.GetLogger(typeof(QueueManager));
        private Dictionary<string, string> queues;
        private ConfigParser _config;

        public QueueManager()
        {
           logger.Info("QueueManager initialized.");
        }
        
        public async Task GetChannel(Dictionary<string, string> queues)
        {
            logger.Info("Getting Connection factory...");
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            logger.Info("Trying to connect to RabbitMQ...");
            IConnection connection = await factory.CreateConnectionAsync();
            logger.Info("Checking Connection...");
            if (connection.IsOpen)
            {
                logger.Info("Connected to RabbitMQ successfully.");
                channel = await connection.CreateChannelAsync();
                CreateQueues(queues);
            }
            else
            {
                logger.Error("Failed to connect to RabbitMQ.");
                throw new Exception("Failed to connect to RabbitMQ");
            }
            logger.Info("Queue Setup Finished");
            return;
        }

        private async void CreateQueues(Dictionary<string, string> queues)
        {   
            foreach (var queue in queues)
            {
                if (channel.IsOpen)
                {
                    logger.Info($"Declaring queue {queue}");
                    await channel.QueueDeclareAsync(queue: queue.Value, durable: true, exclusive: false, autoDelete: false,
                                                        arguments: new Dictionary<string, object> { { "x-queue-type", "quorum" } });
                }
                else
                {
                    logger.Error($"Cannot declare queue {queue} because channel is not open.");
                }
            }
        }


    }
}
