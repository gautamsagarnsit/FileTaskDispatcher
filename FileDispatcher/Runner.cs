using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;


namespace FileDispatcher
{
    public class Runner
    {
        private readonly ConfigParser _configParser;
        private readonly List<FileSystemWatcher> _watchers;
        private FileLogger logger;


        public Runner()
        {
            _configParser = new ConfigParser("config.xml");
            _watchers = new List<FileSystemWatcher>();
            logger = new FileLogger();

        }

        public void Run()
        {
            // Parse configuration including TagDirectories
            _configParser.ParseConfig();
            CreateWatchers();            
        }

        private void CreateWatchers()
        {
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
                        watcher.Created += (s, e) => logger.Log($"Created: {e.FullPath}");
                        break;
                    case "Changed":
                        watcher.Changed += (s, e) => logger.Log($"Changed: {e.FullPath}");
                        break;
                    case "FileDeleted":
                        watcher.Deleted += (s, e) => logger.Log($"Deleted: {e.FullPath}");
                        break;
                    case "Renamed":
                        watcher.Renamed += (s, e) => logger.Log($"Renamed: {e.OldFullPath} → {e.FullPath}");
                        break;
                }
            }
            return watcher;
        }

    }
}
