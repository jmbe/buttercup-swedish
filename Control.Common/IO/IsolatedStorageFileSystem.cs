using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Store = System.IO.IsolatedStorage.IsolatedStorageFile;


namespace Buttercup.Control.Common.IO
{
	public class IsolatedStorageFileSystem : IFileSystem
	{
		#region Fields (1) 

		private readonly Store _store;

		#endregion Fields 



		#region Constructors (1) 

		public IsolatedStorageFileSystem(Store store)
		{
			_store = store;
		}

		#endregion Constructors 



		#region Properties (2) 

		public bool IsBrowsable
		{
			get { return false; }
		}

		public bool UpdateFileLastAccess
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		#endregion Properties 



		#region Methods (16) 

		public string CombinePath(string path1, string path2)
		{
			return Path.Combine(path1, path2);
		}



		public void CreateDirectory(string path)
		{
			if(DirectoryExists(path))
			{
				throw new IOException(string.Format("Directory {0} already exists.", path));
			}

			_store.CreateDirectory(path);
		}



		public Stream CreateFile(string path)
		{
			if(FileExists(path))
			{
				throw new IOException(string.Format("File {0} already exists.", path));
			}

			IFile file = GetFile(path);
			if(!file.Parent.Exists)
			{
				file.Parent.Create();
			}

			return _store.CreateFile(path);
		}



		public void DeleteDirectory(string path)
		{
			if(!DirectoryExists(path))
			{
				throw new IOException(string.Format("Directory {0} doesn't exist.", path));
			}

			IDirectory directory = GetDirectory(path);
			foreach(IDirectory subDirectory in directory.GetDirectories())
				subDirectory.Delete();
			foreach(IFile file in directory.GetFiles())
				file.Delete();

			_store.DeleteDirectory(path);
		}



		public void DeleteFile(string path)
		{
			if(!FileExists(path))
			{
				throw new IOException(string.Format("File {0} doesn't exist.", path));
			}

			_store.DeleteFile(path);
		}



		public bool DirectoryExists(string path)
		{
			return _store.DirectoryExists(path);
		}



		public bool FileExists(string path)
		{
			bool fileExists = false;
			fileExists = _store.FileExists(path);

			return fileExists;
		}



		public IList<IDirectory> GetDirectories(string searchPattern)
		{
			string searchRoot = Path.GetDirectoryName(searchPattern);

			return _store.GetDirectoryNames(searchPattern).Select(n => new IsolatedStorageDirectory(this, Path.Combine(searchRoot, n))).Cast<IDirectory>().ToList();
		}



		public IDirectory GetDirectory(string path)
		{
			return new IsolatedStorageDirectory(this, path);
		}



		public IFile GetFile(string path)
		{
			return new IsolatedStorageFile(this, path);
		}



		/// <summary>
		/// Gets the files that fit the given search pattern.
		/// </summary>
		/// <param name="searchPattern">The search pattern.</param>
		/// <returns>The list of files found to meet the given pattern.</returns>
		/// <remarks>The search is case-insensitive.</remarks>
		public IList<IFile> GetFiles(string searchPattern)
		{
			string searchRoot = GetParentDirectory(searchPattern);

			string[] fileNames = _store.GetFileNames(searchPattern);
			return fileNames.Select(n => new IsolatedStorageFile(this, Path.Combine(searchRoot, n)))
				.Cast<IFile>().ToList();
		}



		public static IsolatedStorageFileSystem GetForApplication()
		{
		    Store fileStore = Store.GetUserStoreForSite();

			return new IsolatedStorageFileSystem(fileStore);
		}



		public static string GetParentDirectory(string path)
		{
			string parentDirectoryPath = string.Empty;

			int finalPathSeperatorIndex = path.LastIndexOfAny(new[] {'\\', '/'});

			if(finalPathSeperatorIndex != -1)
			{
				//if (finalPathSeperatorIndex + 1 == path.Length)
				//{
				//  path = path.Substring(0, finalPathSeperatorIndex);
				//  finalPathSeperatorIndex = path.LastIndexOfAny(new[] { '\\', '/' });
				//}

				parentDirectoryPath = path.Substring(0, finalPathSeperatorIndex);
			}

			return parentDirectoryPath;
		}



		public Stream OpenFile(string path, FileMode mode)
		{
			if(!FileExists(path))
			{
				throw new IOException(string.Format("File {0} doesn't exist.", path));
			}

			return _store.OpenFile(path, mode);
		}



		public Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
		{
			if(!FileExists(path))
			{
				throw new IOException(string.Format("File {0} doesn't exist.", path));
			}

			return _store.OpenFile(path, mode, access, share);
		}



		// Private Methods (1) 

		void IDisposable.Dispose()
		{
			_store.Dispose();
		}

		#endregion Methods 
	}
}