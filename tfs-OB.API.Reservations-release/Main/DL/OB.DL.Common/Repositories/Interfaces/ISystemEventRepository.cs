using OB.Domain.ProactiveActions;

namespace OB.DL.Common.Interfaces
{
    /// <summary>
    /// Interface that defines the finders available for the SystemEvent repository implementations.
    /// </summary>
    public interface ISystemEventRepository : IRepository<SystemEvent>
    {
        /// <summary>
        /// Finds the SystemEvent given its code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        SystemEvent FindByEventCode(string code);

        /// <summary>
        /// Finds the SystemEvent given its code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        SystemEvent FindByEventCode(int code);
    }
}