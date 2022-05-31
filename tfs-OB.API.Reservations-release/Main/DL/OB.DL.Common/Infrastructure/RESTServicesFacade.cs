using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using OB.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PO.BL.Contracts.Responses;

namespace OB.DL.Common.Infrastructure
{
    public static class RESTServicesFacade
    {
        private static readonly ILogger Logger = LogsManager.CreateLogger("OBRESTServicesFacade");

        public class RestWebClient : WebClient
        {
            //protected override WebRequest GetWebRequest(Uri uri)
            //{
            //    WebRequest w = base.GetWebRequest(uri);
            //    w.Timeout = 19 * 60 * 1000; // 19 Minutos Timeout
            //    return w;
            //}
        }

        private static bool _initialized = false;
        private static int NumberCallRetries = 3;
        private static int CallRetrySleeptimeMs = 1500;



        private static T Call<T, TRequest>(string endpoint, TRequest request, string restService, string operation)
        {
            if (System.Web.HttpContext.Current?.Request?.Headers["Content-Type"] == "application/json")
                return CallJson<T, TRequest>(endpoint, request, restService, operation);

            return CallBson<T, TRequest>(endpoint, request, restService, operation);
        }

        private static T CallJson<T, TRequest>(string endpoint, TRequest request, string restService, string operation)
        {
            try
            {
                var serializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };

                var jsonToUpload = JsonConvert.SerializeObject(request, serializerSettings);

                if (!_initialized)
                {
                    _initialized = true;
                    var servicePoint = ServicePointManager.FindServicePoint(new Uri(endpoint));
                    servicePoint.ConnectionLimit = 30;
                    servicePoint.SetTcpKeepAlive(false, 1200000, 800);
                    servicePoint.UseNagleAlgorithm = false;
                    servicePoint.Expect100Continue = false;
                }

                using (var client = new RestWebClient())
                {
                    for (int i = 1; i <= NumberCallRetries; i++)
                    {
                        try
                        {

                            // BSON serialization
                            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                            client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                            client.Headers.Add("APP_ID", Configuration.AppName);
                            client.Encoding = System.Text.Encoding.UTF8;
                            string jsonResult = client.UploadString(endpoint + "/api/" + restService + "/" + operation, "POST", jsonToUpload);
                            return JsonConvert.DeserializeObject<T>(jsonResult, serializerSettings);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            if (i == NumberCallRetries)
                                throw;
                            Thread.Sleep(CallRetrySleeptimeMs);
                        }
                    }
                }
                throw new InvalidOperationException("Request not made with success!");
            }
            catch (ThreadAbortException taex)
            {
                Logger.Error(taex, "Call Method");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Resquest With Error: Endpoint:{0} / Service:{1} / Operation:{2}", endpoint, restService, operation);
                throw;
            }
        }

