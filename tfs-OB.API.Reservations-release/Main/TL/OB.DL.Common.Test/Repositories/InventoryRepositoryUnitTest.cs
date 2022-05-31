using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.BL.Operations.Test.Helper;
using OB.DL.Common.BusinessObjects;
using OB.DL.Common.Impl;
using OB.DL.Common.Interfaces;
using OB.DL.Common.Test.Helper;
using OB.Domain.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Test.Repositories
{
    [TestClass]
    public class InventoryRepositoryUnitTest : BaseTest
    {
        protected IUnityContainer Container { get; set; }
        private ISessionFactory sessionFactory;
        private IRepositoryFactory repositoryFactory;

        [TestInitialize]
        public override void Initialize()
        { 
            base.Initialize();

            this.Container = new UnityContainer();
            this.Container = this.Container.AddExtension(new DataAccessLayerModule());

            sessionFactory = Container.Resolve<ISessionFactory>() as SessionFactory;
            repositoryFactory = Container.Resolve<IRepositoryFactory>();
        }

        #region Insert
        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Insert_Allocated")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Insert_Allocated")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Insert_Allocated()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container).WithNewAllocated();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Insert_Used")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Insert_Used")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Insert_Used()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container).WithNewUsed();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Insert_AllocatedAjustment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Insert_AllocatedAjustment")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Insert_AllocatedAjustment()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container).WithNewAllocatedAdjustment();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Insert_AllocatedAndUsed")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Insert_AllocatedAndUsed")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Insert_AllocatedAndUsed()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container).WithNewAllocated().WithNewUsed();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Insert_AllocatedAndAllocatedAdjustment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Insert_AllocatedAndAllocatedAdjustment")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Insert_AllocatedAndAllocatedAdjustment()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container).WithNewAllocated().WithNewAllocatedAdjustment();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Insert_UsedAndAllocatedAdjusment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Insert_UsedAndAllocatedAdjusment")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Insert_UsedAndAllocatedAdjusment()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container).WithNewUsed().WithNewAllocatedAdjustment();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        /// <summary>
        /// Tests Insert Allocated, Used and AllocatedAdjusment.
        /// </summary>
        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Insert_AllTypes")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Insert_AllTypes")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Insert_AllTypes()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container)
                .WithNewAllocated()
                .WithNewUsed()
                .WithNewAllocatedAdjustment();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }
        #endregion

        #region Update
        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Update_Allocated")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Update_Allocated")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Update_Allocated()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container).WithExistingAllocated_DiferentQty();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Update_Used")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Update_Used")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Update_Used()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container).WithExistingUsed_DiferentQty();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Update_AllocatedAjustment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Update_AllocatedAjustment")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Update_AllocatedAjustment()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container).WithExistingAllocatedAdjustment_DiferentQty();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Update_AllocatedAndUsed")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Update_AllocatedAndUsed")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Update_AllocatedAndUsed()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container)
                .WithExistingAllocated_DiferentQty()
                .WithExistingUsed_DiferentQty();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Update_AllocatedAndAllocatedAdjustment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Update_AllocatedAndAllocatedAdjustment")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Update_AllocatedAndAllocatedAdjustment()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container)
                .WithExistingAllocated_DiferentQty()
                .WithExistingAllocatedAdjustment_DiferentQty();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Update_UsedAndAllocatedAdjusment")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Update_UsedAndAllocatedAdjusment")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Update_UsedAndAllocatedAdjusment()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container)
                .WithExistingUsed_DiferentQty()
                .WithExistingAllocatedAdjustment_DiferentQty();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        /// <summary>
        /// Tests Update Allocated, Used and AllocatedAdjusment.
        /// </summary>
        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Update_AllTypes_OnlyChangedQty")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Update_AllTypes_OnlyChangedQty")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Update_AllTypes_OnlyChangedQty()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container)
                .WithExistingAllocated_DiferentQty()
                .WithExistingUsed_DiferentQty()
                .WithExistingAllocatedAdjustment_DiferentQty();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        /// <summary>
        /// Tests update Allocated, Used and AllocatedAdjusment with only quantities that hasn't changed.
        /// </summary>
        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Update_AllTypes_OnlyUnchangedQty")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Update_AllTypes_OnlyUnchangedQty")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Update_AllTypes_OnlyUnchangedQty()
        {
            // Arrange
            var inventoryBuilder = new InventoryBuilder(Container).WithExistingAnyType_EqualQty();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }

        /// <summary>
        /// Tests update Allocated, Used and AllocatedAdjusment with mixed quantities, changed and unchanged.
        /// </summary>
        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_Update_AllTypes_ChangedAndUnchangedQty")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_Update_AllTypes_ChangedAndUnchangedQty")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_Update_AllTypes_ChangedAndUnchangedQty()
        {
            var inventoryBuilder = new InventoryBuilder(Container)
                .WithExistingAllocated_DiferentQty()
                .WithExistingUsed_DiferentQty()
                .WithExistingAllocatedAdjustment_DiferentQty()
                .WithExistingAnyType_EqualQty();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }
        #endregion

        #region Insert and Update
        [TestMethod]
        [TestCategory("Inventory")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.mdf", "TestUpdateInventory_InsertAndUpdate_AllTypes")]
        [DeploymentItem("Databases_WithData/Omnibees_dev2_small_no_images.ldf", "TestUpdateInventory_InsertAndUpdate_AllTypes")]
        [DeploymentItem("./DL")]
        public void TestUpdateInventory_InsertAndUpdate_AllTypes()
        {
            var inventoryBuilder = new InventoryBuilder(Container)
                .WithExistingAllocated_DiferentQty()
                .WithExistingUsed_DiferentQty()
                .WithExistingAllocatedAdjustment_DiferentQty()
                .WithExistingAnyType_EqualQty()
                .WithNewAllocated()
                .WithNewUsed()
                .WithNewAllocatedAdjustment();

            this.UpdateInventoryActAndAsserts(inventoryBuilder);
        }
        #endregion

        private void UpdateInventoryActAndAsserts(InventoryBuilder inventoryBuilder)
        {
            IEnumerable<Inventory> modifiedItems;

            // Act
            using (var unitOfWork = sessionFactory.GetUnitOfWork())
            {
                var inventoryRepo = repositoryFactory.GetInventoryRepository(unitOfWork);
                modifiedItems = inventoryRepo.UpdateInventory(inventoryBuilder.InputData.RoomsInventories);
            }

            // Asserts
            Assert.IsNotNull(modifiedItems, "Modified inventories cannot be null");
            Assert.AreEqual(inventoryBuilder.ExpectedData.Inventories.Count(), modifiedItems.Count(), "Modified inventories count should be " + inventoryBuilder.ExpectedData.Inventories.Count());
            Assert.IsTrue(modifiedItems.All(x => x.UID > 0), "Any modified inventory UID is not grather than 0");

            foreach (var expectedInventory in inventoryBuilder.ExpectedData.Inventories)
            {
                Assert.AreEqual(1, modifiedItems.Count(x =>
                    x.RoomType_UID == expectedInventory.RoomType_UID &&
                    x.Date.CompareTo(expectedInventory.Date) == 0 &&
                    x.QtyRoomOccupied == expectedInventory.QtyRoomOccupied));
            }
        }
    }
}
