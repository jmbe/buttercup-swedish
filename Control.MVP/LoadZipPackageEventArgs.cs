using System;
using System.IO;

namespace Control.MVP
{
    public class LoadZipPackageEventArgs : EventArgs
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for the LoadZipPackageEventArgs, this EventArgs will be used to pass
        /// a FileStream for a Zip Package.
        /// </summary>
        /// <param name="zipStream"></param>
        public LoadZipPackageEventArgs(FileStream zipStream)
        {
            ZipStream = zipStream;
        }

        #endregion Constructors

        #region Properties (1)

        public FileStream ZipStream { get; set; }

        #endregion Properties
    }
}