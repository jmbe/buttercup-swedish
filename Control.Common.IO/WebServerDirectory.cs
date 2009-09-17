using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Buttercup.Control.Common.IO
{
    public class WebServerDirectory: IDirectory
    {

        #region IDirectory Members

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string FullName
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Generic.IList<IFile> GetFiles()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<IFile> GetFiles(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<IDirectory> GetDirectories()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<IDirectory> GetDirectories(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public void Create()
        {
            throw new NotImplementedException();
        }

        public IDirectory GetSubdirectory(string path)
        {
            throw new NotImplementedException();
        }

        public IFile GetFile(string path)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public bool Exists
        {
            get { throw new NotImplementedException(); }
        }

        public IDirectory Parent
        {
            get { throw new NotImplementedException(); }
        }

        public void CopyTo(string path)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
