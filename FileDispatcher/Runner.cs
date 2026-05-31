using AutoFileDispatcher.Common;
using log4net;
using log4net.Config;
using Microsoft.Win32;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FileDispatcher
{
    public class Runner
    {
        private readonly ConfigParser _configParser;
        private readonly List<FileSystemWatcher> _watchers;
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private QueueManager _queueManager;

        public Runner()
        {
            XmlConfigurator.Configure();
            logger.Info("Runner initialized.");
            logger.Info("Config File Location: \"AutoFileDispatcher\\\\FileDispatcher\\\\config.xml\"");
            _configParser = new ConfigParser("C:\\Users\\gauta\\source\\repos\\AutoFileDispatcher\\FileDispatcher\\config.xml");
            _watchers = new List<FileSystemWatcher>();
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            _queueManager = new QueueManager();
        }

        public void Run()
        {
            // Parse configuration including TagDirectories
            logger.Info("Parsing Configuration...");
            _configParser.ParseConfig();
            CreateWatchers();
            _queueManager.GetChannel(_configParser.queues);
            Console.WriteLine("Enter to exit...");
            Console.ReadLine();
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                logger.Info("System resumed. Recreating watchers...");
                CreateWatchers();
            }
        }

        private void CreateWatchers()
        {
            logger.Info("Create File Event Listeners...");
            foreach (var dir in _configParser.directories.Values)
            {
                _watchers.Add(GetFileEventListener(dir));
            }
        }
        private FileSystemWatcher GetFileEventListener(EventDirectory dir)
        {
            var watcher = new FileSystemWatcher(dir.Path);
            watcher.NotifyFilter = NotifyFilters.Attributes
                                    | NotifyFilters.CreationTime
                                    | NotifyFilters.DirectoryName
                                    | NotifyFilters.FileName
                                    | NotifyFilters.LastAccess
                                    | NotifyFilters.LastWrite
                                    | NotifyFilters.Security
                                    | NotifyFilters.Size;

            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            foreach (var dirEvent in dir.fileEventType.Keys)
            {
                switch (dirEvent)
                {
                    case FileEventType.FileCreated:
                        watcher.Created += OnCreated;
                        break;
                    case FileEventType.FileChanged:
                        watcher.Changed += OnChanged;
                        break;
                    case FileEventType.FileDeleted:
                        watcher.Deleted += OnDeleted;
                        break;
                    case FileEventType.FileRenamed:
                        watcher.Renamed += OnRenamed;
                        break;
                }
            }
            logger.Info($"Created Watcher for {dir.Path}");
            return watcher;
        }

        public void OnCreated(object s, FileSystemEventArgs e)
        {
            string message = $"File created: {e.FullPath} at {DateTime.Now}";
            logger.Info(message);
            publishEvent(FileEventType.FileCreated, e.FullPath);
        }

        public void OnChanged(object s, FileSystemEventArgs e)
        {
            string message = $"Changed: {e.FullPath}";
            logger.Info(message);
            //publishEvent(FileEventType.FileChanged, e.FullPath);
        }

        public void OnDeleted(object s, FileSystemEventArgs e)
        {
            string message = $"Deleted: {e.FullPath}";
            logger.Info(message);
            publishEvent(FileEventType.FileDeleted, e.FullPath);
        }

        public void OnRenamed(object s, RenamedEventArgs e)
        {
            string message = $"Renamed: {e.OldFullPath} → {e.FullPath}";
            logger.Info(message);
            publishEvent(FileEventType.FileRenamed, e.FullPath);
        }

        public void publishEvent(FileEventType eventType, string filePath)
        {
            EventDirectory dir = _configParser.directories[Path.GetDirectoryName(filePath)];
            EventType fileEventType = dir.fileEventType[eventType];
            foreach (var action in fileEventType.eventActions)
            {
                logger.Info("Available Event Action: " + action.eventName);
                action.filePath = filePath;
                publishToQueue(action);
            }
        }

        public async void publishToQueue(EventAction action)
        {
            foreach (var queueName in action.InBoundQueues)
            {
                if (_queueManager.channel.IsOpen)
                {                    
                    var message = JsonConvert.SerializeObject(action);
                    logger.Info("Publishing to Queue " + message);
                    var body = Encoding.UTF8.GetBytes(message);
                    await _queueManager.channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
                    logger.Info($"Published message to queue {queueName}");
                }
                else
                {
                    logger.Error($"Cannot publish message to queue {queueName} because channel is not open.");
                }
            }

        }
    }
}
