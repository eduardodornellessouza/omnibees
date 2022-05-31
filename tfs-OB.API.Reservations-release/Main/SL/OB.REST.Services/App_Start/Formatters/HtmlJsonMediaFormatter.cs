using Newtonsoft.Json;
using OB.Reservation.BL.Contracts.Responses;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace OB.REST.Services.Formatters
{
    /// <summary>
    /// MediaFormatter for HTTP requests that accept 'text/html' as response.
    /// </summary>
    internal class HtmlJsonMediaFormatter : MediaTypeFormatter
    {
        public HtmlJsonMediaFormatter()
            : base()
        {
            this.SupportedMediaTypes.Add(HtmlMediaTypeHeaderValue.Value);
            this.MediaTypeMappings.Add(new RequestHeaderMapping("Accept", "text/html", StringComparison.OrdinalIgnoreCase, true, HtmlMediaTypeHeaderValue.Value));

            this.SupportedEncodings.Add(new UTF8Encoding(false, true));
            this.SupportedEncodings.Add(new UnicodeEncoding(false, true, true));
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(ResponseBase).IsAssignableFrom(type);
        }


        //
        // Summary:
        //     Asynchronously writes an object of the specified type.
        //
        // Parameters:
        //   type:
        //     The type of the object to write.
        //
        //   value:
        //     The object value to write. It may be null.
        //
        //   writeStream:
        //     The System.IO.Stream to which to write.
        //
        //   content:
        //     The System.Net.Http.HttpContent if available. It may be null.
        //
        //   transportContext:
        //     The System.Net.TransportContext if available. It may be null.
        //
        //   cancellationToken:
        //     The token to cancel the operation.
        //
        // Returns:
        //     A System.Threading.Tasks.Task that will perform the write.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     Derived types need to support writing.
        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
        {
            var config = GlobalConfiguration.Configuration;

            var routeData = HttpContext.Current.Request.RequestContext.RouteData;
            var requestContext = new RequestContext(new System.Web.HttpContextWrapper(HttpContext.Current), routeData);
            ////var urlHelper = new UrlHelper(requestContext);

            var controllerName = routeData.Values["controller"] as string;
            var action = routeData.Values["action"] as string;
            var method = HttpContext.Current.Request.HttpMethod;
            var parameters = HttpUtility.UrlDecode(HttpContext.Current.Request.QueryString.ToString());


            var controllerMapping = config.Services.GetHttpControllerSelector().GetControllerMapping();
            var descriptor = controllerMapping[controllerName];
            var controllerType = descriptor.ControllerType;


            var actionDescriptor = config.Services.GetActionSelector().GetActionMapping(descriptor)[action]
                    .FirstOrDefault(x => x.SupportedHttpMethods.Select(m => m.Method).Contains(method));

            var actionParameters = actionDescriptor.GetParameters();

            var documentationProvider = config.Services.GetDocumentationProvider();
            var controllerDescription = documentationProvider.GetDocumentation(descriptor);

            var actionDescription = documentationProvider.GetDocumentation(actionDescriptor);

            var actionParametersDescription = new StringBuilder();

            foreach (var actionParameter in actionParameters)
            {
                actionParametersDescription.AppendFormat("<tr><td style='border-right:1px solid darkgray;'><b>{0}</b></td><td><pre style='margin-left:10px'>{1}</pre></td></tr>", actionParameter.ParameterName, documentationProvider.GetDocumentation(actionParameter));
            }

            var responseDescription = documentationProvider.GetResponseDocumentation(actionDescriptor);

            Encoding effectiveEncoding = SelectCharacterEncoding(content.Headers);

            var writer = new StreamWriter(writeStream, effectiveEncoding);

            await writer.WriteLineAsync("<html>");
            await writer.WriteLineAsync("<body style='font-family: Segoe UI Light, Frutiger, Frutiger Linotype, Dejavu Sans, Helvetica Neue, Arial, sans-serif;'>");
            await writer.WriteLineAsync("<h1>OmniBees RESTful Services API</h1>");
            await writer.WriteLineAsync(string.Format("<table><tr><td><h4>Controller:</h4></td><td>{0}</td></tr>", controllerName));
            await writer.WriteLineAsync(string.Format("<tr><td><h4>Action:</h4></td><td>{0}</td></tr>", action));
            await writer.WriteLineAsync(string.Format("<tr><td><h4>Method:</h4></td><td>{0}</td></tr>", method));
            await writer.WriteLineAsync(string.Format("<tr><td><h4>Query Parameters:</h4></td><td>{0}</td></tr></table>", parameters));

            await writer.WriteLineAsync(string.Format("<h3>Summary:</h3><pre>{0}</pre>", actionDescription));

            await writer.WriteLineAsync(string.Format("<h3>Parameters:</h3><table style='border:1px solid darkgray;'>{0}</table>", actionParametersDescription));

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;

            string json = JsonConvert.SerializeObject(value, settings);

            await writer.WriteAsync("<h3>Response Result:</h3>");
            await writer.WriteAsync("<pre>");
            await writer.WriteAsync(json);
            await writer.WriteAsync("</pre>");

            await writer.WriteAsync("<h3>Response Information:</h3>");
            await writer.WriteAsync(string.Format("<pre>{0}</pre>", responseDescription));

            await writer.WriteLineAsync("</body>");
            await writer.WriteLineAsync("</html>");
            await writer.FlushAsync();
        }
    }

    public class HtmlMediaTypeHeaderValue : MediaTypeHeaderValue
    {
        protected HtmlMediaTypeHeaderValue() : base("text/html") { }

        public static readonly HtmlMediaTypeHeaderValue Value = new HtmlMediaTypeHeaderValue();
    }
}
