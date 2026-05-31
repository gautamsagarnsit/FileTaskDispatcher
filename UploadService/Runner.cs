using AutoFileDispatcher.Common;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UploadService
{
    public class Runner
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Runner));
        private QueueManager _queueManager;
        private readonly string _handlerName = "UploadService";
        private XDocument _doc;
        private readonly string _configPath = "C:\\Users\\gauta\\source\\repos\\AutoFileDispatcher\\UploadService\\config.xml";
        private Dictionary<string, string> queues;
        private IConfigurationRoot _driveConfig;
        private UploadHandler _uploadHandler;
        public Runner(UploadHandler uploadHandler) 
        {
            _logger.Info("UploadService Runner initialized.");
            _uploadHandler = uploadHandler;
            _queueManager = new QueueManager();
            queues = new Dictionary<string, string>();
            ParseConfig();
        }

        public async Task Run()
        {
            _logger.Info("Upload Service Running...");
            await _queueManager.GetChannel(queues);
            await StartConsumer();
            return;
        }

        public async Task StartConsumer()
        {
            var consumer = new AsyncEventingBasicConsumer(_queueManager.channel);

            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.Info($"Received {message}");
                var doc = JsonDocument.Parse(message);
                
                if (doc.RootElement.GetProperty("eventName").GetInt32() == 1)
                {
                    string filePath = doc.RootElement.GetProperty("filePath").GetString();
                    _uploadHandler.Upload(filePath);
                }
                _queueManager.channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                return Task.CompletedTask;
            };
            _logger.Info("Starting queue consumer...");
            await _queueManager.channel.BasicConsumeAsync(queues[_handlerName], autoAck: false, consumer: consumer);
            _logger.Info("Queue consumer started. Waiting for messages...");
            return;
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
