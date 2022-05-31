using OB.BL.Operations.Interfaces;
using System.Threading.Tasks;

namespace OB.BL.Operations.Helper
{
    public class NullRegisteredTasksManager : IRegisteredTasksManager
    {
        void IRegisteredTasksManager.RegisterTask(Task task)
        {
            // TODO: Why this is empty?
        }
    }
}