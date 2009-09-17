using System;
using System.Net;
using System.IO;
using Buttercup.Control.Common.Net;

namespace Buttercup.Control.Common.IO
{
    public class InMemoryFile: IFile
    {
        public InMemoryFile(InMemoryFileSystem fileSystem, string path)
        {
            FullName = path;
            FileSystem = fileSystem;
            Exists = false; // File doesn't exist until Create is called.
            FileBuffer = new byte[900000];
            ContentsStream = new InMemoryFileStream();
            ContentsStream.Disposing += new EventHandler(ContentsStream_Disposing);
            //ContentsStream = new MemoryStream();
            //BinaryReader reader = new BinaryReader(ContentsStream);

        }

        /// <summary>
        /// This event is fired before the stream is disposed and gives us a chance to
        /// save the Stream contents to a local byte array.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ContentsStream_Disposing(object sender, EventArgs e)
        {
            FileBuffer = ContentsStream.ToArray();
        }

        private byte[] FileBuffer { get; set; }
        private InMemoryFileStream ContentsStream { get; set; }
        private InMemoryFileSystem FileSystem { get; set; }

        #region IFile Members

        public string Name
        {
            get { return Path.GetFileName(FullName); }
        }

        public string FullName { get; private set; }

        public void Delete()
        {
            FileSystem.DeleteFile(FullName);
        }

        public bool Exists { get; set; }
        
        public void CopyTo(string path)
        {
            throw new NotImplementedException();
        }

        public IDirectory Parent
        {
            get
            {
                return FileSystem.GetDirectory(Path.GetDirectoryName(FullName));
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Stream Create()
        {
            return FileSystem.CreateFile(FullName);
        }

        public Stream Open(FileMode mode)
        {
            if (!ContentsStream.CanRead)
            {
                // Stream must have been closed so create a new one based on the
                // FileBuffer we have
                ContentsStream = new InMemoryFileStream(FileBuffer);
            }
            ContentsStream.Seek(0, SeekOrigin.Begin);
            return ContentsStream;
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return Open(mode);
        }

        #endregion

        public override string ToString()
        {
            return FullName;
        }

        #region IFile Members (TODO)


        public void OpenAsync()
        {
            OpenAsync(null);
        }

        public void OpenAsync(object userState)
        {
            Exception error = null;
            Stream stream = null;
 
            // This is actually a synchronous call at the moment.
            try
            {
                stream = Open(FileMode.Open);
            }
            catch (Exception e)
            {
                error = e;
            }

            // Raise event if anyone is listening
            if (OpenAsyncComplete != null)
            {
                DownloadCompleteEventArgs eventArgs = new DownloadCompleteEventArgs(stream, error, false, userState);
                OpenAsyncComplete(this, eventArgs);
            }

        }

        public event EventHandler<DownloadCompleteEventArgs> OpenAsyncComplete;

        #endregion
    }
}
