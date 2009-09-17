using System;
using System.IO;
using Buttercup.Control.Common.Net;


namespace Buttercup.Control.Common.IO
{
	public class IsolatedStorageFile : IFile
	{
		#region Fields (1) 

		private readonly IFileSystem _fileSystem;

		#endregion Fields 



		#region Constructors (1) 

		public IsolatedStorageFile(IsolatedStorageFileSystem fileSystem, string path)
		{
			_fileSystem = fileSystem;
			FullName = path;
		}

		#endregion Constructors 



		#region Properties (5) 

		public bool Exists
		{
			get
			{
				bool fileExists = _fileSystem.FileExists(FullName);
				return fileExists;
			}
		}

		public string FullName { get; private set; }

		public DateTime LastAccessTimeUtc
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public string Name
		{
			get { return Path.GetFileName(FullName); }
		}

		public IDirectory Parent
		{
			get { return _fileSystem.GetDirectory(IsolatedStorageFileSystem.GetParentDirectory(FullName)); }
		}

		#endregion Properties 



		#region Delegates and Events (1) 

		public event EventHandler<DownloadCompleteEventArgs> OpenAsyncComplete;

		#endregion Delegates and Events 



		#region Methods (7) 

		public void CopyTo(string path)
		{
			throw new NotImplementedException();
		}



		public Stream Create()
		{
			Stream fileStream = _fileSystem.CreateFile(FullName);
			return fileStream;
		}



		public void Delete()
		{
			_fileSystem.DeleteFile(FullName);
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



		public void OpenAsync()
		{
			OpenAsync(null);
		}



		public void OpenAsync(object userState)
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
			catch(Exception e)
			{
				error = e;
			}

			// Raise event if anyone is listening
			if(OpenAsyncComplete != null)
			{
				DownloadCompleteEventArgs eventArgs = new DownloadCompleteEventArgs(stream, error, false, userState);
				OpenAsyncComplete(this, eventArgs);
			}
		}

		#endregion Methods 
	}
}