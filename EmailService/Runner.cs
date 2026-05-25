using AutoFileDispatcher.Common;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EmailService
{
    public class Runner
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Runner));
        private QueueManager _queueManager;
        private readonly string _handlerName = "EmailService" ;
        private XDocument _doc;
        private readonly string _configPath = "C:\\Users\\gauta\\source\\repos\\AutoFileDispatcher\\EmailService\\config.xml";
        private Dictionary<string, string> queues;
        public Runner()
        {
            _logger.Info("EmailService Runner initialized.");
            queues = new Dictionary<string, string>();
            _queueManager = new QueueManager();
            ParseConfig();
        }

        public async  void Run()
        {
            _queueManager.GetChannel(queues);
            var consumer = new AsyncEventingBasicConsumer(_queueManager.channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.Info($"Received {message}");
                _queueManager.channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                return Task.CompletedTask;
            }; 
            await _queueManager.channel.BasicConsumeAsync(queues[_handlerName], autoAck: false, consumer: consumer);
        }
      
        public void ParseConfig()
        {
            _doc = XDocument.Load(_configPath);
            ReadQueuesConfig();
        }

        private void ReadQueuesConfig()
        {
            var queueElems = _doc.Descendants("Queues").Elements();
            foreach (var key in queueElems)
            {
                queues[key.Name.LocalName] = key.Value.Trim();
            }
        }
    }
}
