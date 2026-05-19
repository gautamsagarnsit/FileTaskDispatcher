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
        public string eventName;
        public Dictionary<string, string> eventConfig;
        public List<string> InBoundQueues;
        public string handlerName;
        public EventAction(string name)
        {
            eventName = name;
        }

        public void updateConfig(string key, string value)
        {
            eventConfig[key] = value;
        }
    }
}
