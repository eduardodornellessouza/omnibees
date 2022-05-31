using OB.Domain;

namespace OB.DL.Common.Interfaces
{
    public interface ISessionFactory
    {
        /// <summary>
        /// Gets a unit of work, fetching the existing UnitOfWork for the current Thread, or creating a new one if it doesn't exist yet.
        /// Only one UnitOfWork instance can be created (exists) per thread, this means that subsequent calls to GetUnitOfWork()
        /// will return the same instance of IUnitOfWork until it is disposed.
        /// <example>
        /// var unitOfWork1 = sessionFactory.GetUnitOfWork();
        /// var unitOfWork2 = sessionFactory.GetUnitOfWork();
        /// //unitOfWork1 = unitOfWork2
        ///
        /// unitOfWork2.Dispose();
        ///
        /// var unitOfWork3 = sessionFactory.GetUnitOfWork();
        /// //unitOfWork3 != unitOfWork2
        /// </example>
        /// </summary>
        IUnitOfWork GetUnitOfWork(params DomainScope[] domainScopes);

        IUnitOfWork GetUnitOfWork(bool readOnly = false, params DomainScope[] domainScopes);

        /// <summary>
        /// Gets the UnitOfWork for the Current thread returning one if it already exists or null if it hasn't been created before.
        /// </summary>
        IUnitOfWork CurrentUnitOfWork { get; }

        void SetTrace(bool isActive);

        void StopTrace();

        void StartTrace();
    }
}