        private static T CallBson<T, TRequest>(string endpoint, TRequest request, string restService, string operation)
        {
            try
            {
                var serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };

                var ms = new MemoryStream();
                var writer = new BsonWriter(ms);
                serializer.Serialize(writer, request);

                if (!_initialized)
                {
                    _initialized = true;
                    var servicePoint = ServicePointManager.FindServicePoint(new Uri(endpoint));
                    servicePoint.ConnectionLimit = 30;
                    servicePoint.SetTcpKeepAlive(false, 1200000, 800);
                    servicePoint.UseNagleAlgorithm = false;
                    servicePoint.Expect100Continue = false;
                }

                using (var client = new RestWebClient())
                {
                    for (int i = 1; i <= NumberCallRetries; i++)
                    {
                        try
                        {

                            // BSON serialization
                            client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");
                            client.Headers.Add(HttpRequestHeader.Accept, "application/bson");
                            client.Headers.Add("APP_ID", Configuration.AppName);
                            byte[] binaryResult = client.UploadData(endpoint + "/api/" + restService + "/" + operation,
                                "POST", ms.ToArray());
                            var reader = new BsonReader(
                                new BinaryReader(new MemoryStream(binaryResult), Encoding.UTF8), false,
                                DateTimeKind.Unspecified);
                            return serializer.Deserialize<T>(reader);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            if (i == NumberCallRetries)
                                throw;
                            Thread.Sleep(CallRetrySleeptimeMs);
                        }
                    }
                }
                throw new InvalidOperationException("Request not made with success!");
            }
            catch (ThreadAbortException taex)
            {
                Logger.Error(taex, "Call Method");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Resquest With Error: Endpoint:{0} / Service:{1} / Operation:{2}", endpoint, restService, operation);
                throw;
            }
        }

        private static async Task<T> CallAsync<T, TRequest>(string endpoint, TRequest request, string restService, string operation)
        {
            if (System.Web.HttpContext.Current?.Request?.Headers["Content-Type"] == "application/json")
                return await CallJsonAsync<T, TRequest>(endpoint, request, restService, operation);

            return await CallBsonAsync<T, TRequest>(endpoint, request, restService, operation);
        }

        private async static Task<T> CallJsonAsync<T, TRequest>(string endpoint, TRequest request, string restService, string operation)
        {
            try
            {
                var serializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };

                var jsonToUpload = JsonConvert.SerializeObject(request, serializerSettings);

                if (!_initialized)
                {
                    _initialized = true;
                    var servicePoint = ServicePointManager.FindServicePoint(new Uri(endpoint));
                    servicePoint.ConnectionLimit = 30;
                    servicePoint.SetTcpKeepAlive(false, 1200000, 800);
                    servicePoint.UseNagleAlgorithm = false;
                    servicePoint.Expect100Continue = false;
                }

                using (var client = new RestWebClient())
                {
                    for (int i = 1; i <= NumberCallRetries; i++)
                    {
                        try
                        {

                            // BSON serialization
                            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                            client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                            client.Encoding = System.Text.Encoding.UTF8;
                            client.Headers.Add("APP_ID", Configuration.AppName);
                            string jsonResult = await client.UploadStringTaskAsync(new Uri(endpoint + "/api/" + restService + "/" + operation), "POST", jsonToUpload);
                            return JsonConvert.DeserializeObject<T>(jsonResult, serializerSettings);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            if (i == NumberCallRetries)
                                throw;
                            Thread.Sleep(CallRetrySleeptimeMs);
                        }
                    }
                }
                throw new InvalidOperationException("Request not made with success!");
            }
            catch (ThreadAbortException taex)
            {
                Logger.Error(taex, "Call Method");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Resquest With Error: Endpoint:{0} / Service:{1} / Operation:{2}", endpoint, restService, operation);
                throw;
            }
        }

        private async static Task<T> CallBsonAsync<T, TRequest>(string endpoint, TRequest request, string restService, string operation)
        {
            try
            {
                var serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };

                var ms = new MemoryStream();
                var writer = new BsonWriter(ms);
                serializer.Serialize(writer, request);

                if (!_initialized)
                {
                    _initialized = true;
                    var servicePoint = ServicePointManager.FindServicePoint(new Uri(endpoint));
                    servicePoint.ConnectionLimit = 30;
                    servicePoint.SetTcpKeepAlive(false, 1200000, 800);
                    servicePoint.UseNagleAlgorithm = false;
                    servicePoint.Expect100Continue = false;
                }
                using (var client = new RestWebClient())
                {
                    for (int i = 1; i <= NumberCallRetries; i++)
                    {
                        try
                        {
                            // BSON serialization
                            client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");
                            client.Headers.Add(HttpRequestHeader.Accept, "application/bson");
                            byte[] binaryResult = await client.UploadDataTaskAsync(endpoint + "/api/" + restService + "/" + operation,
                                "POST", ms.ToArray());
                            var reader = new BsonReader(
                                new BinaryReader(new MemoryStream(binaryResult), Encoding.UTF8), false,
                                DateTimeKind.Unspecified);
                            return serializer.Deserialize<T>(reader);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            if (i == NumberCallRetries)
                                throw;
                            Thread.Sleep(CallRetrySleeptimeMs);
                        }
                    }
                }
                throw new InvalidOperationException("Request not made with success!");
            }
            catch (ThreadAbortException taex)
            {
                Logger.Error(taex, "Call Method");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Resquest With Error: Endpoint:{0} / Service:{1} / Operation:{2}", endpoint, restService, operation);
                throw;

            }
        }

        #region OB CALL

        /// <summary>
        /// Calls the po rest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        public static async Task<T> CallAsync<T>(BL.Contracts.Requests.RequestBase request, string restService, string operation)
        {
            string endpoint = Configuration.OBRestServiceEndpoint;
            return await CallAsync<T, BL.Contracts.Requests.RequestBase>(endpoint, request, restService, operation);   
        }

        /// <summary>
        /// Calls the po rest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        public static T Call<T>(BL.Contracts.Requests.RequestBase request, string restService, string operation) where T : BL.Contracts.Responses.ResponseBase
        {
            string endpoint = Configuration.OBRestServiceEndpoint;
            return Call<T, BL.Contracts.Requests.RequestBase>(endpoint, request, restService, operation);
        }

        #endregion OB CALL

        #region PO CALL
        /// <summary>
        /// Calls the po rest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        public static async Task<T> CallAsync<T>(PO.BL.Contracts.Requests.RequestBase request, string restService, string operation)
        {
            string endpoint = Configuration.PORestServiceEndpoint;
            return await CallAsync<T, PO.BL.Contracts.Requests.RequestBase>(endpoint, request, restService, operation);   
        }

        /// <summary>
        /// Calls the po rest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        public static T Call<T>(PO.BL.Contracts.Requests.RequestBase request, string restService, string operation) where T : PO.BL.Contracts.Responses.ResponseBase
        {
            string endpoint = Configuration.PORestServiceEndpoint;
            return Call<T, PO.BL.Contracts.Requests.RequestBase>(endpoint, request, restService, operation);
        }

        #endregion

        #region ES CALL
        /// <summary>
        /// Calls the External Systems rest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        public static async Task<T> CallAsync<T>(ES.API.Contracts.Requests.Request request, string restService, string operation)
        {
            string endpoint = Configuration.ESAPIServiceEndpoint;
            return await CallAsync<T, ES.API.Contracts.Requests.Request>(endpoint, request, restService, operation);   
        }

        /// <summary>
        /// Calls the ExternalSystems rest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        public static T Call<T>(ES.API.Contracts.Requests.Request request, string restService, string operation) where T : ES.API.Contracts.Response.Response
        {
            string endpoint = Configuration.ESAPIServiceEndpoint;
            return CallJson<T, ES.API.Contracts.Requests.Request>(endpoint, request, restService, operation);
        }

        #endregion

    }
}
