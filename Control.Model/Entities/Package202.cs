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
using System.IO;
using System.Collections.Generic;
using Buttercup.Control.Model.Exceptions;
using System.Linq;
using System.Xml.Linq;

namespace Buttercup.Control.Model.Entities
{
    /// <summary>
    /// Concrete implementation of IPackage for the DAISY 2.02 format
    /// </summary>
    public class Package202: IPackage
    {
		#region Fields (1) 

        private const string FILENAME_NCC = "ncc.html";
        private string _dtBookXmlPath = null;
        private string _bookId = null;

		#endregion Fields 

		#region Constructors (1) 

        /// <summary>
        /// Instantiate package based on a list of files
        /// </summary>
        /// <param name="packageFiles"></param>
        public Package202(Dictionary<string, Stream> packageFiles)
        {
            PackageFiles = packageFiles;
        }

		#endregion Constructors 

		#region Properties (6) 

        /// <summary>
        /// The path of the top level directory containing the files for the book
        /// </summary>
        public string BookFolder { get; set; }

        public string BookId
        {
            get
            {
                if (_bookId == null)
                {
                    // This will throw an exception if the ncc file doesn't exist or isn't wellformed
                    XDocument nccXml = XDocument.Load(PackageFiles[NcxXmlPath]);

                    // Find the dc:identifier meta element
                    var q = from x in nccXml.Descendants()
                            where
                                x.Attribute("name") != null
                                && x.Attribute("name").Value == "dc:identifier"
                            select x.Attribute("content").Value;

                    // There should only be one value returned from the above query
                    if (q.Count() == 1)
                    {
                        _bookId = q.First();
                    }
                    else
                    {
                        throw new InvalidPackageException("No unique ID or Title for the book");
                    }
                }
                return _bookId;
            }
        }

        public string DtBookXmlPath
        {
            get 
            {
                if (_dtBookXmlPath == null)
                {
                    // Search for the first xml or html file (other than the ncc.html) that
                    // contains wellformed Xml
                    foreach (var keyValuePair in PackageFiles)
                    {
                        string key = keyValuePair.Key;

                        if ((key.EndsWith("xml") || key.EndsWith("html") || key.EndsWith("htm")) && !key.EndsWith(FILENAME_NCC))
                        {
                            if (IsXmlFile(keyValuePair.Value))
                            {
                                _dtBookXmlPath = keyValuePair.Key;
                                break;
                            }
                        }
                    }
                }
                return _dtBookXmlPath;
            }
        }

        /// <summary>
        /// Examines the file and determines whether it's a content file based on
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private bool IsXmlFile(Stream stream)
        {
            try
            {
                XDocument contents;
                contents = XDocument.Load(stream);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int FileCount
        {
            get 
            {
                return PackageFiles.Count();
            }
        }

        public string NcxXmlPath
        {
            get
            {
                var q = from f in PackageFiles where f.Key.EndsWith("ncc.html") select f;
                if (q != null)
                {
                    return q.First().Key;
                }
                else
                {
                    throw new InvalidPackageException("Missing ncc.html file");
                }
            }
        }

        private Dictionary<string, Stream> PackageFiles { get; set; }

		#endregion Properties 
    }
}
