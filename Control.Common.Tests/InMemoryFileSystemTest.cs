using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Buttercup.Control.Common.IO;
using System.IO;
using System.Windows.Resources;
using Buttercup.Control.Common.Net;

namespace Control.Common.Tests
{
    [TestClass]
    public class InMemoryFileSystemTest
    {
        public InMemoryFileSystemTest()
        {
            FileSystem = new InMemoryFileSystem();
        }

        IFileSystem FileSystem { get; set; }

        [TestMethod]
        public void CreateDirectory()
        {
            // Create folders 
            FileSystem.CreateDirectory("Books"); // implied that directory is created from root
            FileSystem.CreateDirectory("/Settings"); // implicit root specification - forward slash
            FileSystem.CreateDirectory(@"/Settings\Users/"); // trailing - mixed slashes OK
            FileSystem.CreateDirectory(@"\Settings\Application"); // no trailing

            Assert.IsTrue(FileSystem.GetDirectory("Books").Exists
                    && FileSystem.GetDirectory("Settings").Exists
                    && FileSystem.GetDirectory("Settings/Users").Exists
                    && FileSystem.GetDirectory("Settings/Application").Exists);

        }

        [TestCleanup]
        public void CleanUp()
        {
            // Remove the root directory
            FileSystem.DeleteDirectory("Books");
        }

        [TestInitialize]
        public void StartUp()
        {
            if (FileSystem == null)
            {
                FileSystem = new InMemoryFileSystem();
            }
        }

        [TestMethod]
        public void GetDirectoriesWildcard()
        {
            // Create 2 folders at the root
            FileSystem.CreateDirectory("Books");
            FileSystem.CreateDirectory("/Books/FirstDirectory"); // root specifier, no trailing /
            FileSystem.CreateDirectory("/Books/SecondDirectory/"); // trailing /
            FileSystem.CreateDirectory("/Books/ThirdDirectory/"); // trailing /

            // Get a reference to the Book directory
            IDirectory books = FileSystem.GetDirectory("Books");
            Assert.IsTrue(books.GetDirectories("*Directory").Count == 3, "*Directory");
            Assert.IsTrue(books.GetDirectories("*Dir*").Count == 3, "*Dir*");
            Assert.IsTrue(books.GetDirectories("*dir*").Count == 3, "*dir*"); // case insensitive
            Assert.IsTrue(books.GetDirectories("First*").Count == 1, "First*");
            Assert.IsTrue(books.GetDirectories("First").Count == 0, "First");

        }

        [TestMethod]
        public void CreateAndWriteToFile()
        {
            byte[] buffer = new byte[1024];

            IFileSystem fileSystem = new Buttercup.Control.Common.IO.InMemoryFileSystem();

            // Should create the directory aswell
            Stream targetStream;
            IFile file;
            // Wrap in using to test that we can still open the file's stream after it
            // has been disposed.
            using (targetStream = fileSystem.CreateFile("Books/Test.txt"))
            {
                file = fileSystem.GetFile("Books/Test.txt");
                Assert.IsTrue(file.Exists);

                // Write the contents of this file to the new file
                StreamResourceInfo info = App.GetResourceStream(new Uri(@"Control.Common.Tests;component/Files/File1.txt", UriKind.Relative));
                Stream sourceStream = info.Stream;

                while (true)
                {
                    int count = sourceStream.Read(buffer, 0, buffer.Length);
                    if (count > 0)
                    {
                        targetStream.Write(buffer, 0, count);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Get the file again and make sure it has the same contents.
            file = fileSystem.GetFile("Books/Test.txt");
            Stream fileStream = file.Open(FileMode.Open);
            StreamReader reader = new StreamReader(fileStream);

            Assert.IsTrue(reader.ReadToEnd() == "This is a test file to load into the InMemoryFileSystem for testing purposes.");
        }

        [TestMethod]
        public void DeleteFiles()
        {
            IFileSystem fileSystem = new Buttercup.Control.Common.IO.InMemoryFileSystem();

            fileSystem.CreateDirectory("Books");
            fileSystem.CreateFile("Books/File1.txt");
            fileSystem.CreateFile("Books/File2.txt");
            fileSystem.CreateFile("Books/File3.txt");
            fileSystem.CreateFile("Books/File4.txt");
            int fileCount1 = fileSystem.GetDirectory("Books").GetFiles().Count;

            IFile file = fileSystem.GetFile("Books/File3.txt");
            file.Delete();
            int fileCount2 = fileSystem.GetDirectory("Books").GetFiles().Count;

            Assert.IsTrue(fileCount1 == 4 && fileCount2 == 3);
        }

        [TestMethod]
        public void DeleteFolders()
        {
            IFileSystem fileSystem = new Buttercup.Control.Common.IO.InMemoryFileSystem();

            fileSystem.CreateDirectory("Books");
            fileSystem.CreateFile("Books/File1.txt");

            fileSystem.DeleteDirectory("Books");

            IDirectory directory = fileSystem.GetDirectory("Books");
            IFile file = fileSystem.GetFile("Books/File1.txt");

            Assert.IsTrue(!directory.Exists && !file.Exists);
        }

        [TestMethod]
        public void AsyncOpen()
        {
            #region Create File (TODO: create as reuseable method

            byte[] buffer = new byte[1024];

            IFileSystem fileSystem = new Buttercup.Control.Common.IO.InMemoryFileSystem();

            // Should create the directory aswell
            Stream targetStream;
            IFile file;
            // Wrap in using to test that we can still open the file's stream after it
            // has been disposed.
            using (targetStream = fileSystem.CreateFile("Books/Test.txt"))
            {
                file = fileSystem.GetFile("Books/Test.txt");
                Assert.IsTrue(file.Exists);

                // Write the contents of this file to the new file
                StreamResourceInfo info = App.GetResourceStream(new Uri(@"Control.Common.Tests;component/Files/File1.txt", UriKind.Relative));
                Stream sourceStream = info.Stream;

                while (true)
                {
                    int count = sourceStream.Read(buffer, 0, buffer.Length);
                    if (count > 0)
                    {
                        targetStream.Write(buffer, 0, count);
                    }
                    else
                    {
                        break;
                    }
                }
            }


            #endregion

            file = fileSystem.GetFile("Books/Test.txt");
            file.OpenAsyncComplete +=new EventHandler<DownloadCompleteEventArgs>(file_OpenAsyncComplete);  //+= new EventHandler<DownloadCompleteEventArgs>(file_OpenAsyncComplete);
            file.OpenAsync();

            Assert.IsTrue(true);
        }

        void file_OpenAsyncComplete(object sender, DownloadCompleteEventArgs e)
        {
            Assert.IsTrue(e.Result != null);
        }
    }
}
