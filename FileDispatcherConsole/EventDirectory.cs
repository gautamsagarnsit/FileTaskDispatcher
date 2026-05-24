using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileDispatcher.Common;

namespace FileDispatcherConsole
{
    [Serializable]
    public class EventDirectory
    {
        public string Path;
        public Dictionary<FileEventType, EventType> fileEventType;

        public EventDirectory()
        {
            fileEventType = new Dictionary<FileEventType, EventType>();
        }
    }
}
