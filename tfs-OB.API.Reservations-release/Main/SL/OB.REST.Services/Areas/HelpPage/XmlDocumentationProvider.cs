using OB.REST.Services.Areas.HelpPage.ModelDescriptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;

namespace OB.REST.Services.Areas.HelpPage
{
    /// <summary>
    /// A custom <see cref="IDocumentationProvider"/> that reads the API documentation from an XML documentation file.
    /// </summary>
    public class XmlDocumentationProvider : IDocumentationProvider, IModelDocumentationProvider
    {
        private Dictionary<string, XPathNavigator> _documentNavigators = new Dictionary<string,XPathNavigator>();
        private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
        private const string MethodExpression = "/doc/members/member[@name='M:{0}']";
        private const string PropertyExpression = "/doc/members/member[@name='P:{0}']";
        private const string FieldExpression = "/doc/members/member[@name='F:{0}']";
        private const string ParameterExpression = "param[@name='{0}']";

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocumentationProvider"/> class.
        /// </summary>
        /// <param name="assemblyName">The name of the Assembly to register the XML documentation path with. 
        /// It's required in order to map Types to the respective Assembly XML documentation file.</param>
        /// <param name="documentPath">The physical path to XML document.</param>
        public XmlDocumentationProvider(string assemblyName, string documentPath)
        {
            if (documentPath == null)
            {
                throw new ArgumentNullException("documentPath");
            }

            var memStream = new MemoryStream();
            using (var fileStream = File.OpenRead(documentPath))
            {
                fileStream.CopyTo(memStream, 4048);
                memStream.Position = 0;

                XPathDocument xpath = new XPathDocument(memStream);
                _documentNavigators.Add(assemblyName, xpath.CreateNavigator());
                fileStream.Close();
            }
        }

        public void AddDocumentationAssembly(string assemblyName, string documentPath)
        {
            if (documentPath == null)
            {
                throw new ArgumentNullException("documentPath");
            }
            var memStream = new MemoryStream();
            using (var fileStream = File.OpenRead(documentPath))
            {
                fileStream.CopyTo(memStream, 4048);
                memStream.Position = 0;

                XPathDocument xpath = new XPathDocument(memStream);
                _documentNavigators.Add(assemblyName, xpath.CreateNavigator());
                
                fileStream.Close();
            }
        }

        public string GetDocumentation(HttpControllerDescriptor controllerDescriptor)
        {
            XPathNavigator typeNode = GetTypeNode(controllerDescriptor.ControllerType);
            return GetTagValue(typeNode, "summary");
        }

        public virtual string GetDocumentation(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator methodNode = GetMethodNode(actionDescriptor);
            return GetTagValue(methodNode, "summary");
        }

        public virtual string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            ReflectedHttpParameterDescriptor reflectedParameterDescriptor = parameterDescriptor as ReflectedHttpParameterDescriptor;
            if (reflectedParameterDescriptor != null)
            {
                XPathNavigator methodNode = GetMethodNode(reflectedParameterDescriptor.ActionDescriptor);
                if (methodNode != null)
                {
                    string parameterName = reflectedParameterDescriptor.ParameterInfo.Name;
                    XPathNavigator parameterNode = methodNode.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, ParameterExpression, parameterName));
                    if (parameterNode != null)
                    {
                        return parameterNode.Value.Trim();
                    }
                }
            }

            return null;
        }

        public string GetResponseDocumentation(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator methodNode = GetMethodNode(actionDescriptor);
            return GetTagValue(methodNode, "returns");
        }

        public string GetDocumentation(MemberInfo member)
        {
            string memberName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", GetTypeName(member.DeclaringType), member.Name);
            string expression = member.MemberType == MemberTypes.Field ? FieldExpression : PropertyExpression;
            string selectExpression = String.Format(CultureInfo.InvariantCulture, expression, memberName);

            var assemblyName = member.DeclaringType.Assembly.GetName().Name;

            if(!_documentNavigators.ContainsKey(assemblyName))
                return default(string);

            var documentNavigator = _documentNavigators[assemblyName];

            XPathNavigator propertyNode = documentNavigator.SelectSingleNode(selectExpression);
            return GetTagValue(propertyNode, "summary");
        }

        public string GetDocumentation(Type type)
        {
            XPathNavigator typeNode = GetTypeNode(type);
            return GetTagValue(typeNode, "summary");
        }

        private XPathNavigator GetMethodNode(HttpActionDescriptor actionDescriptor)
        {
            ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null)
            {

                var methodInfo = reflectedActionDescriptor.MethodInfo;
                var assemblyName = methodInfo.DeclaringType.Assembly.GetName().Name;
                var documentNavigator = _documentNavigators[assemblyName];


                string selectExpression = String.Format(CultureInfo.InvariantCulture, MethodExpression, GetMemberName(methodInfo));
                return documentNavigator.SelectSingleNode(selectExpression);
            }

            return null;
        }

        private static string GetMemberName(MethodInfo method)
        {
            string name = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", GetTypeName(method.DeclaringType), method.Name);
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 0)
            {
                string[] parameterTypeNames = parameters.Select(param => GetTypeName(param.ParameterType)).ToArray();
                name += String.Format(CultureInfo.InvariantCulture, "({0})", String.Join(",", parameterTypeNames));
            }

            return name;
        }

        private static string GetTagValue(XPathNavigator parentNode, string tagName)
        {
            if (parentNode != null)
            {
                XPathNavigator node = parentNode.SelectSingleNode(tagName);
                if (node != null)
                {
                    return node.Value.Trim();
                }
            }

            return null;
        }

        private XPathNavigator GetTypeNode(Type type)
        {
            string controllerTypeName = GetTypeName(type);
            string selectExpression = String.Format(CultureInfo.InvariantCulture, TypeExpression, controllerTypeName);

            string assemblyName = type.Assembly.GetName().Name;

            if(!_documentNavigators.ContainsKey(assemblyName))
                return default(XPathNavigator);

            var documentNavigator = _documentNavigators[assemblyName];

            return documentNavigator.SelectSingleNode(selectExpression);
        }

        private static string GetTypeName(Type type)
        {
            string name = type.FullName;
            if (type.IsGenericType)
            {
                // Format the generic type name to something like: Generic{System.Int32,System.String}
                Type genericType = type.GetGenericTypeDefinition();
                Type[] genericArguments = type.GetGenericArguments();
                string genericTypeName = genericType.FullName;

                // Trim the generic parameter counts from the name
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                string[] argumentTypeNames = genericArguments.Select(t => GetTypeName(t)).ToArray();
                name = String.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", genericTypeName, String.Join(",", argumentTypeNames));
            }
            if (type.IsNested)
            {
                // Changing the nested type name from OuterType+InnerType to OuterType.InnerType to match the XML documentation syntax.
                name = name.Replace("+", ".");
            }

            return name;
        }
    }
}
