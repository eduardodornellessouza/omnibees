using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.BL.Operations.Helper.Interfaces;
using OB.BL.Operations.Test.Mocks;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.Api.Core;

namespace OB.BL.Operations.Test
{
    [TestClass]
    public class IntegrationBaseTest : BaseTest
    {
        private IUnityContainer _container;

        protected IUnityContainer Container 
        {         
            get 
            { 
                return _container;   
            } 
            set
            {
                _container = value;
            }
        }

        /// <summary>
        /// Property that specifies if the Tests use the Mocked ProjectGeneral to send mails or not.
        /// </summary>
        public virtual bool UseMailSenderMock
        {
            get
            {
                return false;
            }
        }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

       

            IUnityContainer containerScope = new UnityContainer();
            containerScope = containerScope.AddExtension(new DataAccessLayerModule());

            if (UseMailSenderMock)
            {
                containerScope.RegisterType<IProjectGeneral, ProjectGeneralMock>(new ContainerControlledLifetimeManager());
            }
            containerScope = containerScope.AddExtension(new BusinessLayerModule());

            Container = containerScope;

            var currentUnitOfWork = this.SessionFactory.CurrentUnitOfWork;
            if (currentUnitOfWork != null)
                currentUnitOfWork.Dispose();

        }

        [TestCleanup]
        public override void Cleanup()
        {
            var currentUnitOfWork = this.SessionFactory.CurrentUnitOfWork;
            if (currentUnitOfWork != null && !currentUnitOfWork.IsDisposed)
                currentUnitOfWork.Dispose();


            //clean mails
            if (UseMailSenderMock)
            {             
                ProjectGeneralMock mockMailSender = Container.Resolve<IProjectGeneral>() as ProjectGeneralMock;
                mockMailSender.SentMails.Clear();
            }

            var oldContainer = Container;
            var oldSessionFactory = this.SessionFactory;
            var oldRepositoryFactory = this.RepositoryFactory;


         
            oldContainer.Teardown(oldSessionFactory);
            oldContainer.Teardown(oldRepositoryFactory);
            oldContainer = oldContainer.RemoveAllExtensions();

            oldContainer.Dispose();

            this.Container = new UnityContainer();

            base.Cleanup();
        }

        protected virtual ProjectGeneralMock MailSenderMock
        {
            get
            {
                return this.Container.Resolve<IProjectGeneral>() as ProjectGeneralMock;
            }
        }

        protected virtual ISessionFactory SessionFactory
        {
            get
            {
                return this.Container.Resolve<ISessionFactory>();
            }

        }

        protected virtual IRepositoryFactory RepositoryFactory
        {
            get
            {
                return this.Container.Resolve<IRepositoryFactory>();
            }
        }

    }
}
