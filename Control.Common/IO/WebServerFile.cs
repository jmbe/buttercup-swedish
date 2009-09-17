using System;
using System.IO;
using Buttercup.Control.Common.Helpers;
using Buttercup.Control.Common.Net;
using Wilco.Windows.Browser.Net;
using System.Windows;

namespace Buttercup.Control.Common.IO
{
    public class WebServerFile : IFile
    {
        /// <summary>
        /// We only want the _last_ request for a file download to process that download, this delegate will be overwritten by
        /// whichever file is the latest to request that particular file, so only the latest request will be notified of the download
        /// completion.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region Delegates
        public delegate void NotifyLatestDownloadRequestDelegate(object sender, DownloadCompleteEventArgs e);
        #endregion

        private const string CacheDirectory = "ButtercupCache";
        private Stream _cachedFileStream;
        private IDownloader _downloader;
        private object _userState;
        private string _fullName;

        public NotifyLatestDownloadRequestDelegate NotifyLatestDownloadRequest;

        #region Constructors (1) 
        public WebServerFile(WebServerFileSystem fileSystem, string path)
        {
            FileSystem = fileSystem;

            FullName = path; // FileSystem.CombinePath(FileSystem.RootUri.ToString(), path);
            
            //get the filename from the path, will just have a flat memory structure?
            //TODO better/more secure filename extraction? escaping of characters
            string filename = FileSystem.CombinePath(CacheDirectory, path.Substring(path.LastIndexOf('/') + 1));

            //if the file exists in memory, then set the file stream
            if (FileSystem.InMemoryCache.FileExists(filename))
            {
                IFile cachedFile = FileSystem.InMemoryCache.GetFile(filename);
                _cachedFileStream = cachedFile.Open(FileMode.Open);
            }

        }

        #endregion Constructors 

        private Stream CachedFileStream
        {
            get
            {
                if (_cachedFileStream != null)
                {
                    _cachedFileStream.Position = 0;
                }
                return _cachedFileStream;
            }

            set { _cachedFileStream = value; }
        }

        #region Properties (5) 
        private WebServerFileSystem FileSystem { get; set; }

        public string FullName 
        { 
            get
            {
                return _fullName;
            }
            set
            {
                // Verify the path is Absolute
                if (!Uri.IsWellFormedUriString(Uri.EscapeUriString(value), UriKind.RelativeOrAbsolute))
                {
                    throw new Exception("Book reference is not a valid Uri.");
                }

                Uri uri = new Uri(value, UriKind.RelativeOrAbsolute);
                if (uri.IsAbsoluteUri)
                {
                    _fullName = value;
                }
                else
                {
                    Uri hostUri = Application.Current.Host.Source;

                    _fullName = FileSystem.CombinePath(
                                                hostUri.Scheme +
                                                "://" +
                                                hostUri.DnsSafeHost,
                                                value);
                }
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public IDirectory Parent
        {
            get { throw new NotImplementedException(); }
        }
        #endregion Properties 

        // Events (1) 

        #region IFile Members
        public event EventHandler<DownloadCompleteEventArgs> OpenAsyncComplete;

        public bool Exists
        {
            // TODO:
            get { return true; }
        }
        #endregion

        #region Methods (12) 
        // Public Methods (7) 

        public void CopyTo(string path)
        {
            throw new NotImplementedException();
        }


        public Stream Create()
        {
            throw new NotImplementedException();
        }


        public void Delete()
        {
            throw new NotImplementedException();
        }


        public Stream Open(FileMode mode)
        {
            if (CachedFileStream != null) return CachedFileStream;

            var request = HttpWebRequestEx.Create(new Uri(FullName));
            var sameDomainRequest = (HttpWebRequestEx)request;
            var response = sameDomainRequest.GetResponse();
            return response.GetResponseStream();
        }


        public Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return Open(FileMode.Open);
        }


        public void OpenAsync()
        {
            OpenAsync(null);
        }


        public void OpenAsync(object userState)
        {
            _userState = userState;

            if (FileSystem.CachingInProgress(this))
            {
                //if an existing download is in progress for this file, override it's NotifyLatestDownloadRequest
                //delegate to notify us of the download completion, since we are the last request for that file
                WebServerFile existingDownload = FileSystem.GetCachingInProgress(FullName);
                existingDownload.NotifyLatestDownloadRequest = DownloadComplete;
            }
                //otherwise, if the file has been cached, we already have a filestream and are done
            else if (CachedFileStream != null)
            {
                if (OpenAsyncComplete != null)
                {
                    OpenAsyncComplete(this, new DownloadCompleteEventArgs(CachedFileStream, null, false, userState));
                }
            }
                //as a last resort, we actually download the file
            else
            {
                FileSystem.SetCachingInProgress(FullName, this);

                //set the notifylatest delegate to this file, since we are the only request at this point
                NotifyLatestDownloadRequest = DownloadComplete;

                _downloader = new WebDownloader();
                _downloader.DownloadComplete += downloader_DownloadComplete;
                try
                {
                    _downloader.DownloadAsync(new Uri(FullName), userState);
                }
                catch
                {
                    throw new Exception("The server book reference given is an invalid URL.");
                }
            }
        }

        /// <summary>
        /// Executed when the current file has finished downloading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadComplete(object sender, DownloadCompleteEventArgs e)
        {
            if (OpenAsyncComplete != null)
            {
                OpenAsyncComplete(this, new DownloadCompleteEventArgs(e.Result, e.Error, e.Cancelled, _userState));
            }
        }

        private void downloader_DownloadComplete(object sender, DownloadCompleteEventArgs e)
        {
            _downloader.DownloadComplete -= downloader_DownloadComplete;

            //get the filename from the path, will just have a flat memory structure?
            //TODO better/more secure filename extraction? escaping of characters
            string filename = FileSystem.CombinePath(CacheDirectory, FullName.Substring(FullName.LastIndexOf('/') + 1));

            //if the file doesn't already exist in the cache
            if (!FileSystem.InMemoryCache.FileExists(filename))
            {
                e.Result.Position = 0;

                //we have the file now, so store it in the webfilesystem memory cache
                Stream cacheStream = FileSystem.InMemoryCache.CreateFile(filename);
                FileHelpers.CopyStream(cacheStream, e.Result);
            }

            //remove the completed download from the filesystem dictionary of downloads in progress
            FileSystem.SetCachingInProgress(FullName, null);

            //notify the last download request for this file that they can continue execution
            if (NotifyLatestDownloadRequest != null)
            {
                NotifyLatestDownloadRequest(sender, e);
            }
        }
        #endregion Methods 
    }
}