using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.IO;

namespace Buttercup.Control.Common.IO
{
	public class IsolatedStorageDirectory : IDirectory
	{
		private readonly IsolatedStorageFileSystem _fileSystem;

		public IsolatedStorageDirectory(IsolatedStorageFileSystem fileSystem, string path)
		{
			_fileSystem = fileSystem;
			FullName = path;
		}

		public string Name
		{
			get { return Path.GetFileName(FullName); }
		}

		public string FullName { get; private set; }

		public IList<IFile> GetFiles()
		{
			return GetFiles("*");
		}

		public IList<IFile> GetFiles(string searchPattern)
		{
			string fullSearchPattern = _fileSystem.CombinePath(FullName, searchPattern);
			return _fileSystem.GetFiles(fullSearchPattern);
		}

		public IList<IDirectory> GetDirectories()
		{
			return GetDirectories("*");
		}

		public IList<IDirectory> GetDirectories(string searchPattern)
		{
			return _fileSystem.GetDirectories(_fileSystem.CombinePath(FullName, searchPattern));
		}

		public void CreateSubdirectory(string path)
		{
			_fileSystem.CreateDirectory(_fileSystem.CombinePath(FullName, path));
		}

		public Stream CreateFile(string path)
		{
			return _fileSystem.CreateFile(_fileSystem.CombinePath(FullName, path));
		}

		public void Create()
		{
			_fileSystem.CreateDirectory(FullName);
		}

		public void Delete()
		{
			_fileSystem.DeleteDirectory(FullName);
		}

		public bool Exists
		{
			get { return _fileSystem.DirectoryExists(FullName); }
		}

		public IDirectory Parent
		{
			get { return _fileSystem.GetDirectory(IsolatedStorageFileSystem.GetParentDirectory(FullName)); }
		}

		public void CopyTo(string path)
		{
			throw new NotImplementedException();
		}

		public IDirectory GetSubdirectory(string path)
		{
			return _fileSystem.GetDirectory(_fileSystem.CombinePath(FullName, path));
		}

		public IFile GetFile(string path)
		{
			return _fileSystem.GetFile(_fileSystem.CombinePath(FullName, path));
		}
	}
}