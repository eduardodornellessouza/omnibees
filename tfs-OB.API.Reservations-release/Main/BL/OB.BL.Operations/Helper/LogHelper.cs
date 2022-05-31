using System;
using System.Linq;

namespace OB.BL.Operations.Helper
{
    // USED FOR APPLICATION ERROR LOGGING
    public class LogHelper
    {
        public string GetExceptionMessage(Exception ex)
        {
            if (ex == null)
                return string.Empty;
            
            return ex.ToString();
        }

        public string GetMethodSignature(System.Reflection.MethodBase method)
        {
            string MethodSignature;
            if (method.GetParameters() != null && method.GetParameters().Any())
                MethodSignature = method.DeclaringType.FullName + "." + method.Name + "(" + string.Join(",", method.GetParameters().Select(p => p.ParameterType.ToString() + " " + p.Name)) + ")";
            else
                MethodSignature = method.DeclaringType.FullName + "." + method.Name + "()";

            return MethodSignature;
        }
    };
}
