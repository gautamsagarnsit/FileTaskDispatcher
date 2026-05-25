using AutoFileDispatcher.Common;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Common
{
    public class ConfigParser
    {
        private readonly string _configPath;
        private XDocument _doc;
        public Dictionary<string, string> queues;
        private static readonly ILog logger = LogManager.GetLogger(typeof(ConfigParser));

        public ConfigParser(string configPath = "config.xml")
        {
            _configPath = configPath;
            queues = new Dictionary<string, string>();
        }
        public void ParseConfig()
        {
            _doc = XDocument.Load(_configPath);
            //ReadDefaultEventActionsConfig();
            ReadQueuesConfig();
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
