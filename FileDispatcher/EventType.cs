using AutoFileDispatcher.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDispatcher
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
