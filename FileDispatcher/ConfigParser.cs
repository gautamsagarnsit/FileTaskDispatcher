using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FileDispatcher
{ 
    public class ConfigParser
    {
        private readonly string _configPath;
        private XDocument _doc;
        public Dictionary<string, EventAction> eventActions;
        public List<EventDirectory> directories;

        public ConfigParser(string configPath = "config.xml")
        {
            _configPath = configPath;
            eventActions = new Dictionary<string, EventAction>();
        }
        public void ParseConfig()
        {
            _doc = XDocument.Load(_configPath);
            ReadDefaultEventActionsConfig();
            ReadDirectoriesConfig();
        }

        public void ReadDefaultEventActionsConfig()
        {
            var eventActionsElem = _doc.Descendants("Actions");
            foreach (var eventActionElem in eventActionsElem)
            {
                EventAction action = createEventAction(eventActionElem);           
                eventActions[action.eventName] = action;
            }
        }

        public void ReadDirectoriesConfig()
        {
            var dirElems = _doc.Descendants("Directories");
            foreach(var dirElem in dirElems)
            {
                EventDirectory dir = new EventDirectory();
                dir.Path = dirElem.Attribute("Path").Value;
                var eventsElems = dirElem.Descendants("Events").FirstOrDefault();
                dir.events = ReadEventTypesConfig(eventsElems);
                directories.Add(dir);
            }
        }

        public List<EventType> ReadEventTypesConfig(XElement eventsElem)
        {
            var eventTypesElem = eventsElem.Descendants("Event");
            List<EventType> eventTypes = new List<EventType>();
            foreach (var eventElem in eventTypesElem)
            {
                eventTypes.Add(createEventType(eventElem));
            }
            return eventTypes;
            
        }
        private EventType createEventType(XElement eventElem)
        {
            EventType eventType = new EventType();
            eventType.FileEventName = eventElem.Attribute("Type").Value;
            eventType.OutboundQueue = eventElem.Descendants("QueueName").FirstOrDefault().Value;
            var eventActionsElem = eventElem.Descendants("Actions");
            foreach(var eventActionElem in eventActionsElem)
            {
                EventAction action = eventActions[eventActionElem.Attribute("Name").Value];
                eventType.eventActions.Add(updateEventAction(action, eventActionElem));
            }
            return eventType;
        }
        private EventAction updateEventAction(EventAction action, XElement eventActionElem)
        {        
            var ConfigElem = eventActionElem.Descendants("Config");
            foreach (var key in ConfigElem)
            {
                action.updateConfig(key.Value, ConfigElem.Descendants(key.Value).FirstOrDefault().Value);
            }
            return action;
        }
        private EventAction createEventAction(XElement eventActionElem)
        {
            EventAction action = new EventAction(eventActionElem.Attribute("Name").Value);
            var InboundQueuesElem = eventActionElem.Descendants("InboundQueues");
            foreach (var queue in InboundQueuesElem.Descendants("QueueName"))
            {
                action.InBoundQueues.Add(queue.Value);
            }
            var ConfigElem = eventActionElem.Descendants("Config");
            foreach (var key in ConfigElem)
            {
                action.updateConfig(key.Value, ConfigElem.Descendants(key.Value).FirstOrDefault().Value);
            }
            action.handlerName = eventActionElem.Descendants("Handler").FirstOrDefault().Value;
            return action;
        }


    }
}
