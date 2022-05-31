using System.Collections.Generic;

namespace OB.BL.Operations.Helper
{
    // HELPER FOR SILVERLIGHT
    public class LogJsonArguments
    {
        public string MethodSignature { get; set; }
        public string ExceptionToString { get; set; }
        public LogSeverity Severity { get; set; }
        public Dictionary<string, string> Arguments { get; set; }
        public Dictionary<string, string> ManualInfo { get; set; }
        public string Subject { get; set; }
        public string To { get; set; }
        public bool SendToOBTeam { get; set; }
        public bool SendToConnectorTeam { get; set; }
    };
}
