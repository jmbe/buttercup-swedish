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
using System.Linq;
using System.Text.RegularExpressions;

namespace Buttercup.Control.Common.IO
{
    public class InMemoryFileSystem: IFileSystem
    {
        
        public InMemoryFileSystem()
        {
            // Create a new Root directory
            Root = new InMemoryDirectory(this, "");
        }

        public InMemoryDirectory Root { get; set; }
        
        public char[] DirectorySeparators
        {
            get
            {
                return new char[] { '\u005c', '/' };
            }
        }

        #region IFileSystem Members

        public IDirectory GetDirectory(string path)
        {
            IDirectory directory = Root;

            if (path.Length != 0 && path != @"\")
            {
                foreach (string directoryName in path.Split(DirectorySeparators))
                {
                    directory = directory.GetSubdirectory(directoryName);
                    if (!directory.Exists) 
                    {
                        break; // no point going further the directory doesn't exist
                    }
                }
            }

            // The directory wasn't found - so return a blank directory for the path
            if (!directory.Exists)
            {
                directory = new InMemoryDirectory(this, path);
            }
            return directory;

        }

        public IFile GetFile(string path)
        {
            IFile file = null;
            IDirectory directory = GetDirectory(Path.GetDirectoryName(path));
            if (directory.Exists)
            {
				string soughtFileName = Path.GetFileName(path).ToLower();
                file = directory.GetFiles().SingleOrDefault(f => f.Name.ToLower() == soughtFileName);
            }

            // If the requested directory doesn't exist or the file isn't in the directory then
            // return a blank file (Exists == false)
            if (!directory.Exists || file == null)
            {
                file = new InMemoryFile(this, path);
            }
            return file;
        }

        public IList<IDirectory> GetDirectories(string searchPattern)
        {
            return Root.GetDirectories(searchPattern);
        }

        public IList<IFile> GetFiles(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            return false;
        }

        public void DeleteFile(string path)
        {
            InMemoryDirectory directory = GetDirectory(Path.GetDirectoryName(path)) as InMemoryDirectory;
            InMemoryFile file = GetFile(path) as InMemoryFile;
            if (file.Exists)
            {
                directory.DeleteFile(file);
            }
        }

        public void DeleteDirectory(string path)
        {
            // Get the parent of the directory
            InMemoryDirectory directory = GetDirectory(path) as InMemoryDirectory;
            InMemoryDirectory parent = directory.Parent as InMemoryDirectory;
            parent.DeleteSubDirectory(directory);
        }

        public Stream CreateFile(string path)
        {
            string directoryPath = Path.GetDirectoryName(path);

            // Make sure the directories are created first.
            CreateDirectory(directoryPath);

            InMemoryDirectory directory = GetDirectory(directoryPath) as InMemoryDirectory;

            InMemoryFile file = new InMemoryFile(this, path);
            return directory.AddFile(file);
        }

        public void CreateDirectory(string directory)
        {
            InMemoryDirectory dir = Root;
            InMemoryDirectory newDir;

            // Make sure all the directory and all its ancestors exist
            foreach (string directoryName in directory.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!dir.GetSubdirectory(directoryName).Exists)
                {
                    // Create a new directory
                    newDir = new InMemoryDirectory(this, Path.Combine(dir.FullName, directoryName));
                    dir.AddSubDirectory(newDir);
                }
                else
                {
                    dir = dir.GetSubdirectory(directoryName) as InMemoryDirectory;
                }
            }
        }

        public Stream OpenFile(string path, System.IO.FileMode mode)
        {
            if (!FileExists(path))
                throw new IOException(string.Format("File {0} doesn't exist.", path));

            IFile file = GetFile(path);
            return file.Open(mode);
        }

        public Stream OpenFile(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share)
        {
            return OpenFile(path, mode);
        }

        public string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public bool UpdateFileLastAccess
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
