using OB.DL.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace OB.DL.Common.Infrastructure
{
    public interface ICachedRepository<TEntity> : Repositories.Interfaces.IRepository<TEntity>, ICachedRepository, IDisposable where TEntity : Domain.DomainObject
    {
    }

    public interface ICachedRepository
    {
        void Invalidate(bool forceUpdate = true);
        void Invalidate(long id);
        void Invalidate(IEnumerable<long> ids);
    }
}
