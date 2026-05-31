using AutoFileDispatcher.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDispatcher
{
    [Serializable]
    public class EventAction
    {
        public FileEventAction eventName;
        public Dictionary<string, string> eventConfig;
        public List<string> InBoundQueues;
        public string handlerName;
        public string filePath;
        public EventAction(FileEventAction name)
        {
            eventName = name;
            eventConfig = new Dictionary<string, string>();
            InBoundQueues = new List<string>();
        }

        public void updateConfig(string key, string value)
        {
            eventConfig[key] = value;
        }
    }
}
