using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OB.Log;
using OB.Log.Messages;
using OB.Reservation.BL.Contracts.Requests;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OB.REST.Services.Loggers
{
    public static class LoggerHelper
    {
        public static string GetRequestId(string request, ILogger logger)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(request))
                    result = JsonConvert.DeserializeObject<RequestBase>(request).RequestId;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error getting request id");
            }

            return result;
        }
        public static async Task<string> ReadHttpContent(HttpContent content, LogMessageBase logMsgBefore, ILogger logger)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(content?.Headers?.ContentType?.MediaType))
                {
                    switch (content.Headers.ContentType.MediaType)
                    {
                        case "application/bson":
                            result = await ReadBson(content);
                            logMsgBefore.Description = "BSON";
                            break;
                        case "application/json":
                            result = await content.ReadAsStringAsync();
                            logMsgBefore.Description = "JSON";
                            break;
                        default:
                            logMsgBefore.Description += " Unknown content type in request";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error getting request");
            }

            return result;
        }

        private static async Task<string> ReadBson(HttpContent content)
        {
            JObject result;
            var bytes = await content.ReadAsByteArrayAsync();
            MemoryStream ms = new MemoryStream(bytes);
            using (var reader = new Newtonsoft.Json.Bson.BsonReader(ms))
            {
                result = (JObject)JToken.ReadFrom(reader);
            }

            return await Task.FromResult(result.ToString(Formatting.None));
        }

    }
}