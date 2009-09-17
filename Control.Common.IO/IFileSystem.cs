using System.Collections.Generic;
using System.IO;
using System;
namespace Buttercup.Control.Common.IO
{
    public interface IFileSystem : IDisposable
    {
        IDirectory GetDirectory(string path);
        IFile GetFile(string path);
        IList<IDirectory> GetDirectories(string searchPattern);
        IList<IFile> GetFiles(string searchPattern);
        bool FileExists(string path);
        bool DirectoryExists(string path);
        void DeleteFile(string path);
        void DeleteDirectory(string path);
        Stream CreateFile(string path);
        void CreateDirectory(string directory);
        Stream OpenFile(string path, FileMode mode);
        Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share);

        string CombinePath(string path1, string path2);

        bool UpdateFileLastAccess { get; set; }
    }
}