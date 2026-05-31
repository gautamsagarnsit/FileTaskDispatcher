using AutoFileDispatcher.Common;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace FileDispatcher
{
    public class ConfigParser
    {
        private readonly string _configPath;
        private XDocument _doc;
        public Dictionary<FileEventAction, EventAction> eventActions;
        public Dictionary<string, string> queues;
        public Dictionary<string, EventDirectory> directories;
        private static readonly ILog logger = LogManager.GetLogger(typeof(ConfigParser));

        public ConfigParser(string configPath = "config.xml")
        {
            _configPath = configPath;
            eventActions = new Dictionary<FileEventAction, EventAction>();
            queues = new Dictionary<string, string>();
            directories = new Dictionary<string, EventDirectory>();
        }
        public void ParseConfig()
        {
            _doc = XDocument.Load(_configPath);
            ReadQueuesConfig();
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
            foreach (var dirElem in dirElems)
            {
                EventDirectory dir = new EventDirectory();
                dir.Path = dirElem.Attribute("Path").Value;
                var eventsElems = dirElem.Element("Events");
                ReadEventTypesConfig(dir, eventsElems);
                directories[dir.Path] = dir;
            }
        }

        public void ReadEventTypesConfig(EventDirectory dir, XElement eventsElem)
        {
            var eventTypesElem = eventsElem.Descendants("Event");
            foreach (var eventElem in eventTypesElem)
            {
                EventType fileEvent = createEventType(eventElem);
                dir.fileEventType[fileEvent.FileEventName] = fileEvent;
            }
        }
        private EventType createEventType(XElement eventElem)
        {
            EventType eventType = new EventType();
            Enum.TryParse(eventElem.Attribute("Type").Value, out FileEventType eventTypeName);
            eventType.FileEventName = eventTypeName;
            eventType.OutboundQueue = eventElem.Element("QueueName").Value;
            var eventActionsElem = eventElem.Descendants("Actions").Descendants("Action");
            logger.Info($"Creating Event Actions for the File Event Type: {eventTypeName}");
            foreach (var eventActionElem in eventActionsElem)
            {
                eventType.eventActions.Add(createEventAction(eventActionElem));
            }
            return eventType;
        }

        private EventAction createEventAction(XElement eventActionElem)
        {
            Enum.TryParse(eventActionElem.Attribute("Name").Value, out FileEventAction eventActionName);
            EventAction action = new EventAction(eventActionName);
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
            logger.Info($"Created Action: {JsonConvert.SerializeObject(action)}");
            return action;
        }

        private void ReadQueuesConfig()
        {
            var queueElems = _doc.Descendants("Queues").Elements();
            foreach (var key in queueElems)
            {
                queues[key.Name.LocalName] = key.Value.Trim();
            }
        }

    }
}
