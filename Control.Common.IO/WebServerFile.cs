using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Buttercup.Control.Common.Net;

namespace Buttercup.Control.Common.IO
{
    public class WebServerFile: IFile
    {
        public WebServerFile(WebServerFileSystem fileSystem, string path)
        {
            FileSystem = fileSystem;
            FullName = FileSystem.CombinePath(FileSystem.RootUri.ToString(), path);
        }

        WebServerFileSystem FileSystem { get; set; }

        #region IFile Members

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string FullName { get; set; }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public bool Exists
        {
            // TODO:
            get { throw new NotImplementedException(); }
        }

        public void CopyTo(string path)
        {
            throw new NotImplementedException();
        }

        public IDirectory Parent
        {
            get { throw new NotImplementedException(); }
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

        public System.IO.Stream Create()
        {
            throw new NotImplementedException();
        }

        
        void downloader_DownloadComplete(object sender, DownloadCompleteEventArgs e)
        {
            if (OpenAsyncComplete != null)
            {
                OpenAsyncComplete(this, e);
            }
        }
        
        public System.IO.Stream Open(System.IO.FileMode mode)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream Open(System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IFile Members


        public void OpenAsync()
        {
            OpenAsync(null);
        }

        public void OpenAsync(string userState)
        {
            IDownloader downloader = new WebDownloader();
            downloader.DownloadComplete += new EventHandler<DownloadCompleteEventArgs>(downloader_DownloadComplete);
            downloader.DownloadAsync(new Uri(FullName), userState);
        }

        public event EventHandler<DownloadCompleteEventArgs> OpenAsyncComplete;

        #endregion
    }
}
