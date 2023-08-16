using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DronDoomTexUtilsDLL
{
    public class Logger
    {
        // Delegate
        public delegate void LogDelegate(string message);



        // Variables
        private LogDelegate _logger;
        public bool logTime = true;



        // Constructors
        public Logger(LogDelegate logger)
        {
            _logger = logger;
        }



        // Properties
        public bool HasDelegate => _logger != null;



        // Methods
        public void AddDelegate(LogDelegate logger)
        {
            _logger += logger;
        }

        public void ClearDelegates()
        {
            _logger = null;
        }

        public bool Log(string message)
        {
            if (_logger != null)
            {
                if (logTime) _logger.Invoke("[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + message);
                else _logger.Invoke(message);
                return true;
            }
            else
                return false;
        }

        public bool LogFormat(string message, params string[] list)
        {
            if (_logger != null)
            {
                if (logTime) _logger.Invoke(string.Format("[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + message, list));
                else _logger.Invoke(string.Format(message, list));
                return true;
            }
            else
                return false;
        }
    }
}
