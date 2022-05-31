using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
//using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.REST.Services.Test
{
    /// <summary>
    /// Base class for all Test classes in this assembly. Migrate this to a common project?!?.
    /// </summary>
    public class BaseTest
    {

        public TestContext TestContext
        {
            get;
            set;
        }

        [TestInitialize]
        public virtual void Initialize()
        {
            //var instance = System.Data.Entity.SqlProviderServices.Instance;
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(TestContext.TestDeploymentDir, TestContext.TestName));

        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            //SqlConnection.ClearAllPools();
        }
    }

}
