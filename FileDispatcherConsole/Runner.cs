using Common;
using log4net;
using log4net.Appender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace FileDispatcherConsole
{
    public class Runner
    {
        private readonly ConfigParser _configParser;
        private readonly List<FileSystemWatcher> _watchers;
        private static readonly ILog logger = LogManager.GetLogger(typeof(Runner));


        public Runner()
        {        
            logger.Info("Runner initialized.");
            var repository = LogManager.GetRepository();
            var appender = repository.GetAppenders()
                             .OfType<FileAppender>()
                             .FirstOrDefault();
            Console.WriteLine(appender?.File);
            _configParser = new ConfigParser("C:\\Users\\gauta\\source\\repos\\AutoFileDispatcher\\FileDispatcherConsole\\config.xml");
            _watchers = new List<FileSystemWatcher>();
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

        private void CreateWatchers()
        {
            logger.Info("Create File Event Listeners...");
            foreach(var dir in _configParser.directories)
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

            foreach(var dirEvent in dir.events)
            {
                switch(dirEvent.FileEventName)
                {
                    case "FileCreated":
                        watcher.Created += (s, e) => logger.Info($"Created: {e.FullPath}");
                        break;
                    case "Changed":
                        watcher.Changed += (s, e) => logger.Info($"Changed: {e.FullPath}");
                        break;
                    case "FileDeleted":
                        watcher.Deleted += (s, e) => logger.Info($"Deleted: {e.FullPath}");
                        break;
                    case "Renamed":
                        watcher.Renamed += (s, e) => logger.Info($"Renamed: {e.OldFullPath} → {e.FullPath}");
                        break;
                }
            }
            logger.Info($"Created Watcher for {dir.Path}");
            return watcher;
        }

    }
}
