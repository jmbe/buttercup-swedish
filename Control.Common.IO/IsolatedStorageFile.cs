using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Path=System.IO.Path;
using Buttercup.Control.Common.Net;

namespace Buttercup.Control.Common.IO
{
  public class IsolatedStorageFile : IFile
  {
    private readonly IFileSystem _fileSystem;

    public IsolatedStorageFile(IsolatedStorageFileSystem fileSystem, string path)
    {
      _fileSystem = fileSystem;
      FullName = path;
    }

    public string Name
    {
      get { return Path.GetFileName(FullName); }
    }

    public string FullName { get; private set; }

    public void Delete()
    {
      _fileSystem.DeleteFile(FullName);
    }

    public bool Exists
    {
		get
		{
			bool fileExists = _fileSystem.FileExists(FullName);
			return fileExists;
		}
    }

    public void CopyTo(string path)
    {
      throw new NotImplementedException();
    }

    public IDirectory Parent
    {
      get { return _fileSystem.GetDirectory(IsolatedStorageFileSystem.GetParentDirectory(FullName)); }
    }

    public DateTime LastAccessTimeUtc
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public Stream Create()
    {
		Stream fileStream = _fileSystem.CreateFile(FullName);
		return fileStream;
    }

    public Stream Open(FileMode mode)
    {
		Stream fileStream = _fileSystem.OpenFile(FullName, mode);
		return fileStream;
    }

    public Stream Open(FileMode mode, FileAccess access, FileShare share)
    {
		Stream fileStream = _fileSystem.OpenFile(FullName, mode, access, share);
		return fileStream;
    }

    #region IFile Members (TODO)

    public void OpenAsync()
    {
        OpenAsync(null);
    }

    public void OpenAsync(string userState)
    {
        // Not really implemented asynchronously at the moment. We wanted to have this to make
        // opening files from web servers the same process as reading local/inmemory files. Web
        // resources can only accessed asynchronously (via WebClient)
        Exception error = null;
        Stream stream = null;
 
        try
        {
            stream = Open(FileMode.Open, FileAccess.Read, FileShare.Read);
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

    #region IFile Members


   
    #endregion
  }
}
