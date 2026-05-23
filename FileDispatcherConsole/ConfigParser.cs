using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FileDispatcherConsole
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
            directories = new List<EventDirectory>();
        }
        public void ParseConfig()
        {
            _doc = XDocument.Load(_configPath);
            ReadDefaultEventActionsConfig();
            ReadDirectoriesConfig();
        }

        public void ReadDefaultEventActionsConfig()
        {
            var eventActionsElem = _doc.Descendants("DefaultActionsConfig").Descendants("Action");
            foreach (var eventActionElem in eventActionsElem)
            {
                EventAction action = createEventAction(eventActionElem);           
                eventActions[action.eventName] = action;
            }
        }

        public void ReadDirectoriesConfig()
        {
            var dirElems = _doc.Descendants("Directories").Descendants("Directory");
            foreach(var dirElem in dirElems)
            {
                EventDirectory dir = new EventDirectory();
                dir.Path = dirElem.Attribute("Path").Value;
                var eventsElems = dirElem.Element("Events");
                ReadEventTypesConfig(dir, eventsElems);
                directories.Add(dir);
            }
        }

        public void ReadEventTypesConfig(EventDirectory dir, XElement eventsElem)
        {
            var eventTypesElem = eventsElem.Descendants("Event");
            foreach (var eventElem in eventTypesElem)
            {
                dir.events.Add(createEventType(eventElem));
            }          
        }
        private EventType createEventType(XElement eventElem)
        {
            EventType eventType = new EventType();
            eventType.FileEventName = eventElem.Attribute("Type").Value;
            eventType.OutboundQueue = eventElem.Element("QueueName").Value;
            var eventActionsElem = eventElem.Descendants("Actions").Descendants("Action");
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
                action.updateConfig(key.Name.LocalName, key.Value);
            }
            return action;
        }
        private EventAction createEventAction(XElement eventActionElem)
        {
            EventAction action = new EventAction(eventActionElem.Attribute("Name").Value.Trim());
            var InboundQueuesElem = eventActionElem.Descendants("InboundQueues");
            foreach (var queue in InboundQueuesElem.Descendants("QueueName"))
            {
                action.InBoundQueues.Add(queue.Value.Trim());
            }
            var ConfigElem = eventActionElem.Descendants("Config").Elements();
            foreach (var key in ConfigElem)
            {
                action.updateConfig(key.Name.LocalName, key.Value.Trim());
            }
            action.handlerName = eventActionElem.Descendants("Handler").FirstOrDefault().Value.Trim();
            return action;
        }


    }
}
