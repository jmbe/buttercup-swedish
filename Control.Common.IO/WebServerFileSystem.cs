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
    public class WebServerFileSystem : IFileSystem
    {

        #region Constructor

        public WebServerFileSystem(Uri rootUri)
        {
            RootUri = rootUri;
        }

        #endregion

        #region Properties

        public Uri RootUri { get; private set; }

        public string DirectorySeparator
        {
            get
            {
                return "/";
            }
        }

        private Uri AbsolutePath(string relativePath)
        {
            return new Uri(CombinePath(RootUri.ToString(), relativePath));
        }

        /// <summary>
        /// For this file system the path must be a valid - relative - url
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool PathValid(string path)
        {
            // Make sure it's a valid relative path
            Uri uri = new Uri(path);

            // Make sure it's a valid absolute path
            uri = new Uri(CombinePath(RootUri.ToString(), path));

            if (
                (!uri.IsAbsoluteUri) ||
                (uri.Scheme != Uri.UriSchemeHttp || uri.Scheme != Uri.UriSchemeHttps)
            )
            {
                throw new UriFormatException("The path must be relative to the RootUri and use the http/https protocols.");
            }
            
            return true;
        }

        #endregion

        #region IFileSystem Members

        public IFile GetFile(string path)
        {
            return new WebServerFile(this, path);
        }

        public IDirectory GetDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<IDirectory> GetDirectories(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<IFile> GetFiles(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            // Can do this one
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream CreateFile(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string directory)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream OpenFile(string path, System.IO.FileMode mode)
        {
            // Can do this one
            throw new NotImplementedException();
        }

        public System.IO.Stream OpenFile(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share)
        {
            // Can do this one
            throw new NotImplementedException();
        }

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

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            // Can do this one
            throw new NotImplementedException();
        }

        #endregion
    }
}
