using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDispatcherConsole
{
    [Serializable]
    public class EventType
    {
        public string FileEventName;
        public string OutboundQueue;
        public List<EventAction> eventActions;
        public EventType()
        {
            eventActions = new List<EventAction>();
        }
    }
}
