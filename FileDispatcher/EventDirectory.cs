using AutoFileDispatcher.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDispatcher
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
