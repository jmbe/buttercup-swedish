using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Buttercup.Control.Common.IO;
using Buttercup.Control.Model;
using Buttercup.Control.Model.Entities;
using System.Windows.Resources;
using System.IO;

namespace Control.Model.Tests
{
    [TestClass]
    public class DataContextTest
    {
        IFileSystem FileSystem { get; set; }
        DataContext Context { get; set; }
        string BookId { get; set; }

        public DataContextTest()
        {
            
            // TODO: Should be in the TestIntialize MEthod but that doesn't seem to get called.
            FileSystem = new InMemoryFileSystem();
            Context = new DataContext(FileSystem);
        }

        [TestMethod]
        public void Load_Valid_Book_Into_FileSystem()
        {
            
            StreamResourceInfo info = App.GetResourceStream(new Uri(Config.ResourcePath_Valid30Package, UriKind.Relative));
            Stream zipStream = info.Stream;

            // Create a package inspector for the package
            PackageInspector inspector = new PackageInspector();
            inspector.ProcessPackage(zipStream);

            // Used by subsequent tests
            BookId = inspector.Package.BookId;

            IDirectory directory = Context.LoadZipPackage(inspector, false);

            // Verify that all the files have been saved
            int fileCount = inspector.Package.FileCount;
            int actualFileCount = FileCount(directory);

            // Can't actually use this as some of the opf's have unreliable lists of files
            //Assert.IsTrue(actualFileCount == fileCount, "Number of files in the zip package does not match the number of files loaded into FileSystem.");
            Assert.IsTrue(actualFileCount > 0, "No files have been loaded.");
        }

        [TestMethod]
        public void Get_Existing_Book_From_FileSystem()
        {
            // If it hasn't already been run...
            Load_Valid_Book_Into_FileSystem();
            
            Book book = Context.GetBook(BookId);
            Assert.IsTrue(book.Title != null);
        }

        [TestInitialize]
        private void SetUp()
        {
            // Never fires!
            FileSystem = new InMemoryFileSystem();
            Context = new DataContext(FileSystem);
        }

        #region Helper Methods

        /// <summary>
        /// Performs a recursive FileCount of the given directory (and it's descendents)
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private int FileCount(IDirectory directory)
        {
            int count = directory.GetFiles().Count;

            // Iterate the child directories
            foreach (IDirectory childDirectory in directory.GetDirectories())
            {
                count += FileCount(childDirectory);
            }

            return count;
        }


        #endregion

    }
}
