using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace Buttercup.Control.Common.IO
{
    public class InMemoryDirectory : IDirectory
    {
        private List<IFile> _files;
        IList<IDirectory> _subDirectories;

        public InMemoryDirectory(InMemoryFileSystem fileSystem, string path)
        {
            FullName = path;
            FileSystem = fileSystem;
            Exists = false; // Directory doesn't exist until Create is called.
        }

        private InMemoryFileSystem FileSystem { get; set; }
        
        private IList<IFile> Files 
        {
            get
            {
                if (_files == null)
                {
                    _files = new List<IFile>();
                }
                return _files;
            }
        }

        private IList<IDirectory> SubDirectories 
        {
            get
            {
                if (_subDirectories == null)
                {
                    _subDirectories = new List<IDirectory>();
                }
                return _subDirectories;
            }
            set
            {
                _subDirectories = value;
            }
        }

        #region IDirectory Members

        public string Name
        {
            get { return Path.GetFileName(FullName); }
        }

        public string FullName
        {
            get;
            private set;
        }

        public IList<IFile> GetFiles()
        {
            return Files;
        }

        public IList<IFile> GetFiles(string searchPattern)
        {
            Regex reg = new Regex(Utility.WildcardToRegex(searchPattern).ToLower());
            return Files.Where(f => reg.Match(f.Name.ToLower()).Success).ToList();
        }

        public IList<IDirectory> GetDirectories()
        {
            return SubDirectories;
        }

        public IList<IDirectory> GetDirectories(string searchPattern)
        {
            Regex reg = new Regex(Utility.WildcardToRegex(searchPattern).ToLower());
            return SubDirectories.Where(d => reg.Match(d.Name.ToLower()).Success).ToList();
        }

        public void Create()
        {
            if (!Exists)
            {
                FileSystem.CreateDirectory(FullName);
            }
            else
            {
                throw new Exception("Directory already exists.");
            }
        }

        public IDirectory GetSubdirectory(string path)
        {
            IDirectory directory = null;

            // Search for the directory
            if (SubDirectories != null)
            {
                directory = SubDirectories.SingleOrDefault(d => d.Name == path);
            }

            // Return a virtual directory (Exists = false) if not found
            if (directory == null)
            {
                directory = new InMemoryDirectory(FileSystem, Path.Combine(FullName, path));
            }

            return directory;
        }

        public IFile GetFile(string path)
        {
            return FileSystem.GetFile(Path.Combine(FullName, path));
        }

        public void Delete()
        {
            FileSystem.DeleteDirectory(FullName);
        }

        public bool Exists {get; private set;}
        
        public IDirectory Parent
        {
            get
            {
                IDirectory parent;
                if (!FullName.Contains(Path.DirectorySeparatorChar))
                {
                    parent = (FileSystem as InMemoryFileSystem).Root;
                }
                else
                {
                    string parentPath = FullName.Substring(0, FullName.LastIndexOf(Path.PathSeparator));
                    parent = FileSystem.GetDirectory(parentPath);
                }
                return parent;
            }

        }

        public void CopyTo(string path)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void AddSubDirectory(InMemoryDirectory directory)
        {
            if (SubDirectories == null)
            {
                SubDirectories = new List<IDirectory>();
            }
            SubDirectories.Add(directory);
            directory.Exists = true;
        }

        public Stream AddFile(InMemoryFile file)
        {
            // Check file name doesn't already exist
            if (Files.SingleOrDefault(f => f.Name == file.Name) != null)
            {
                throw new IOException(String.Format("File, {0}, already exists", file.Name));
            }
            Files.Add(file);
            file.Exists = true;
            return file.Open(FileMode.Open);
        }

        public void DeleteFile(InMemoryFile file)
        {
            Files.Remove(file);
        }

        public void DeleteSubDirectory(InMemoryDirectory directory)
        {
            SubDirectories.Remove(directory);
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
