using Microsoft.Practices.Unity;
using OB.DL.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.Api.Core;

namespace OB.BL.Operations.Test.Helper
{
    [Serializable]
    public class BaseBuilder
    {
        public BaseBuilder() { }

        public BaseBuilder(IUnityContainer container)
        {
            this._container = container;
        }

        private IUnityContainer _container;

        public IUnityContainer Container
        {
            get { return _container; }
        }

        private IUnitOfWork _unitOfWork;
        public IUnitOfWork UnitOfWork
        {
            get { return GetUnitOfWork(); }
            set { _unitOfWork = value; }
        }

        public IRepositoryFactory RepositoryFactory
        {
            get
            {
                return Container.Resolve<IRepositoryFactory>();
            }
        }

        public ISessionFactory SessionFactory
        {
            get
            {
                return Container.Resolve<ISessionFactory>();
            }
        }

        public IUnitOfWork GetUnitOfWork()
        {
            _unitOfWork = SessionFactory.GetUnitOfWork();
            return _unitOfWork;
        }
    }
}
