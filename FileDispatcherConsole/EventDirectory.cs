using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDispatcherConsole
{
    [Serializable]
    public class EventDirectory
    {
        public string Path;
        public List<EventType> events;

        public EventDirectory()
        {
            events = new List<EventType>();
        }
    }
}
