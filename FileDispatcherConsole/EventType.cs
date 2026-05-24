using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileDispatcher.Common;

namespace FileDispatcherConsole
{
    [Serializable]
    public class EventType
    {
        public FileEventType FileEventName;
        public string OutboundQueue;
        public List<EventAction> eventActions;
        public EventType()
        {
            eventActions = new List<EventAction>();
        }
    }
}
