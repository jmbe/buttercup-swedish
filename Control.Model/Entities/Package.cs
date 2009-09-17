using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Buttercup.Control.Model.Entities
{
    public class Package
    {
		#region Fields (3) 

        private string _bookId;
        private string _dtBookXmlPath;
        private string _ncxXmlPath;

		#endregion Fields 

		#region Constructors (1) 

        /// <summary>
        /// Creates a Package instance from a Stream that represents the 
        /// OPF file.
        /// </summary>
        /// <param name="opfStream"></param>
        public Package(Stream opfStream)
        {
            opfStream.Seek(0, SeekOrigin.Begin);
            Xml = XDocument.Load(opfStream);
        }

		#endregion Constructors 

		#region Properties (9) 

        /// <summary>
        /// Retrieve the unique id for the book
        /// </summary>
        public string BookId
        {
            get
            {
                if (_bookId == null)
                {
                    // Find the element that has an id attribute with the same value of the 
                    // "unique-identifier" attribute of the package element.
                    var q = from x in Xml.Descendants()
                            where
                                x.Attribute("id") != null
                                && x.Attribute("id").Value
                                   == Xml.Element(xmlNamespace + "package").Attribute("unique-identifier").Value
                            select x.Value;

                    // There should only be one value returned from the above query
                    if (q.Count() > 0)
                    {
                        _bookId = q.First();
                    }
                    else
                    {
                        // If no unique id is found then return the Title of the book instead.
                        // TODO: Should we even do this? Check whether the unique id is actually
                        // mandatory or not.
                        q = from x in Xml.Descendants(dcNamespace + "Title")
                            select x.Value;

                        // Again there should only be one value returned, but to be safe...
                        if (q.Count() > 0)
                        {
                            _bookId = q.First();
                        }
                        else
                        {
                            // TODO: What to do here?
                            // Really want to advise the user that the book has an invalid format.
                            throw new Exception("No unique ID or Title for the book");
                        }
                    }
                }
                return _bookId;
            }
        }

        /// <summary>
        /// The dc namespace of the ODF XML document
        /// </summary>
        private XNamespace dcNamespace
        {
            get { return "http://purl.org/dc/elements/1.1/"; }
        }

        /// <summary>
        /// Retrieve the path to the dtBook XML file
        /// </summary>
        public string DtBookXmlPath
        {
            get
            {
                if (_dtBookXmlPath == null)
                {
                    IEnumerable<string> q;

                    try
                    {
                       q  = from x in Xml.Element(xmlNamespace + "package").Element(xmlNamespace + "manifest").Elements(xmlNamespace + "item")
                                where x.Attribute("media-type") != null &&
                                      x.Attribute("media-type").Value == "application/x-dtbook+xml"
                                select x.Attribute("href").Value;
                    } catch
                    {
                        throw new Exception("Invalid package file given, is not in the correct opf format.");
                    }

                    // At the moment we only support a single dtbook file
                    if (q.Count() == 1)
                    {
                        _dtBookXmlPath = q.First();
                    }
                    else
                    {
                        throw new Exception("Currently we only support books with one, and only one, application/x-dtbook+xml media type.");
                    }
                }
                return _dtBookXmlPath;
            }
        }

        /// <summary>
        /// Returns the total number of files in the package. This can be unreliable but is a rough
        /// guide to use to estimate progress while processing the zip package.
        /// </summary>
        public int FileCount
        {
            get
            {
                // Count the number of child Elements in the manifest Element.
                var q = from x in Xml.Element(xmlNamespace + "package").Element(xmlNamespace + "manifest").Elements()
                        select x;
                return q.Count();
            }
        }

        /// <summary>
        /// Retrieve the path to the dtBook NCX file for navigation points, table of contents etc.
        /// </summary>
        public string NcxXmlPath
        {
            get
            {
                if (_ncxXmlPath == null)
                {
                    var q = from x in Xml.Element(xmlNamespace + "package").Element(xmlNamespace + "manifest").Elements(xmlNamespace + "item")
                            where x.Attribute("media-type") != null &&
                                  x.Attribute("media-type").Value == "application/x-dtbncx+xml"
                            select x.Attribute("href").Value;

                    // There should only be one NCX reference
                    if (q.Count() == 1)
                    {
                        _ncxXmlPath = q.First();
                    }
                    else
                    {
                        throw new Exception("There should only be one NCX file referenced in the OPF");
                    }
                }
                return _ncxXmlPath;
            }
        }

        public string OPFPath { get; set; }

        /// <summary>
        /// The path of the top level directory containing the files for the book
        /// </summary>
        public string BookFolder { get; set; }

        /// <summary>
        /// The XML representation of the Package - i.e. the OPF file.
        /// </summary>
        private XDocument Xml { get; set; }

        /// <summary>
        /// The default namespace of the OPF XML document (xmlns)
        /// </summary>
        private XNamespace xmlNamespace
        {
            get { return "http://openebook.org/namespaces/oeb-package/1.0/"; }
        }

		#endregion Properties 
    }
}