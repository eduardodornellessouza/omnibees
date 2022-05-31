using System;
using System.Collections.Generic;

namespace OB.BL.Operations.Interfaces 
{
    public interface IEventSystemManagerPOCO
    {
        void SendMessage(OB.Events.Contracts.ICustomNotification notification);
    }
}
