using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using Buttercup.Control.Common.IO;
using Buttercup.Control.Model;
using System;

namespace Buttercup.Control.Model.Entities
{
    public class Book
    {
		#region Fields (10) 

        private XDocument _bookXml;
        private string _creator;
        private string _date;
        private int _headinglevels;
        private string _language;
        private string _multimediacontent;
        private string _multimediatype;
        private string _publisher;
        private string _title;
        private string _totaltime;

        private bool _processedImagesinXML = false;

		#endregion Fields 

		#region Properties (18) 

        public List<Bookmark> Bookmarks { get; set; }

        /// <summary>
        /// This was going to be a fragment of the full Xml - only that needed for rendering.
        /// At the moment it's simply the full Xml.
        /// </summary>
        public XDocument BookXml
        {
            get
            {
                if (_bookXml == null)
                {
                    PostProcessXML(Xml);
                    _bookXml = Xml;
                }
                return _bookXml;
            }
        }

        public string Creator
        {
            get
            {
                if (_creator == null)
                {
                    _creator = GetMetaData("dc:Creator");
                }
                return _creator;
            }
        }

        public string Date
        {
            get
            {
                if (_date == null)
                {
                    _date = GetMetaData("dc:Date");
                }
                return _date;
            }
        }

        /// <summary>
        /// Gets or sets the folder path - this could be an IsolatedStorage directory
        /// or WebServer depending on the source of the book.
        /// <value>The folder path.</value>
        public IDirectory FolderPath { get; set; }

        public bool HasEmbeddedAudio { get; set; }

        /// <summary>
        /// Holds the number of different levels of headings there are in this book. Used to
        /// display in Book Information.
        /// </summary>
        public int HeadingLevels
        {
            get
            {
                if (_headinglevels == 0)
                {
                    //calculate and display the number of heading levels exhibited in the book

                    List<Heading> Headings = TableOfContents.FlatHeadingList;
                    foreach (Heading heading in Headings)
                    {
                        if (heading.level > _headinglevels)
                        {
                            _headinglevels = heading.level + 1;
                        }
                    }
                }
                return _headinglevels;
            }
        }

        /// <summary>
        /// Returns whether the books contents are browseable. If a book has been created
        /// from IsolatedStorage then this will be false. If it has been built from a server
        /// reference then true.
        /// </summary>
        /// <remarks>
        /// This property is set in DataContext.dtBookFile_OpenAsyncComplete when the book
        /// files have been read and a Book instance is created.
        /// </remarks>
        public bool IsBrowseable { get; set; }

        public string Language
        {
            get
            {
                if (_language == null)
                {
                    _language = GetMetaData("dc:Language");
                }
                return _language;
            }
        }

        public string MultimediaContent
        {
            get
            {
                if (_multimediacontent == null)
                {
                    _multimediacontent = GetMetaData("dtb:multimediaContent");
                }
                return _multimediacontent;
            }
        }

        public string MultimediaType
        {
            get
            {
                if (_multimediatype == null)
                {
                    _multimediatype = GetMetaData("dtb:multimediaType");
                }
                return _multimediatype;
            }
        }

        /// <summary>
        /// Gets or sets the page list for this book.
        /// </summary>
        /// <value>The page list.</value>
        public PageList PageList { get; set; }

        public string Publisher
        {
            get
            {
                if (_publisher == null)
                {
                    _publisher = GetMetaData("dc:Publisher");
                }
                return _publisher;
            }
        }

        public TableOfContents TableOfContents { get; set; }

        public string Title
        {
            get
            {
                if (_title == null)
                {
                    _title = GetMetaData("dc:Title");
                }
                return _title;
            }
        }

        public string TotalTime
        {
            get
            {
                if (_totaltime == null)
                {
                    _totaltime = GetMetaData("dtb:TotalTime");
                }
                return _totaltime;
            }
        }

        /// <summary>
        /// The XML representation of the Book - i.e. the dtbook xml file.
        /// </summary>
        public XDocument Xml { get; set; }

        /// <summary>
        /// The default namespace of the dtbook XML document (xmlns)
        /// </summary>
        public XNamespace xmlNamespace
        {
            get { return "http://www.daisy.org/z3986/2005/dtbook/"; }
        }

		#endregion Properties 

		#region Methods (1) 

		// Private Methods (1) 

        /// <summary>
        /// Used to grab specified book information from the meta data in the book xml.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetMetaData(string name)
        {
            string data = null;
            try
            {
                var q = from x in Xml.Element(xmlNamespace + "dtbook")
                            .Element(xmlNamespace + "head")
                            .Elements(xmlNamespace + "meta")
                        where x.Attribute("name") != null && x.Attribute("name").Value == name
                        select x.Attribute("content").Value;
                if (q.Count() == 1)
                {
                    data = q.First();
                }
            }
            catch (Exception ex)
            {

            }
            return data;
        }

        /// <summary>
        /// Post process the XML to add id's to any element we are going to display/read which
        /// do not have id's.
        /// </summary>
        /// <param name="xml"></param>
        private void PostProcessXML(XDocument xml)
        {
            IEnumerable<XElement> elements = xml.Root.Descendants().FilterByAttribute("alt");

            foreach (XElement el in elements)
            {
                if (el.Attribute("id") == null)
                    el.SetAttributeValue("id", System.Guid.NewGuid());
            }
        }

		#endregion Methods 
    }
}