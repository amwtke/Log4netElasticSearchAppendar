﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogManager.Tests
{
    [TestClass()]
    public class FileOperationTests
    {
        [TestMethod()]
        public void WriteFileAppandTest()
        {
            int i = 1;
            FileOperation.WriteFileAppand("transferlog.txt", i.ToString());
            i++;
            FileOperation.WriteFileAppand("transferlog.txt", i.ToString());
            Assert.IsTrue(true);
        }
    }
}

namespace LogHelper.Tests
{
    [TestClass()]
    public class FileOperationTests
    {
        [TestMethod()]
        public void WriteFileTest()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            FileOperation.WriteFile("Transport", date);

            Assert.AreEqual(FileOperation.ReadFile("Transport"),date);
        }
    }
}