using AutoFileDispatcher.Common;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Win32;

namespace FileDispatcher
{
    public class Runner
    {
        private readonly ConfigParser _configParser;
        private readonly List<FileSystemWatcher> _watchers;
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public Runner()
        {
            XmlConfigurator.Configure(new FileInfo("AutoFileDispatcher.FileDispatcher.config"));
            logger.Info("Runner initialized.");
            _configParser = new ConfigParser("C:\\Users\\gauta\\source\\repos\\AutoFileDispatcher\\FileDispatcher\\config.xml");
            _watchers = new List<FileSystemWatcher>();
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        public void Run()
        {
            // Parse configuration including TagDirectories
            logger.Info("Parsing Configuration...");
            _configParser.ParseConfig();
            CreateWatchers();
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
            foreach (var dir in _configParser.directories)
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

            foreach (var dirEvent in dir.events)
            {
                switch (dirEvent.FileEventName)
                {
                    case "FileCreated":
                        watcher.Created += OnCreated;
                        break;
                    case "FileChanged":
                        watcher.Changed += OnChanged;
                        break;
                    case "FileDeleted":
                        watcher.Deleted += OnDeleted;
                        break;
                    case "FileRenamed":
                        watcher.Renamed += OnRenamed;
                        break;
                }
            }
            logger.Info($"Created Watcher for {dir.Path}");
            return watcher;
        }

        public void OnCreated(object s, FileSystemEventArgs e)
        {
            logger.Info($"Created: {e.FullPath}");
        }

        public void OnChanged(object s, FileSystemEventArgs e)
        {
            logger.Info($"Changed: {e.FullPath}");
        }

        public void OnDeleted(object s, FileSystemEventArgs e)
        {
            logger.Info($"Deleted: {e.FullPath}");
        }

        public void OnRenamed(object s, RenamedEventArgs e)
        {
            logger.Info($"Renamed: {e.OldFullPath} → {e.FullPath}");

        }
    }
}
