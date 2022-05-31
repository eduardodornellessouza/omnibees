using OB.Log;
using OB.Log.Messages;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace OB.REST.Services.Loggers
{
    public class NLogControllerTraceHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //var uri = request.RequestUri.PathAndQuery;

            var routeData = request.GetRouteData();
            var controller = routeData.Values["controller"] as string;
            var action = routeData.Values["action"] as string;
            var stopwatch = new Stopwatch();

            var logger = LogsManager.CreateLogger("OB.REST.Services.Controllers." + controller + "Controller" + (string.IsNullOrEmpty(action) ? string.Empty : "." + action));

            var principal = request.GetRequestContext()?.Principal;
            var principalName = principal != null && principal.Identity.IsAuthenticated ? principal.Identity.Name : "Anonymous";

            var logMsgBefore = new LogMessageBase
            {
                ControllerName = controller,
                MethodName = action,
                ClientHostName = HttpContext.Current?.Request?.UserHostAddress
            };

            if (request.Headers.TryGetValues("APP_ID", out var headerValues))
            {
                logMsgBefore.ClientIdentifier = headerValues?.FirstOrDefault();
            }

            var otherLogInfoBefore = new LogEventPropertiesBase
            {
                UserName = principalName
            };

            if (logger.IsTraceEnabled || logger.IsDebugEnabled)
            {
                var requestContent = await LoggerHelper.ReadHttpContent(request.Content, logMsgBefore, logger);
                logMsgBefore.RequestId = LoggerHelper.GetRequestId(requestContent, logger);
                otherLogInfoBefore.Request = requestContent;
            }

            //let other handlers process the request
            stopwatch.Start();
            // NOTE: Before C# 6.0, you can't use await in finally.
            //       When we catch up, change this code.
            return await base.SendAsync(request, cancellationToken)
                .ContinueWith(async task =>
                {
                    stopwatch.Stop();
                    logMsgBefore.TimeTaken = stopwatch.Elapsed;
                    var response = await task;

                    //once response is ready, log it
                    if (logger.IsTraceEnabled)
                    {
                        otherLogInfoBefore.Response = await LoggerHelper.ReadHttpContent(response.Content, logMsgBefore, logger);
                        logger.Trace(logMsgBefore, otherLogInfoBefore);
                    }
                    else if (logger.IsDebugEnabled)
                    {
                        logger.Debug(logMsgBefore, otherLogInfoBefore);
                    }
                    else
                        logger.Info(logMsgBefore, otherLogInfoBefore);

                    response.Headers.ConnectionClose = true;
                    return response;
                }, cancellationToken, TaskContinuationOptions.DenyChildAttach, TaskScheduler.Current)
                .Unwrap();
        }
    }
}
