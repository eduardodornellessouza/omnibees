using Newtonsoft.Json.Serialization;
using OB.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace OB.REST.Services.Loggers
{
    public class NLogJsonTraceWriter : ITraceWriter
    {
        private readonly ILogger _logger;

        public NLogJsonTraceWriter(string logName)
        {
            _logger = LogsManager.CreateLogger(logName);
        }

        public TraceLevel LevelFilter
        {
            //Check NLog configuration to activate or deactivate logging.
            get
            {
                if (_logger.IsTraceEnabled || _logger.IsDebugEnabled)
                    return TraceLevel.Verbose;
                if (_logger.IsErrorEnabled || _logger.IsFatalEnabled || _logger.IsWarnEnabled)
                    return TraceLevel.Error;
                if (_logger.IsInfoEnabled)
                    return TraceLevel.Info;
                return TraceLevel.Off;
            }
        }

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            if (level == TraceLevel.Off)
                return;

            // log Json.NET message to NLog         
            if (_logger.IsTraceEnabled && level == TraceLevel.Verbose)
            {
                if (ex == null)
                    _logger.Trace(message);
                else _logger.Error(ex, message);
            }
            else if (_logger.IsDebugEnabled && level == TraceLevel.Verbose)
            {
                if (ex == null)
                    _logger.Debug(message);
                else _logger.Debug(ex, new List<KeyValuePair<string, object>>(), message);
            }
            else if (_logger.IsWarnEnabled && level == TraceLevel.Warning)
            {
                if (ex == null)
                    _logger.Warn(message);
                else _logger.Warn(ex, message);
            }
            else if (_logger.IsFatalEnabled && level == TraceLevel.Error)
            {
                if (ex == null)
                    _logger.Fatal(message);
                else _logger.Fatal(ex, message);
            }
            else if (_logger.IsErrorEnabled && level == TraceLevel.Error)
            {
                if (ex == null)
                    _logger.Error(message);
                else _logger.Error(ex, message);
            }
            else if (_logger.IsInfoEnabled && level == TraceLevel.Info)
            {
                if (ex == null)
                    _logger.Info(message);
                else _logger.Info(message + " ExceptionCause: " + ex.Message);
            }

        }

    }
}