using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Common
{
    public class LogManager
    {        
        public static ILog GetLogger(Type type)
        {
            return log4net.LogManager.GetLogger(type);
        }
    }
}
