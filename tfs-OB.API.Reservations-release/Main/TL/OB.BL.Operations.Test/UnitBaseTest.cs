using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Transactions;

namespace OB.BL.Operations.Test
{
    [TestClass]
    public class UnitBaseTest : BaseTest
    {
        protected IUnityContainer Container { get; set; }

        Mock<IRepositoryFactory> _repositoryFactoryMock;
        protected Mock<IRepositoryFactory> RepositoryFactoryMock => _repositoryFactoryMock;

        Mock<ISessionFactory> sessionFactoryMock = new Mock<ISessionFactory>();
        protected Mock<ISessionFactory> SessionFactoryMock => sessionFactoryMock;

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        protected Mock<IUnitOfWork> UnitOfWorkMock => unitOfWorkMock;

        //Common GroupRule repository cache Mock 
        protected List<GroupRule> groupRuleList = null;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            
            Container = new UnityContainer();

            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);
            sessionFactoryMock.Setup(x => x.GetUnitOfWork(It.IsAny<bool>())).Returns(unitOfWorkMock.Object);
            sessionFactoryMock.Setup(x => x.GetUnitOfWork(It.IsAny<DomainScope>())).Returns(unitOfWorkMock.Object);

            var transactionScopeMock = new Mock<ITransactionScope>();
            transactionScopeMock.Setup(x => x.Complete());

            var transactionManagerMock = new Mock<ITransactionManager>();
            transactionManagerMock.Setup(x => x.BeginTransactionScope(It.IsAny<DomainScope>(), It.IsAny<TransactionScopeOption>(), It.IsAny<IsolationLevel>(),
                It.IsAny<TimeSpan>(), It.IsAny<TransactionScopeAsyncFlowOption>())).Returns(transactionScopeMock.Object);

            var projectGeneralMock = new Mock<OB.BL.Operations.Helper.Interfaces.IProjectGeneral>();
            projectGeneralMock.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(),
                It.IsAny<int?>(), It.IsAny<bool?>())).Returns(true);

            _repositoryFactoryMock = new Mock<IRepositoryFactory>();

            Container = Container.RegisterInstance(_repositoryFactoryMock.Object)
                .RegisterInstance(sessionFactoryMock.Object)
                .AddExtension(new BusinessLayerModule())
                .RegisterInstance(transactionManagerMock.Object)
                .RegisterInstance(projectGeneralMock.Object);

            //Initialize common groupRule repository cache Mock 
            groupRuleList = new List<OB.Domain.Reservations.GroupRule>();

            typeof(Configuration).GetField("_enableNewOffers", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true);
        }

        protected void FillGroupRulesMock()
        {
            groupRuleList.Add(new GroupRule
            {
                RuleType = RuleType.BE,
                BusinessRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                | BusinessRules.HandleDepositPolicy | BusinessRules.HandleCancelationPolicy | BusinessRules.HandlePaymentGateway | BusinessRules.LoyaltyDiscount
                | BusinessRules.GenerateReservationNumber | BusinessRules.BEReservationCalculation | BusinessRules.PriceCalculationAbsoluteTolerance
                | BusinessRules.CalculateExtraBedPrice
            });
            groupRuleList.Add(new GroupRule
            {
                RuleType = RuleType.GDS,
                BusinessRules = BusinessRules.ConvertValuesToPropertyCurrency | BusinessRules.GDSBuyerGroup
            });
            groupRuleList.Add(new GroupRule
            {
                RuleType = RuleType.Omnibees,
                BusinessRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                | BusinessRules.HandleDepositPolicy | BusinessRules.ForceDefaultCancellationPolicy | BusinessRules.UseReservationTransactions | BusinessRules.ApplyNewReservationFilter
                | BusinessRules.CalculateExtraBedPrice
            });
            groupRuleList.Add(new GroupRule
            {
                RuleType = RuleType.PMS,
                BusinessRules = BusinessRules.IgnoreAvailability
            });
            groupRuleList.Add(new GroupRule
            {
                RuleType = RuleType.PortalOperadoras,
                BusinessRules = BusinessRules.ApplyNewReservationFilter | BusinessRules.ConvertValuesToRateCurrency
            });
            groupRuleList.Add(new GroupRule
            {
                RuleType = RuleType.Pull,
                BusinessRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                    | BusinessRules.HandleDepositPolicy | BusinessRules.ForceDefaultCancellationPolicy | BusinessRules.UseReservationTransactions | BusinessRules.CalculatePriceModel
                    | BusinessRules.GenerateReservationNumber | BusinessRules.EncryptCreditCard | BusinessRules.ConvertValuesToPropertyCurrency
                    | BusinessRules.PullTpiReservationCalculation | BusinessRules.ReturnSellingPrices | BusinessRules.ApplyNewReservationFilter | BusinessRules.IsToPreCheckAvailability
                    | BusinessRules.IgnoreDepositPolicyConcat | BusinessRules.ValidateBookingWindow | BusinessRules.CalculateStayWindowWeekDays | BusinessRules.ValidateReservation
            });
            groupRuleList.Add(new GroupRule
            {
                RuleType = RuleType.BEAPI,
                BusinessRules = BusinessRules.ValidateAllotment | BusinessRules.ValidateCancelationCosts | BusinessRules.ValidateGuarantee | BusinessRules.ValidateRestrictions
                    | BusinessRules.HandleDepositPolicy | BusinessRules.HandleCancelationPolicy | BusinessRules.HandlePaymentGateway | BusinessRules.LoyaltyDiscount
                    | BusinessRules.GenerateReservationNumber | BusinessRules.PullTpiReservationCalculation | BusinessRules.ConvertValuesToPropertyCurrency | BusinessRules.PriceCalculationAbsoluteTolerance
                    | BusinessRules.CalculateExtraBedPrice | BusinessRules.ValidateBookingWindow | BusinessRules.CalculateStayWindowWeekDays | BusinessRules.ValidateReservation
            });
        }

        [TestCleanup]
        public override void Cleanup()
        {
           
            if (SessionFactory != null)
            {
                SessionFactory.GetUnitOfWork().Dispose();

              //  Container.Teardown(SessionFactory);
            }
            
            //if(RepositoryFactory != null)
            //    Container.Teardown(RepositoryFactory);

            //var unityContainer = Container.RemoveAllExtensions();

            //unityContainer.Dispose();

            //Container = null;

            base.Cleanup();

        }

        protected virtual ISessionFactory SessionFactory => Container.Resolve<ISessionFactory>();
        protected virtual IRepositoryFactory RepositoryFactory => Container.Resolve<IRepositoryFactory>();
        protected virtual ITransactionManager TransactionManager => Container.Resolve<ITransactionManager>();

    }
}
