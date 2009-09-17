using System;
using System.Collections.Generic;
using System.IO;


namespace Buttercup.Control.Common.IO
{
	public class IsolatedStorageDirectory : IDirectory
	{
		#region Fields (1) 

		private readonly IsolatedStorageFileSystem _fileSystem;

		#endregion Fields 



		#region Constructors (1) 

		public IsolatedStorageDirectory(IsolatedStorageFileSystem fileSystem, string path)
		{
			_fileSystem = fileSystem;
			FullName = path;
		}

		#endregion Constructors 



		#region Properties (4) 

		public bool Exists
		{
			get { return _fileSystem.DirectoryExists(FullName); }
		}

		public string FullName { get; private set; }

		public string Name
		{
			get { return Path.GetFileName(FullName); }
		}

		public IDirectory Parent
		{
			get { return _fileSystem.GetDirectory(IsolatedStorageFileSystem.GetParentDirectory(FullName)); }
		}

		#endregion Properties 



		#region Methods (11) 

		// Public Methods (11) 

		public void CopyTo(string path)
		{
			throw new NotImplementedException();
		}



		public void Create()
		{
			_fileSystem.CreateDirectory(FullName);
		}



		public Stream CreateFile(string path)
		{
			return _fileSystem.CreateFile(_fileSystem.CombinePath(FullName, path));
		}



		public void CreateSubdirectory(string path)
		{
			_fileSystem.CreateDirectory(_fileSystem.CombinePath(FullName, path));
		}



		public void Delete()
		{
			_fileSystem.DeleteDirectory(FullName);
		}



		public IList<IDirectory> GetDirectories()
		{
			return GetDirectories("*");
		}



		public IList<IDirectory> GetDirectories(string searchPattern)
		{
			return _fileSystem.GetDirectories(_fileSystem.CombinePath(FullName, searchPattern));
		}



		public IFile GetFile(string path)
		{
			return _fileSystem.GetFile(_fileSystem.CombinePath(FullName, path));
		}



		public IList<IFile> GetFiles()
		{
			return GetFiles("*");
		}



		public IList<IFile> GetFiles(string searchPattern)
		{
			string fullSearchPattern = _fileSystem.CombinePath(FullName, searchPattern);
			return _fileSystem.GetFiles(fullSearchPattern);
		}



		public IDirectory GetSubdirectory(string path)
		{
			return _fileSystem.GetDirectory(_fileSystem.CombinePath(FullName, path));
		}

		#endregion Methods 
	}
}