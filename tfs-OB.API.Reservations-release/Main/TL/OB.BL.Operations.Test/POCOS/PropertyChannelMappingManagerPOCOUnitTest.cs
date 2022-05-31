using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using contractsReservation = OB.BL.Contracts.Data.Reservations;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Interfaces;
using OB.DL.Common.Interfaces;
using OB.Domain.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.BL.Operations.Internal.TypeConverters;
using OB.Domain.General;
using System.Linq.Expressions;
using OB.BL.Operations.Exceptions;
using OB.Domain.Properties;
using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.DL.Common.Infrastructure;
using OB.Domain;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.Domain.Channels;

namespace OB.BL.Operations.Test
{
    [TestClass]
    public class PropertyChannelMappingManagerPOCOUnitTest : UnitBaseTest
    {
        private Mock<IRepository<UserProperty>> _userPropertyRepoMock = null;
        private Mock<IChannelsRepository> _channelsRepoMock = null;
        private Mock<IPropertyRepository> _propertiesRepoMock = null;
        private Mock<IChannelsPropertyRepository> _channelsPropertyRepo = null;
        private Mock<IRepository<PropertyChannelMapping>> _propertyChannelMappingRepo = null;
        private Mock<IRepository<PropertyChannelMappingsExtendedField>> _propertyChannelMappingsExtendedFieldRepo = null;
        private Mock<IRepository<ChannelMappingType>> _channelMappingTypeRepo = null;
        private Mock<IRepository<ChannelMappingExtendedField>> _channelMappingExtendedFieldRepo = null;
        private Mock<IRepository<ChannelMappingExtendedFieldDefaultValue>> _channelMappingExtendedFieldDefaultValueRepo = null;
        
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _userPropertyRepoMock = new Mock<IRepository<UserProperty>>(MockBehavior.Default);
            _channelsRepoMock = new Mock<IChannelsRepository>(MockBehavior.Default);
            _propertiesRepoMock = new Mock<IPropertyRepository>(MockBehavior.Default);
            _channelsPropertyRepo = new Mock<IChannelsPropertyRepository>(MockBehavior.Default);
            _propertyChannelMappingRepo = new Mock<IRepository<PropertyChannelMapping>>(MockBehavior.Default);
            _propertyChannelMappingsExtendedFieldRepo = new Mock<IRepository<PropertyChannelMappingsExtendedField>>(MockBehavior.Default);
            _channelMappingTypeRepo = new Mock<IRepository<ChannelMappingType>>(MockBehavior.Default);
            _channelMappingExtendedFieldRepo = new Mock<IRepository<ChannelMappingExtendedField>>(MockBehavior.Default);
            _channelMappingExtendedFieldDefaultValueRepo = new Mock<IRepository<ChannelMappingExtendedFieldDefaultValue>>(MockBehavior.Default);

            // Mock Repository factory
            var repoFactoryMock = new Mock<IRepositoryFactory>();
            repoFactoryMock.Setup(x => x.GetChannelsRepository(It.IsAny<IUnitOfWork>())).Returns(_channelsRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetPropertyRepository(It.IsAny<IUnitOfWork>())).Returns(_propertiesRepoMock.Object);
            repoFactoryMock.Setup(x => x.GetChannelsPropertyRepository(It.IsAny<IUnitOfWork>())).Returns(_channelsPropertyRepo.Object);
           
            // Mock Framework
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var sessionFactoryMock = new Mock<ISessionFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);
            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);
            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(repoFactoryMock.Object);
            this.Container = this.Container.AddExtension(new BusinessLayerModule());

            // Mock channels
            _channelsRepoMock.Setup(x => x.GetQuery()).Returns(new List<Channel>()
            { 
                new Channel{UID = 2, Name = "Booking"},
                new Channel{UID = 3, Name = "Expedia"},
                new Channel{UID = 4, Name = "Travelocity"}
            }.AsQueryable());

            _channelsRepoMock.Setup(x => x.GetQuery()).Returns(new List<Channel>()
            { 
                new Channel{UID = 2, Name = "Booking"},
                new Channel{UID = 3, Name = "Expedia"},
                new Channel{UID = 4, Name = "Travelocity"}
            }.AsQueryable());


            // Mock properties
            _propertiesRepoMock.Setup(x => x.GetQuery()).Returns(new List<Property>()
            { 
                new Property{UID = 1, Name = "Hotel A", IsActive = true},
                new Property{UID = 2, Name = "Hotel B", IsActive = true},
                new Property{UID = 3, Name = "Hotel C", IsActive = true},
                new Property{UID = 4, Name = "Hotel D Inactive", IsActive = false}
            }.AsQueryable());


            // Mock channels properties
            _channelsPropertyRepo.Setup(x => x.GetQuery()).Returns(new List<ChannelsProperty>()
            { 
                new ChannelsProperty{UID = 1, Property_UID = 1, Channel_UID = 2, IsActive = true,  IsPendingRequest = false },
                new ChannelsProperty{UID = 2, Property_UID = 1, Channel_UID = 3, IsActive = true,  IsPendingRequest = false },
                new ChannelsProperty{UID = 3, Property_UID = 1, Channel_UID = 4, IsActive = false, IsPendingRequest = true  },
                new ChannelsProperty{UID = 4, Property_UID = 2, Channel_UID = 2, IsActive = true,  IsPendingRequest = false },
                new ChannelsProperty{UID = 4, Property_UID = 2, Channel_UID = 3, IsActive = true,  IsPendingRequest = false }
            }.AsQueryable());


        }

        #region Test Validate Guarantee

        [TestMethod]
        [TestCategory("ChannelMappingManagerPOCO")]
        public void Test_ChannelMappingManagerPOCO_istPropertiesChannelMappingExtendedInfo()
        {
            // arrange
            _propertyChannelMappingRepo.Setup(x => x.GetQuery()).Returns(new List<PropertyChannelMapping>()
            { 
                new PropertyChannelMapping{UID = 1, Property_UID = 1, Channel_UID = 2, IsDeleted = false},
                new PropertyChannelMapping{UID = 1, Property_UID = 1, Channel_UID = 2, IsDeleted = false},
                new PropertyChannelMapping{UID = 2, Property_UID = 1, Channel_UID = 3, IsDeleted = false},
                new PropertyChannelMapping{UID = 2, Property_UID = 1, Channel_UID = 3, IsDeleted = false},
                new PropertyChannelMapping{UID = 3, Property_UID = 1, Channel_UID = 4, IsDeleted = false},
                new PropertyChannelMapping{UID = 3, Property_UID = 1, Channel_UID = 4, IsDeleted = false},
                new PropertyChannelMapping{UID = 4, Property_UID = 2, Channel_UID = 2, IsDeleted = false},
                new PropertyChannelMapping{UID = 4, Property_UID = 2, Channel_UID = 2, IsDeleted = false},
                new PropertyChannelMapping{UID = 4, Property_UID = 2, Channel_UID = 3, IsDeleted = false},
                new PropertyChannelMapping{UID = 4, Property_UID = 2, Channel_UID = 3, IsDeleted = false}
            }.AsQueryable());

            // act
            var systemUnderTest = this.Container.Resolve<IPropertyChannelMappingManagerPOCO>();
            var response = systemUnderTest.ListPropertiesChannelMappingSummary(new ListPropertiesChannelMappingSummaryRequest()
                {
                    PageIndex = 0,
                    PageSize = 10,
                    ReturnTotal = true,                    
                });

            // assert
            Assert.IsTrue(response.Result.Any());
        }

        #endregion
    }
}
