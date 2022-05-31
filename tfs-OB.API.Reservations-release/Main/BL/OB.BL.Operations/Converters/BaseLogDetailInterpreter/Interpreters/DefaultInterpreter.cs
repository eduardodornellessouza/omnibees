using OB.BL.Contracts.Data.BaseLogDetails;
using OB.Events.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Converters.BaseLogDetailInterpreter.Interpreters
{
    public class DefaultInterpreter : Interpreter
    {
        public DefaultInterpreter(NotificationBase notification)
            : base(notification)
        {

        }

        public override Dictionary<string, int> GetOrder()
        {
            return new Dictionary<string, int>();
        }

        public override GridLineDetail GetGridLine(NotificationBase notification)
        {
            var gridLine = new GridLineDetail();

            gridLine.Action = (int)notification.Action;
            gridLine.SubActions = GetSubActions(notification);
            gridLine.PropertyId = notification.PropertyUID;
            gridLine.CreatedByUsername = notification.CreatedByName;

            return gridLine;
        }

        private List<int> GetSubActions(NotificationBase notification)
        {
            var subActions = new List<int>();
            
            if(notification.SubActions != null)
            {
                subActions.AddRange(notification.SubActions.Select(q => (int)q).ToList());
            }

            return subActions;
        }
    }
}
