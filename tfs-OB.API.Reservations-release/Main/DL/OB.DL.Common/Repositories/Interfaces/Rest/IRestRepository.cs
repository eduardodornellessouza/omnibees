using OB.DL.Common.Infrastructure;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    /// <summary>
    /// Base Interface for all Repository Classes.
    /// </summary>
    public interface IRestRepository<TEntity> : IRepository where TEntity : class
    {

    }
}