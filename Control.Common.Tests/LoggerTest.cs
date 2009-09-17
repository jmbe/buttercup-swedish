using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Buttercup.Control.Common.IO;
using System.IO;
using System.Windows.Resources;
using Buttercup.Control.Common.Net;
using Buttercup.Control.Common;

namespace Control.Common.Tests
{
    [TestClass]
    public class LoggerTest
    {
        private const string _LOG_FILE_PATH = "buttercup.log";

        public LoggerTest()
        {
            //FileSystem = IsolatedStorageFileSystem.GetForApplication();
        }

        IFileSystem FileSystem { get; set; }

        [TestMethod]
        public void CreateNewLog()
        {
            Logger.Instance = new Logger(FileSystem);
            Assert.IsTrue(FileSystem.FileExists(_LOG_FILE_PATH));
        }

        [TestMethod]
        public void WriteTextLog()
        {
            Logger.Log("Test text");
        }

        [TestMethod]
        public void WriteExceptionLog()
        {
            Logger.Log(new Exception());
            Logger.Log(new MulticastNotSupportedException());
            Logger.Log(new AccessViolationException());
        }

        [TestMethod]
        public void WriteArgsLog()
        {
            string[] args = {"arg1", "arg2"};
            Logger.Log("Test args: {0}, {1}", args);
        }

        [TestCleanup]
        public void CleanUp()
        {
            
        }

        [TestInitialize]
        public void StartUp()
        {
            if (FileSystem == null)
            {
                FileSystem = IsolatedStorageFileSystem.GetForApplication();
                if (FileSystem.FileExists(_LOG_FILE_PATH))
                    FileSystem.DeleteFile(_LOG_FILE_PATH);
            }
        }
    }
}
