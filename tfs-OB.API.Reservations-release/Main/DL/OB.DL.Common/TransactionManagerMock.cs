using OB.Api.Core;
using System;
using System.Transactions;

namespace OB.DL.Common
{
    internal class TransactionManagerMock : ITransactionManager
    {
        public ITransactionScope BeginTransactionScope(DomainScope scope, TransactionScopeOption scopeOption = TransactionScopeOption.Required, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, TimeSpan scopeTimeout = default, TransactionScopeAsyncFlowOption asyncFlowOption = TransactionScopeAsyncFlowOption.Enabled)
        {
            #if !DEBUG
                // Do not remove this line. If the code is reaching here you need to remove the place where this class is registered.
                throw new Exception("The TransactionManager is mocked! Please fix the DataAccesLayerModule register.");
            #endif
            return new TransactionScopeMock();
        }
    }

    internal class TransactionScopeMock : ITransactionScope
    {
        public void Complete()
        {
        }

        public void Dispose()
        {
        }
    }
}
