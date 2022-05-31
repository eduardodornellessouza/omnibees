using OB.BL.Contracts.Responses;
using OB.BL.Operations.Exceptions;
using OB.DL.Common.Interfaces;
using OB.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OB.BL.Operations.Helper
{
    public static class Retry
    {
        static readonly ILogger logger = LogsManager.CreateLogger(typeof(Retry));

        #region Retry for Exceptions

        public static void Execute(Action action, TimeSpan retryInterval, int retryCount = 3, string logKey = null,
            bool handleDataLayerUpdateException = false, bool repeatOnBusinessLayerException = false)
        {
            InnerExecute<object>(() => action, new RetryOptions
            {
                RetryCount = retryCount,
                RetryInterval = () => retryInterval,
                LogKey = logKey,
                HandleDataLayerUpdateException = handleDataLayerUpdateException,
                RepeatOnBusinessLayerException = repeatOnBusinessLayerException
            });
        }

        public static void Execute(Action action, Func<TimeSpan> retryInterval, int retryCount = 3, string logKey = null,
            bool handleDataLayerUpdateException = false, bool repeatOnBusinessLayerException = false)
        {
            InnerExecute<object>(() => action, new RetryOptions
            {
                RetryCount = retryCount,
                RetryInterval = retryInterval,
                LogKey = logKey,
                HandleDataLayerUpdateException = handleDataLayerUpdateException,
                RepeatOnBusinessLayerException = repeatOnBusinessLayerException
            });
        }

        public static T Execute<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3, string logKey = null,
            bool handleDataLayerUpdateException = false, bool repeatOnBusinessLayerException = false)
        {
            return InnerExecute<T>(action, new RetryOptions
            {
                RetryCount = retryCount,
                RetryInterval = () => retryInterval,
                LogKey = logKey,
                HandleDataLayerUpdateException = handleDataLayerUpdateException,
                RepeatOnBusinessLayerException = repeatOnBusinessLayerException
            });
        }

        public static T Execute<T>(Func<T> action, Func<TimeSpan> retryInterval, int retryCount = 3, string logKey = null,
            bool handleDataLayerUpdateException = false, bool repeatOnBusinessLayerException = false)
        {
            return InnerExecute<T>(action, new RetryOptions
            {
                RetryCount = retryCount,
                RetryInterval = retryInterval,
                LogKey = logKey,
                HandleDataLayerUpdateException = handleDataLayerUpdateException,
                RepeatOnBusinessLayerException = repeatOnBusinessLayerException
            });
        }

        static T InnerExecute<T>(Func<T> action, RetryOptions options)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < options.RetryCount; retry++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    bool isNotGoingToTryAgain = (retry + 1) == options.RetryCount
                                                || (!options.HandleDataLayerUpdateException && ex.IsDataLayerUpdateException())
                                                || (!options.RepeatOnBusinessLayerException && ex is BusinessLayerException);

                    var retryIn = options.RetryInterval.Invoke();
                    logger.Error(ex, options.LogKey + ": Attempt #" + (retry + 1) + (isNotGoingToTryAgain ? ". Given up..." : " - Retrying in " + retryIn.TotalMilliseconds + " ms"));

                    if (isNotGoingToTryAgain)
                        throw;

                    exceptions.Add(ex);
                    Thread.Sleep(retryIn);
                }
            }

            throw new AggregateException(exceptions);
        }

        #endregion Retry for Exceptions

        #region Retry for Excepetions or Failed Response

        public static T Execute<T>(Func<T> action, RetryOptions options)
        {
            return InnerExecuteFailedResponse(action, options);
        }

        static T InnerExecuteFailedResponse<T>(Func<T> action, RetryOptions options)
        {
            T result = default(T);

            bool success = false;
            var failStatuses = new HashSet<Status> { Status.Fail };
            if (options.RepeatOnPartialSuccess)
                failStatuses.Add(Status.PartialSuccess);

            var exceptions = new List<Exception>();
            for (int retry = 0; retry <= options.RetryCount; retry++)
            {
                Exception exception = null;
                try
                {
                    result = action();

                    var response = result as ResponseBase;
                    success = (response == null) || !failStatuses.Contains(response.Status);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    exceptions.Add(exception);
                    success = false;
                }
                finally
                {
                    if (!success)
                    {
                        bool isNotGoingToTryAgain = exception != null && ((!options.HandleDataLayerUpdateException && exception.IsDataLayerUpdateException())
                                                    || (!options.RepeatOnBusinessLayerException && exception is BusinessLayerException));

                        var retryIn = options.RetryInterval.Invoke();

                        if (exception != null)
                            logger.Log(options.LogLevel, exception, null, options.LogKey + ": Attempt #" + (retry + 1) + (isNotGoingToTryAgain ? ". Given up..." : " - Retrying in " + retryIn.TotalMilliseconds + " ms"));
                        else
                            logger.Log(options.LogLevel, options.LogKey + ": Attempt #" + (retry + 1) + (isNotGoingToTryAgain ? ". Given up..." : " - Retrying in " + retryIn.TotalMilliseconds + " ms"));

                        if (isNotGoingToTryAgain)
                            throw exception;
                        
                        Thread.Sleep(retryIn);
                    }
                }

                if (success)
                    break;
            }

            if (exceptions.Any())
                throw new AggregateException(exceptions);

            return result;
        }

        #endregion Retry for Excepetions or Failed Response
    }

    public class RetryOptions
    {
        public RetryOptions()
        {
            LogLevel = LogLevel.Error;
            RetryCount = 3;
        }

        LogLevel _logLevel;
        public LogLevel LogLevel
        {
            get => _logLevel;
            set { _logLevel = (value != LogLevel.Fatal) ? value : LogLevel.Error; }
        }
        public string LogKey { get; set; }
        public bool RepeatOnFailedResponse { get; set; }
        public Func<TimeSpan> RetryInterval { get; set; }
        public int RetryCount { get; set; }
        public bool HandleDataLayerUpdateException { get; set; }
        public bool RepeatOnBusinessLayerException { get; set; }
        public bool RepeatOnPartialSuccess { get; set; }
    }
}