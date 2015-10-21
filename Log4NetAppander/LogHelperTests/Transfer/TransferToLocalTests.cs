using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.ElasticSearch;

namespace LogManager.Tests
{
    [TestClass()]
    public class TransferToLocalTests
    {
        [TestMethod()]
        public void GetLocalBizObjectByUUIDTest()
        {
            string uuid = "b330c169-07e7-4663-9749-4e925b1d5595";
            BizObject biz =  TransferToLocal.GetLocalBizObjectByUUID(uuid);
            Assert.IsTrue(biz.UUID==uuid);
        }
    }
}