using System;
using System.Collections.Generic;
using System.IO;
using Buttercup.Control.Common.Net;


namespace Buttercup.Control.Common.IO
{
    public class WebServerFileSystem : IFileSystem
    {
        private const string CacheDirectory = "ButtercupCache";
        private Dictionary<string, WebServerFile> _cachesInProgress;

		#region Constructors (1) 

        /// <summary>
        /// Default constructor
        /// </summary>
        public WebServerFileSystem()
        {
            InMemoryCache = new InMemoryFileSystem();
            //create a directory in memory for us to store the cached files
            InMemoryCache.CreateDirectory(CacheDirectory);

            _cachesInProgress = new Dictionary<string, WebServerFile>();
        }

		#endregion Constructors 

		#region Properties (3) 

        /// <summary>
        /// The Web Server File System keeps an in memory copy of every book file it downloads in that session,
        /// So that subsequent calls to the same file will not be downloaded again
        /// </summary>
        public InMemoryFileSystem InMemoryCache { get; private set; }


        /// <summary>
        /// This is the only permitted directory separator for this FileSystem.
        /// </summary>
        public string DirectorySeparator
        {
            get
            {
                return "/";
            }
        }

        /// <summary>
        /// WebServerFileSystem files are always browseable. This is used to determine whether, for
        /// example, whether we can set the source of an Image or MediaElement to a url, rather than
        /// having to Stream it (as is the case for non-browseable FileSystems like IsolatedStorageFileSystem).
        /// </summary>
        public bool IsBrowsable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Not used.
        /// </summary>
        public bool UpdateFileLastAccess
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

		#endregion Properties 

		#region Methods (14) 

		// Public Methods (14) 

        /// <summary>
        /// Combines two WebServer file paths.
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public string CombinePath(string path1, string path2)
        {
            string path = path1;

            if (!path1.EndsWith(DirectorySeparator))
            {
                path += DirectorySeparator;
            }

            path += path2;
            return path;
        }

        public void CreateDirectory(string directory)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream CreateFile(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<IDirectory> GetDirectories(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public IDirectory GetDirectory(string path)
        {
            return new WebServerDirectory(path);
        }

        /// <summary>
        /// Get whether the caching for a particular file is in progress or not
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool CachingInProgress(IFile file)
        {
            return (_cachesInProgress.ContainsKey(file.FullName));
        }

        public WebServerFile GetCachingInProgress(string filename)
        {
            return _cachesInProgress[filename];
        }

        /// <summary>
        /// Set whether the caching for a particular file is in progress or not
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="file"></param>
        public void SetCachingInProgress(string filename, WebServerFile file)
        {
            if (file != null)
            {
                _cachesInProgress[filename] = file;
            } else
            {
                _cachesInProgress.Remove(filename);
            }
            
        }

		public IFile GetFile(string path)
		{
            return new WebServerFile(this, path);
		}

		public System.Collections.Generic.IList<IFile> GetFiles(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream OpenFile(string path, System.IO.FileMode mode)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream OpenFile(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share)
        {
            throw new NotImplementedException();
        }

		#endregion Methods 
    
	}
}
