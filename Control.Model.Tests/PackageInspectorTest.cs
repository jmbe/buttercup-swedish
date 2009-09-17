using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Resources;
using System.IO;
using Buttercup.Control.Model;
using Buttercup.Control.Model.Exceptions;

namespace Control.Model.Tests
{

    /// <summary>
    /// This class tests the PackageInspector that is responsible for performing the initial
    /// parsing of the DTB zip file.
    /// </summary>
    [TestClass]
    public class PackageInspectorTest
    {
        
        #region Test Methods


        [TestMethod]
        public void Process_An_Valid_Package()
        {
            StreamResourceInfo info = App.GetResourceStream(new Uri(Config.ResourcePath_Valid30Package, UriKind.Relative));
            Stream zipStream = info.Stream;

            // Create a package inspector for the package
            PackageInspector inspector = new PackageInspector();
            inspector.ProcessPackage(zipStream);

            Assert.IsTrue(inspector.IsValid && inspector.Package != null);
        }

        [TestMethod]
        public void Process_From_An_Invalid_Stream()
        {
            StreamResourceInfo info = App.GetResourceStream(new Uri(Config.ResourcePath_InvalidPackage, UriKind.Relative));
            ProcessInvalidPackage(info.Stream);
        }

        [TestMethod]
        public void Process_From_An_Null_Stream()
        {
            // Create a package inspector for the package
            PackageInspector inspector = new PackageInspector();
            ProcessInvalidPackage(null);
        }


        [TestMethod]
        public void Access_Properties_Without_Processing()
        {
            // Create a package inspector for the package
            PackageInspector inspector = new PackageInspector();

            try
            {
                // Attempt to access the IsValid property
                if (inspector.IsValid)
                {
                    // Should never get here because IsValid should raise an exception
                    Assert.Fail("PackageInspector.IsValid should raise an exception if called without processing the stream first.");
                }
            }
            catch (InvalidOperationException)
            {
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail("Unexpected exception");
            }

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper method to process an invalid package 
        /// </summary>
        /// <param name="stream"></param>
        private void ProcessInvalidPackage(Stream zipStream)
        {
            try
            {
                // Create a package inspector for the package
                PackageInspector inspector = new PackageInspector();
                inspector.ProcessPackage(zipStream);

                Assert.Fail("No exception was raised when an invalid stream was processed.");
            }
            catch (InvalidPackageException)
            {
                // Expected result
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail("Unexpected Exception when processing an invalid package.");
            }
        }

        #endregion
    }


}
