using System;
using System.IO;
using System.Security.Permissions;
using Buttercup.Control.Common.Net;

namespace Buttercup.Control.Common.IO
{
    public interface IFile
    {
        string Name { get; }
        string FullName { get; }
        void Delete();
        bool Exists { get; }
        void CopyTo(string path);
        IDirectory Parent { get; }
        DateTime LastAccessTimeUtc { get; set; }
        Stream Create();

        void OpenAsync();
        void OpenAsync(string userState);
        event EventHandler<DownloadCompleteEventArgs> OpenAsyncComplete;

        Stream Open(FileMode mode);
        Stream Open(FileMode mode, FileAccess access, FileShare share);
    }
}