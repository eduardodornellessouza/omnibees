using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using OB.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace OB.BL.Operations.Internal.Behaviors
{
    /// <summary>
    /// Logs/Traces information about the methods being invoked.
    /// When NLog.Trace level is used, all the method parameters are serialized as JSON.
    /// When NLog.Debug level is used, all the method parameters are written using ToString().
    /// </summary>
    public class LoggingInterceptionBehavior : IInterceptionBehavior
    {
        [DebuggerStepThrough()]
        public IMethodReturn Invoke(IMethodInvocation input,
          GetNextInterceptionBehaviorDelegate getNext)
        {
            Stopwatch stopwatch = new Stopwatch();
            ILogger logger = null;
            var target = input.Target as BusinessPOCOBase;
            if (target != null)
            {
                logger = target.Logger;
                if (logger.IsTraceEnabled || logger.IsDebugEnabled)
                {
                    // Before invoking the method on the original target.
                    WriteLog(logger, input,
                      "Invoking method {0} with parameters {1}",
                      input.MethodBase.ToString(), input.Inputs);


                    stopwatch.Start();
                }

                // Before invoking the method on the original target.
                //if (input.MethodBase.Name == "GetTenant")
                //{
                //    var tenantName = input.Arguments["tenant"].ToString();
                //    if (IsInCache(tenantName))
                //    {
                //        return input.CreateMethodReturn(
                //          FetchFromCache(tenantName));
                //    }
                //}
            }
            // Invoke the next behavior in the chain.
            var result = getNext()(input, getNext);

            if (target != null && (logger.IsTraceEnabled || logger.IsDebugEnabled))
            {
                stopwatch.Stop();

                // After invoking the method on the original target.
                if (result.Exception != null)
                {
                    WriteLog(logger, input,
                      "Invoked method {0} threw exception {1}",
                      input.MethodBase.ToString(),
                      result.Exception.Message);
                }
                else
                {
                    WriteLog(logger, input,
                      "Invoked method {0} returned {1} , TOOK: {2} ms",
                      input.MethodBase.ToString(),
                      result.ReturnValue,
                      stopwatch.ElapsedMilliseconds);
                }
            }


            return result;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public bool WillExecute
        {
            get
            {
                return true;
            }
        }

        [DebuggerStepThrough()]
        private void WriteLog(ILogger logger, IMethodInvocation input, string messageFormat, params object[] obj)
        {          
            if (logger.IsTraceEnabled)
            {
                var paramStrs = new List<string>();
                foreach (var o in obj)
                {
                    if (o is IParameterCollection)
                    {
                        var parameterCollection = o as IParameterCollection;
                        object[] parameters = new object[parameterCollection.Count];
                        parameterCollection.CopyTo(parameters, 0);
                        OrderedDictionary dic = new OrderedDictionary(parameterCollection.Count);
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            dic.Add(parameterCollection.ParameterName(i), parameters[i]);
                        }
                        paramStrs.Add(ObjectExtension.ToJSON(dic));
                    }
                    else
                    {
                        paramStrs.Add(ObjectExtension.ToJSON(o));
                    }
                }
                var paramsArray = paramStrs.ToArray();
                string trace = string.Format(messageFormat, paramsArray);

                logger.Trace(trace);
            }
            else if (logger.IsDebugEnabled)
            {
                string debug = string.Format(messageFormat, obj);

                logger.Debug(debug);
            }
        }
    }

    #region Custom Logging

    //class CustomLoggerCallHandler : ICallHandler
    //{
    //    public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
    //    {
    //        Stopwatch stopwatch = new Stopwatch();
    //        ILogger logger = null;
    //        var target = input.Target as BusinessPOCOBase;
    //        if (target != null)
    //        {
    //            logger = target.CustomLogger;
    //            if (logger.IsTraceEnabled || logger.IsDebugEnabled)
    //            {
    //                // Before invoking the method on the original target.
    //                WriteLog(logger, input,
    //                  "Invoking method {0} with parameters {1}",
    //                  input.MethodBase.ToString(), input.Inputs);


    //                stopwatch.Start();
    //            }
    //        }

    //        // Invoke the next behavior in the chain.
    //        var result = getNext()(input, getNext);

    //        if (target != null && (logger.IsTraceEnabled || logger.IsDebugEnabled))
    //        {
    //            stopwatch.Stop();

    //            // After invoking the method on the original target.
    //            if (result.Exception != null)
    //            {
    //                WriteLog(logger, input,
    //                  "Invoked method {0} threw exception {1}",
    //                  input.MethodBase.ToString(),
    //                  result.Exception.Message);
    //            }
    //            else
    //            {
    //                WriteLog(logger, input,
    //                  "Invoked method {0} returned {1} , TOOK: {2} ms",
    //                  input.MethodBase.ToString(),
    //                  result.ReturnValue,
    //                  stopwatch.ElapsedMilliseconds);
    //            }
    //        }


    //        return result;
    //    }

    //    private void WriteLog(ILogger logger, IMethodInvocation input, string messageFormat, params object[] obj)
    //    {
    //        Func<string> logTrace = () =>
    //        {
    //            var paramStrs = new List<string>();
    //            foreach (var o in obj)
    //            {
    //                if (o is IParameterCollection)
    //                {
    //                    var parameterCollection = o as IParameterCollection;
    //                    object[] parameters = new object[parameterCollection.Count];
    //                    parameterCollection.CopyTo(parameters, 0);
    //                    OrderedDictionary dic = new OrderedDictionary(parameterCollection.Count);
    //                    for (int i = 0; i < parameters.Length; i++)
    //                    {
    //                        dic.Add(parameterCollection.ParameterName(i), parameters[i]);
    //                    }
    //                    paramStrs.Add(ObjectExtension.ToJSON(dic));
    //                }
    //                else
    //                {
    //                    paramStrs.Add(ObjectExtension.ToJSON(o));
    //                }
    //            }
    //            var paramsArray = paramStrs.ToArray();
    //            return string.Format(messageFormat, paramsArray);
    //        };

    //        Func<string> logDebug = () =>
    //        {
    //            return string.Format(messageFormat, obj);
    //        };

    //        if (logger.IsTraceEnabled)
    //            logger.Trace(logTrace);
    //        else if (logger.IsDebugEnabled)
    //            logger.Debug(logDebug);
    //    }

    //    public int Order { get; set; }
    //}

    //class CustomLoggerAttribute : HandlerAttribute
    //{
    //    public override ICallHandler CreateHandler(IUnityContainer container)
    //    {
    //        return new CustomLoggerCallHandler();
    //    }
    //}

    #endregion
}
