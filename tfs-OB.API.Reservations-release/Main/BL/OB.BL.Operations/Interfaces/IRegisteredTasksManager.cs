using System;
using System.Threading.Tasks;

namespace OB.BL.Operations.Interfaces
{
    public interface IRegisteredTasksManager
    {
        void RegisterTask(Task task);
    }
}
