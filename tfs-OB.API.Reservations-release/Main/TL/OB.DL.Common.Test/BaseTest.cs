using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OB.DL.Common.Test
{
    /// <summary>
    /// Base class for all Test classes in this assembly. Migrate this to a common project?!?.
    /// </summary>
    public class BaseTest
    {

        private class TestCleanupData
        {
            public List<string> FilesToDelete { get; set;}
            public TestContext TestContext { get; set; }
        }

        private static List<TestCleanupData> _cleanupDatas = new List<TestCleanupData>();

      

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
          
            SqlConnection.ClearAllPools();

            var failed = TestContext.CurrentTestOutcome == UnitTestOutcome.Failed
                ||
                TestContext.CurrentTestOutcome == UnitTestOutcome.Error;

            var filesToDelete = new List<string>();

            //TMOREIRA: uncomment this line to keep the Database files of the Tests that failed.
            if(!failed)
            {
                var testDir = TestContext.TestDeploymentDir;

                DirectoryInfo testDirInfo = new DirectoryInfo(testDir + "/" + TestContext.TestName);
                if (testDirInfo.Exists)
                {
                    var files = testDirInfo.GetFiles("*.mdf").Concat(testDirInfo.GetFiles("*.ldf"));


                    foreach (var file in files)
                    {
                        filesToDelete.Add(file.FullName);
                    }
                }
            }
            _cleanupDatas.Add(new TestCleanupData()
            {
                FilesToDelete = filesToDelete,
                TestContext = this.TestContext,
            });                      

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {

            Thread.Sleep(2000);

            //var dataSource = ConfigurationManager.ConnectionStrings["ReservationsConnectionString"];
            //var connectionString = dataSource.ConnectionString;
            //using (SqlConnection conn = new SqlConnection(connectionString))
            //{
            //    conn.Open();
            //    var command = conn.CreateCommand();
            //    command.CommandText = "SHUTDOWN WITH NOWAIT";
            //    command.ExecuteNonQuery();
            //    conn.Close();
            //}

            foreach (var cleanupData in _cleanupDatas)
            {
                foreach (var fileToDelete in cleanupData.FilesToDelete)
                {
                    int retries = 20;
                    while (true)
                    {
                        try
                        {
                            FileInfo fileInfo = new FileInfo(fileToDelete);

                            if (fileInfo.Exists)
                            {
                                Console.WriteLine(string.Format("Deleting:{0}", fileToDelete));
                                fileInfo.Delete();
                                Console.WriteLine(string.Format("Deleted:{0}", fileToDelete));
                            }
                            break;
                        }
                        catch (IOException)
                        {
                            retries--;
                            if (retries == 0)
                                break;
                            Thread.Sleep(2000);
                        }
                    }
                }
            }


        }


    }

}

