using System;
using System.IO;
using Buttercup.Control.Common.Net;


namespace Buttercup.Control.Common.IO
{
	public interface IFile
	{
		#region Data Members (5) 

		bool Exists { get; }

		string FullName { get; }

		DateTime LastAccessTimeUtc { get; set; }

		string Name { get; }

		IDirectory Parent { get; }

		#endregion Data Members 



		#region Delegates and Events (1) 

		event EventHandler<DownloadCompleteEventArgs> OpenAsyncComplete;

		#endregion Delegates and Events 



		#region Operations (7) 

		void CopyTo(string path);

		Stream Create();

		void Delete();

		Stream Open(FileMode mode);

		Stream Open(FileMode mode, FileAccess access, FileShare share);

		void OpenAsync();

		void OpenAsync(object userState);

		#endregion Operations 
	}
}