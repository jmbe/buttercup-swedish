using System;
using System.Collections.Generic;


namespace Buttercup.Control.Common.IO
{
	public class WebServerDirectory : IDirectory
	{
		#region Constructors (1) 

		public WebServerDirectory(string path)
		{
			FullName = path;
		}

		#endregion Constructors 



		#region Properties (4) 

		public bool Exists
		{
			get { throw new NotImplementedException(); }
		}

		public string FullName { get; set; }

		public string Name
		{
			get { throw new NotImplementedException(); }
		}

		public IDirectory Parent
		{
			get { throw new NotImplementedException(); }
		}

		#endregion Properties 



		#region Methods (9) 

		// Public Methods (9) 

		public void CopyTo(string path)
		{
			throw new NotImplementedException();
		}



		public void Create()
		{
			throw new NotImplementedException();
		}



		public void Delete()
		{
			throw new NotImplementedException();
		}



		public IList<IDirectory> GetDirectories()
		{
			throw new NotImplementedException();
		}



		public IList<IDirectory> GetDirectories(string searchPattern)
		{
			throw new NotImplementedException();
		}



		public IFile GetFile(string path)
		{
			throw new NotImplementedException();
		}



		public IList<IFile> GetFiles()
		{
			throw new NotImplementedException();
		}



		public IList<IFile> GetFiles(string searchPattern)
		{
			throw new NotImplementedException();
		}



		public IDirectory GetSubdirectory(string path)
		{
			throw new NotImplementedException();
		}

		#endregion Methods 
	}
}