using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Permissions;

namespace Buttercup.Control.Common.IO
{
  public interface IDirectory
  {
    string Name { get; }
    string FullName { get; }
    IList<IFile> GetFiles();
    IList<IFile> GetFiles(string searchPattern);
    IList<IDirectory> GetDirectories();
    IList<IDirectory> GetDirectories(string searchPattern);
    void Create();
    IDirectory GetSubdirectory(string path);
    IFile GetFile(string path);
    void Delete();
    bool Exists { get; }
    IDirectory Parent { get; }
    void CopyTo(string path);
  }
}