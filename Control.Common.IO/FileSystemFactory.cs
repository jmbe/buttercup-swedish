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
using Buttercup.Control.Common.IO;

namespace Buttercup.Control.Common
{
    public class FileSystemFactory
    {

        /// <summary>
        /// Call this to get the FileSystem to use for the application. The returned type
        /// will depend on some configuration mode. For example, in Server based mode, we may
        /// have a ServerFileSystem that knows how to retrieve books from a remote server via
        /// a url.
        /// 
        /// TODO: Consider two stores - one for books (could be IsolatedStorageFileSystem or
        /// ServerFileSystem depending on the application mode). The other store could be for
        /// a user's application settings and preferences and is likey to always be 
        /// IsolatedStorageFileSystem.
        /// </summary>
        public static IFileSystem FileSystem
        {
            get
            {
                // Hardwire to IS for now.
                return IsolatedStorageFileSystem.GetForApplication();
                //return new InMemoryFileSystem();
            }
        }
    }
}
