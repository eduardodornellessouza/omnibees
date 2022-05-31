using OB.Domain.ProactiveActions;

namespace OB.DL.Common.Interfaces
{
    /// <summary>
    /// Interface that defines the finders available for the SystemAction repository implementations.
    /// </summary>
    public interface ISystemActionRepository : IRepository<SystemAction>
    {
        /// <summary>
        /// Finds the SystemAction given its code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        SystemAction FindByName(string name);

        /// <summary>
        /// Finds the SystemAction given its code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        SystemAction FindByActionCode(string code);

        /// <summary>
        /// Finds the SystemAction given its code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        SystemAction FindByActionCode(int code);
    }
}