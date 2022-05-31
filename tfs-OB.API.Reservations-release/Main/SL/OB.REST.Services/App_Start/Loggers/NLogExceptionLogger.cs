using OB.Log;
using OB.Log.Messages;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;

namespace OB.REST.Services.Loggers
{
    public class NLogExceptionLogger : ExceptionLogger
    {       
        static readonly ILogger _logger = LogsManager.CreateLogger(typeof(NLogExceptionLogger));
        
        public override async void Log(ExceptionLoggerContext context)
        {
            var logObj = new LogMessageBase
            {
                MethodName = context.Request?.GetRouteData()?.Values["action"] as string
            };

            var requestContent = await LoggerHelper.ReadHttpContent(context.Request?.Content, logObj, _logger);
            logObj.RequestId = LoggerHelper.GetRequestId(requestContent, _logger);

            _logger.Error(context.Exception, logObj);
        }
    }
}