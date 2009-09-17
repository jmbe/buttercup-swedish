using System;
using System.Collections.Generic;
using System.IO;


namespace Buttercup.Control.Common.IO
{
	public interface IFileSystem : IDisposable
	{
		#region Data Members (2) 

		bool IsBrowsable { get; }

		bool UpdateFileLastAccess { get; set; }

		#endregion Data Members 



		#region Operations (13) 

		string CombinePath(string path1, string path2);

		void CreateDirectory(string directory);

		Stream CreateFile(string path);

		void DeleteDirectory(string path);

		void DeleteFile(string path);

		bool DirectoryExists(string path);

		bool FileExists(string path);

		IList<IDirectory> GetDirectories(string searchPattern);

		IDirectory GetDirectory(string path);

		IFile GetFile(string path);

		IList<IFile> GetFiles(string searchPattern);

		Stream OpenFile(string path, FileMode mode);

		Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share);

		#endregion Operations 
	}
}