using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Buttercup.Control.Common;
using Buttercup.Control.Common.Compression;
using Buttercup.Control.Common.IO;
using Buttercup.Control.Common.Net;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.Common.Helpers;

namespace Buttercup.Control.Model
{
    /// <summary>
    /// This class is the central access point to create and persist domain objects. 
    /// It is instantiated with an IFileSystem implementation that will determine where
    /// the data comes from (i.e. IsolatedStorage or InMemory)
    /// </summary>
    public class DataContext : IObservableProgress
    {
        #region Fields (3)

        private readonly Book _book = new Book();
        private double _totalFiles;
        private const string BOOK_DIR = "Books";

        #endregion Fields

        #region Constructors (1)

        /// <summary>
        /// Constructor initialises this datacontext with a filesystem for it to use
        /// </summary>
        /// <param name="fileSystem"></param>
        public DataContext(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        #endregion Constructors

        #region Properties (2)

        /// <summary>
        /// Private reference to the FileSystem to use.
        /// </summary>
        private IFileSystem FileSystem { get; set; }

        /// <summary>
        /// The directory that all book directories are saved in to.
        /// </summary>
        private IDirectory RootBookDirectory
        {
            get
            {
                IDirectory directory = FileSystem.GetDirectory(BOOK_DIR);
                if (!directory.Exists)
                {
                    throw new Exception(String.Format("Root book directory does not exist, {0}.", BOOK_DIR));
                }
                return directory;
            }
        }

        #endregion Properties

        #region Delegates and Events (2)

        // Events (2) 

        /// <summary>
        /// Raised when the book has been loaded and created, so we may begin playing it, displaying
        /// book information etc.
        /// </summary>
        public event EventHandler<BookCreateEventArgs> BookCreated;

        /// <summary>
        /// Raises this event when progress has been made in loading a book, notifies the application
        /// presenter so that the main view can be updated with the progress
        /// </summary>
        public event EventHandler<ProgressNotificationEventArgs> ProgressChanged;

        #endregion Delegates and Events

        #region Methods (13)

        // Public Methods (5) 

        /// <summary>
        /// Retrieves a book object representing the book. This is based on the dtbook.xml file.
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public Book GetBook(string bookId)
        {
            Book book = null;

            // Get the dtbook xml
            IDirectory bookDirectory = GetBookFolder(bookId);
            if (bookDirectory != null && bookDirectory.Exists)
            {
                IPackage parentPackage = GetPackage(bookId);

                if (parentPackage == null)
                {
                    throw new ArgumentException("No package exists for the given book ID");
                }

                IList<IFile> dtBookFile = bookDirectory.GetFiles(parentPackage.DtBookXmlPath);

                if (dtBookFile.Count == 1)
                {
                    // Set the Xml on the book. Most other properties are derived from this.
                    book = new Book();
                    book.Xml = XDocument.Load(dtBookFile[0].Open(FileMode.Open, FileAccess.Read, FileShare.Read));
                    book.FolderPath = bookDirectory;


                    // Set table of contents (no need for lazy loading as this is too expensive
                    // to do)
                    TableOfContents toc;
                    PageList pages;

                    GetNavStructures(bookId, out toc, out pages);

                    book.TableOfContents = toc;
                    book.PageList = pages;
                }
            }

            return book;
        }

        /// <summary>
        /// Get's a book from a package reference asyncronously. The BookCreated event will 
        /// be raised when the Book is available.
        /// </summary>
        /// <param name="package"></param>
        public void GetBookAsync(IPackage package)
        {
            IFile dtBookFile;
            //If the Book Folder for the package has been defined (for example with server books), then use the folder
            //otherwise, call GetBookFolder to try and find the book folder
            if (package.BookFolder != null)
            {
                dtBookFile = FileSystem.GetFile(FileSystem.CombinePath(package.BookFolder, package.DtBookXmlPath));
            }
            else
            {
                dtBookFile = FileSystem.GetFile(FileSystem.CombinePath(GetBookFolder(package.BookId).FullName, package.DtBookXmlPath));//
            }
            dtBookFile.OpenAsyncComplete += dtBookFile_OpenAsyncComplete;
            if (dtBookFile.Exists)
            {
                dtBookFile.OpenAsync(package);
            }
            else
            {
                throw new Exception("Cannot find book xml");
            }
        }

        /// <summary>
        /// Returns a navigational structures for a book. This is based on the ncx.xml file.
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="toc"></param>
        /// <param name="pageList"></param>
        /// <returns></returns>
        public void GetNavStructures(string bookId, out TableOfContents toc, out PageList pageList)
        {
            toc = null;
            pageList = null;

            // Get the dtbook xml
            IDirectory bookDirectory = GetBookFolder(bookId);
            IList<IFile> ncxFile = bookDirectory.GetFiles(GetPackage(bookId).NcxXmlPath);

            if (ncxFile.Count == 1)
            {
                // Set the Xml on the TOC. All other properties are derived from this.
                XDocument tocXml = XDocument.Load(ncxFile[0].Open(FileMode.Open, FileAccess.Read, FileShare.Read));
                toc = new TableOfContents(tocXml);
                pageList = new PageList(tocXml);
            }
            // It's OK not to have a TOC. Let the null get returned if no ncxFile found.
        }

        /// <summary>
        /// Gets package information for a book.
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public IPackage GetPackage(string bookId)
        {
            // TODO: Create a Dictionary for each bookid to cache the results and prevent 
            // recalculating each time.
            IPackage package = null;

            IDirectory bookDirectory = GetBookFolder(bookId);

            if (bookDirectory != null && bookDirectory.Exists)
            {
                // TODO: cater for OPF (upper case)
                IList<IFile> files = bookDirectory.GetFiles("*.opf");
                if (files.Count == 1)
                {
                    // TODO: Determine whether v2 or v3 and instantiate the appropriate IPackage
                    Stream opfStream = files[0].Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    package = new Package300(opfStream);
                }
                else if (files.Count == 0)
                {
                    throw new Exception("OPF file not found in root directory, " + bookDirectory.FullName);
                }
                else if (files.Count > 1)
                {
                    // It is not legal to have more than one OPF
                    throw new Exception("More than 1 OPF file");
                }
            }
            // If no book directory is found, then do nothing, a null Package will be returned.

            return package;
        }

        /// <summary>
        /// Loads a zip package into a FileSystem
        /// </summary>
        /// <param name="inspector"></param>
        /// <param name="replace"></param>
        /// <returns>The directory into which the files have been loaded.</returns>
        public IDirectory LoadZipPackage(IPackageInspector inspector, bool replace)
        {
            DoProgressChanged(0, "Book Load Started...");

            // All books are saved to a directory called Books
            IDirectory booksDirectory = FileSystem.GetDirectory(BOOK_DIR);
            if (!booksDirectory.Exists)
            {
                booksDirectory.Create();
            }

            // Get the directory for the book (whether it exists or not something will be returned)
            IDirectory newBookDirectory = booksDirectory.GetSubdirectory(inspector.Package.BookId);

            _totalFiles = inspector.Package.FileCount;

            if (replace && newBookDirectory.Exists)
            {
                newBookDirectory.Delete();
            }

            // Notify any listeners that we are about to start.
            DoProgressChanged(10, "Unzip started...");

            // If you get this far and there isn't a newBookDirectory then you need to recreate
            // it and unzip to it. If it does exist then it will contain the files already.
            if (!newBookDirectory.Exists)
            {
                // Create the directory for the book files.
                newBookDirectory.Create();

                // Unzip files
                UnzipFiles(inspector.PackageFiles, newBookDirectory);
            }

            DoProgressChanged(80, "Reading book from store...");

            // To get the changes we need to re-read the newBookDirectory
            //inspector.Package.BookFolder = newBookDirectory.FullName;
            IDirectory bookDirectory = FileSystem.GetDirectory(newBookDirectory.FullName);

            DoProgressChanged(100, "Done...");

            return bookDirectory;
        }
        // Private Methods (8) 

        /// <summary>
        /// Converts all of the image tags in the bookXml to valid paths depending on the filesystem being used.
        /// </summary>
        private void ConvertImagePaths()
        {
            foreach (XElement imageElement in _book.Xml.Descendants(_book.xmlNamespace + "img"))
            {
                XAttribute imgSrc = imageElement.Attribute("src");
                if (imgSrc != null)
                {
                    string src = imgSrc.Value;
                    imgSrc.Value = FileSystem.CombinePath(_book.FolderPath.FullName, src);
                }
            }
        }

        /// <summary>
        /// Raise event to notify others that progress has been made in loading the book
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="message"></param>
        private void DoProgressChanged(int percentage, string message)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressNotificationEventArgs(percentage, message));
            }
        }

        /// <summary>
        /// Asynchronously load the Xml for the book, then begin loading the book table of contents/navigation points
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtBookFile_OpenAsyncComplete(object sender, DownloadCompleteEventArgs e)
        {
            //Remove the event
            //This prevents a file from getting opened twice causing an exception when trying to read
            //the stream of the document since the file has already been opened and is currently stored 
            //in memory.
            ((IFile)sender).OpenAsyncComplete -= dtBookFile_OpenAsyncComplete;

            if (e.Error == null)
            {
                IPackage package = e.UserState as IPackage;

                if (package != null)
                {
                    // TODO: Make member variable for _book                
                    _book.Xml = XDocument.Load(e.Result);
                    //If the Book Folder for the package has been defined (for example with server books), then use the folder
                    //otherwise, call GetBookFolder to try and find the book folder
                    if (package.BookFolder != null)
                    {
                        _book.FolderPath = FileSystem.GetDirectory(package.BookFolder);
                    }
                    else
                    {
                        _book.FolderPath = FileSystem.GetDirectory(GetBookFolder(package.BookId).FullName);
                    }
                    
                    _book.IsBrowseable = FileSystem.IsBrowsable;

                    // If the FileSystem is browsable we may need to convert the image src paths
                    if (FileSystem.IsBrowsable)
                    {
                        ConvertImagePaths();
                    }

                    GetTableOfContentsAsync(e.UserState as IPackage);
                }
            }

            // TODO: What to do if there is an error?
        }

        /// <summary>
        /// Retrieve the directory for a book of given id. Only two situations are handled
        /// (1) The book files are located in the root directory.
        /// (2) The book files are located in a single folder in the root directory.
        /// Any other variation is currently _unsupported_.
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        private IDirectory GetBookFolder(string bookId)
        {
            string rootDirectory;
            string subDirectory = null;

            // Normally it's directly under the Root directory but sometimes there is a single
            // folder in this that then contains the book files. We will cope with this but
            // no other variation.
            IDirectory folder = RootBookDirectory.GetSubdirectory(bookId);
            rootDirectory = folder.FullName;

            if (folder.Exists)
            {
                // Check whether there are any files in this folder. If there aren't then check if
                // there is a single folder within this folder. Then check that this folder has
                // files.
                IList<IFile> files = folder.GetFiles();
                if (files.Count == 0)
                {
                    IList<IDirectory> folders = folder.GetDirectories();

                    if (folders.Count == 1)
                    {
                        folder = folders[0];
                        subDirectory = folder.FullName;
                        if (folder.GetFiles().Count == 0)
                        {
                            // This isn't the right folder either - we screwed, this couldn't have
                            // been a valid zip package.
                            folder = null;
                        }
                    }
                    else
                    {
                        // We only accept a single folder - any more and this indicates an invalid
                        // structure.
                        folder = null;
                    }
                }
            }
            if (folder == null || !folder.Exists)
            {
                throw new Exception(String.Format("The book's files can not be found in the folders, {0}, or, {1}", rootDirectory, subDirectory));
            }
            return folder;
        }

        /// <summary>
        /// Asynchronously load the contents of the dtBook NCX file, the book created event
        /// will be raised after the ncx file is loaded and processed.
        /// </summary>
        /// <param name="package"></param>
        private void GetTableOfContentsAsync(IPackage package)
        {
            IFile ncxFile;
            //If the Book Folder for the package has been defined (for example with server books), then use the folder
            //otherwise, call GetBookFolder to try and find the book folder
            if (package.BookFolder != null)
            {
                ncxFile = FileSystem.GetFile(FileSystem.CombinePath(package.BookFolder, package.NcxXmlPath));
            } else
            {
                ncxFile = FileSystem.GetFile(FileSystem.CombinePath(GetBookFolder(package.BookId).FullName, package.NcxXmlPath));
            }
            ncxFile.OpenAsyncComplete += ncxFile_OpenAsyncComplete;
            if (ncxFile.Exists)
            {
                ncxFile.OpenAsync();
            } else
            {
                throw new Exception("Cannot find ncx");
            }
        }

        private void ncxFile_OpenAsyncComplete(object sender, DownloadCompleteEventArgs e)
        {
            //Remove the event
            //This prevents a file from getting opened twice causing an exception when trying to read
            //the stream of the document since the file has already been opened and is currently stored 
            //in memory.
            ((IFile)sender).OpenAsyncComplete -= ncxFile_OpenAsyncComplete;

            XDocument xml;
            try
            {
                xml = XDocument.Load(e.Result);
            }
            catch (Exception ex)
            {
                Logger.Log("Stream length = " + e.Result.Length + "; Error: " + ex.ToString());
                throw;
            }
            _book.TableOfContents = new TableOfContents(xml);
            _book.PageList = new PageList(xml);

            // This is the last asynchronous operation - we can tell listeners (our view) that
            // the book has been created now.
            if (BookCreated != null)
            {
                BookCreated(this, new BookCreateEventArgs(_book));
            }
        }

        /// <summary>
        /// Unzips a stream of data that represents a zip file and copies the files
        /// to the selected target directory.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="targetDirectory"></param>
        private void UnzipFiles(Dictionary<string, Stream> files, IDirectory targetDirectory)
        {
            int totalFilesRead = 0;

            //Read the local headers until no more are found
            foreach (KeyValuePair<string, Stream> item in files)
            {
                string fullPath = item.Key.Replace('/', '\\');
                IFile newFile = targetDirectory.GetFile(fullPath);

                Stream fileStream;
                using (fileStream = newFile.Create())
                {
                    FileHelpers.CopyStream(fileStream, item.Value);
                }
                
                totalFilesRead += 1;
                
                DoProgressChanged(Convert.ToInt32(totalFilesRead / _totalFiles * 100), "Saving book to local storage...");
            }

            DoProgressChanged(100, "Done.");
        }

        #endregion Methods
    }
}