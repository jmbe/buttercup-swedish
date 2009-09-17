using System;
using System.Collections.Generic;
using System.IO;
using Buttercup.Control.Common;
using Buttercup.Control.Common.Compression;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.Model.Exceptions;
using System.Linq;

namespace Buttercup.Control.Model
{
    public class PackageInspector : IPackageInspector
    {
        private const string PROGRESS_MESSAGE = "Inspecting Package Files...";

        #region Constructors (1)

        /// <summary>
        /// Constructor for the PackageInspector, which is used to create a book package.
        /// </summary>
        public PackageInspector()
        {
            // Until the ProcessPackage is called this flag will remain false
            Processed = false;

            // Initialise the ValidationErrors collection in case we get any errors parsing
            // the file.
            ValidationErrors = new List<string>();
        }

        #endregion Constructors

        #region Properties (4)

        /// <summary>
        /// Verifies whether the zip package contains valid and complete Daisy files.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Occurs if this property is accessed without calling <see cref="ProcessPackage"/>.</exception>
        public bool IsValid
        {
            get
            {
                // To avoid confusion by this method returning 'false' before a zip file has
                // event been processed, let's raise an exception.
                if (!Processed)
                {
                    throw new InvalidOperationException("You must first call ProcessPackage(Stream) method before accessing this property.");
                }
                return ValidationErrors == null || ValidationErrors.Count == 0;
            }
        }

        /// <summary>
        /// The domain representation of a book's package (set of files). 
        /// </summary>
        public IPackage Package { get; set; }

        public Dictionary<string, Stream> PackageFiles { get; set; }

        private bool Processed { get; set; }

        /// <summary>
        /// List of issues that are preventing the package from being loaded.
        /// </summary>
        public List<string> ValidationErrors { get; private set; }

        #endregion Properties

        #region Delegates and Events (1)

        // Events (1) 

        public event EventHandler<ProgressNotificationEventArgs> ProgressChanged;

        #endregion Delegates and Events

        #region Methods (2)

        // Public Methods (1) 

        /// <summary>
        /// Derives information about the zip package and verifies it is valid. The zip file 
        /// must contain all the files required to process the book according to the DAISY 3.0 
        /// standard.
        /// </summary>
        /// <remarks>
        /// This method will raise ProgressChanged events as it parses the zip package. Users
        /// should subscribe to this event before calling this method.
        /// 
        /// This method must be called before the other methods are accessed.
        /// </remarks>
        /// <param name="zipStream">Stream of the zip file.</param>
        /// <returns></returns>
        public void ProcessPackage(Stream zipStream)
        {
            if (zipStream == null || zipStream.Length == 0)
            {
                throw new InvalidPackageException("Stream is null or empty");
            }

            // Initial progress
            DoProgressChanged(50.00, PROGRESS_MESSAGE);

            // Obtain a zip reader implementation
            ZipFileReader zipReader = ZipFileReaderFactory.GetZipFileReader(zipStream);

            // Note, current implementation of CopyToDictionary always returns filenames in lower case
            PackageFiles = zipReader.CopyToDictionary;

            // Check whether this is a version 2.* package
            var q = from f in PackageFiles where f.Key.EndsWith("ncc.html") select f;

            if (q.Count() != 0)
            {
                Package = new Package202(PackageFiles);
            }
            else
            {
                q = from f in PackageFiles where f.Key.EndsWith(".opf") select f;

                if (q.Count() != 0)
                {
                    // Instantiate a DAISY 3.0 Package with the OPF Stream
                    // All other information is derived from this file.
                    Stream opfStream = q.First().Value;
                    Package = new Package300(opfStream);
                }
                else
                {
                    // Unknown package type.
                    throw new InvalidPackageException("We can't determine the version of the DAISY files. Either there is no OPF file (for version 3.0) or no NCC.HTML (for version 2.02).");
                }
            }

            DoProgressChanged(100.00, "Inspecting Package Files...");

            // Successfully processed zip package
            Processed = true;
        }

        // Private Methods (1) 

        /// <summary>
        /// Helper method to raise the ProgressChanged event to anyone who is listening.
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="message"></param>
        private void DoProgressChanged(double percentage, string message)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressNotificationEventArgs(percentage, message));
            }
        }

        #endregion Methods
    }
}