using Hangfire;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;

namespace OB.Services.Jobs.Operations
{
    public class CallRestService
    {
        [Queue("omnibees")]
        public void Call(string endpoint, string restService, string operation, object jsonRequest)
        {
            try
            {
                using (var client = new WebClient())
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    serializer.Formatting = Formatting.None;
                    serializer.PreserveReferencesHandling = PreserveReferencesHandling.All;
                    serializer.DefaultValueHandling = DefaultValueHandling.Ignore;

                    var ms = new MemoryStream();
                    var writer = new BsonWriter(ms);
                    serializer.Serialize(writer, jsonRequest);

                    // BSON serialization
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");
                    client.Headers.Add(HttpRequestHeader.Accept, "application/bson");
                    byte[] binaryResult = client.UploadData(endpoint + "/api/" + restService + "/" + operation, "POST", ms.ToArray());
                    var reader = new BsonReader(new BinaryReader(new MemoryStream(binaryResult), Encoding.UTF8));
                    var result = (ResponseBase)serializer.Deserialize<ResponseBase>(reader);
                    
                    if(result != null && result.Status != Status.Success && result.Errors.Count > 0)
                    {
                        throw new Exception(Newtonsoft.Json.JsonConvert.SerializeObject(result.Errors));
                    }
                }
            }
            catch (Exception ex)
            {
                //Dictionary<string, object> arguments = new Dictionary<string, object>();
                //arguments.Add("Endpoint", endpoint);
                //arguments.Add("RestService", restService);
                //arguments.Add("Operation", operation);
                //arguments.Add("Request", jsonRequest);
                //SendEmail(System.Reflection.MethodBase.GetCurrentMethod(), ex, arguments);
                ExceptionDispatchInfo.Capture(ex).Throw();
            }
        }
    }
}