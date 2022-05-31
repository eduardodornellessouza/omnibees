using OB.DL.Common.Impl;
using OB.DL.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.Api.Core;

namespace OB.DL.Common.Test.Helper
{
    internal class UnitTestSessionFactory : SessionFactory
    {
        //internal override Type GetContextType(DomainScope scope)
        //{
        //    return typeof(UnitTestContext);
        //}
        public UnitTestSessionFactory(IReadOnlyCollection<DomainScope> domainScopes) : base(domainScopes)
        {
        }
    }
}
