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
        public List<EventType> events;
    }
}
