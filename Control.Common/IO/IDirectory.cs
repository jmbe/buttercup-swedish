using System.Collections.Generic;


namespace Buttercup.Control.Common.IO
{
	public interface IDirectory
	{
		#region Data Members (4) 

		/// <summary>
		/// Gets a value indicating whether the directory exists.
		/// </summary>
		/// <value>True if the directory exists, otherwise false.</value>
		bool Exists { get; }

		string FullName { get; }

		string Name { get; }

		IDirectory Parent { get; }

		#endregion Data Members 



		#region Operations (9) 

		void CopyTo(string path);

		void Create();

		void Delete();

		IList<IDirectory> GetDirectories();

		IList<IDirectory> GetDirectories(string searchPattern);

		IFile GetFile(string path);

		IList<IFile> GetFiles();

		IList<IFile> GetFiles(string searchPattern);

		IDirectory GetSubdirectory(string path);

		#endregion Operations 
	}
}