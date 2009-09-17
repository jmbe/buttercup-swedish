using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Buttercup.Control.Model.Entities
{
    public class PageList
    {
        #region Fields (3)

        private static readonly XNamespace _ncxNamespace;
        private const string _PAGELIST_ROOT_NODE_NAME = "pageList";
        private const string _PAGETARGET_NODE_NAME = "pageTarget";

        #endregion Fields

        #region Constructors (2)

        /// <summary>
        /// Construct the list of pages for a book from the dtBook NCX File
        /// </summary>
        /// <param name="ncxXml"></param>
        public PageList(XDocument ncxXml)
        {
            XElement navRootElement = ncxXml.Element(_ncxNamespace + "ncx").Element(_ncxNamespace + _PAGELIST_ROOT_NODE_NAME);

            Pages = GetPages(navRootElement);
        }

        static PageList()
        {
            _ncxNamespace = "http://www.daisy.org/z3986/2005/ncx/";
        }

        #endregion Constructors

        #region Properties (1)

        /// <summary>
        /// Gets or sets the list of pages.
        /// </summary>
        /// <value>The list of pages.</value>
        public List<PageReference> Pages { get; private set; }

        #endregion Properties

        #region Methods (1)

        // Private Methods (1) 

        /// <summary>
        /// Gets the pages from the XML document.
        /// </summary>
        /// <param name="rootElement">The root element containing the page marker definitions.</param>
        /// <returns>The list of pages described by the XML.</returns>
        private List<PageReference> GetPages(XElement rootElement)
        {
            if (rootElement == null)
                return null;

            IEnumerable<XElement> pageTargets = rootElement.Elements(_ncxNamespace + _PAGETARGET_NODE_NAME);
            if (pageTargets == null || pageTargets.Count() == 0)
                return null;

            List<PageReference> pages = new List<PageReference>();
            foreach (XElement currentTarget in pageTargets)
            {
                PageReference currentPage = new PageReference();

                //An exception will be thrown if either of these attributes are missing or have invalid values (A2S).
                string playOrder = currentTarget.Attribute("playOrder").Value;
                string pageType = currentTarget.Attribute("type").Value;
                string pageNumber = currentTarget.Attribute("value").Value;
                currentPage.AbsoluteOrder = Int32.Parse(playOrder);
                currentPage.PageType = (PageType)Enum.Parse(typeof(PageType), pageType, true);
                currentPage.PageNumber = Int32.Parse(pageNumber);

                XElement contentLink = currentTarget.Element(_ncxNamespace + "content");
                //An exception will be thrown if the content element and/or its src attribute is missing.
                // These must be specified (A2S).
                currentPage.SmilReference = contentLink.Attribute("src").Value;


                XElement label = currentTarget.Element(_ncxNamespace + "navLabel");
                //An exception will be thrown if the label is missing - a navLabel element must be specified (A2S)

                string text = String.Empty;
                XElement textElement = label.Element(_ncxNamespace + "text");
                if (textElement != null)
                {
                    text = textElement.Value;
                }
                currentPage.Text = text;

                XElement audioElement = label.Element(_ncxNamespace + "audio");
                if (audioElement != null)
                {
                    currentPage.Audio = new Audio();
                    currentPage.Audio.SourceUri = new Uri(audioElement.Attribute("src").Value, UriKind.Relative);
                    currentPage.Audio.ClipStart = ValueConversionHelper.GetConvertedTimeSpan
                        (audioElement.Attribute("clipBegin").Value);
                    currentPage.Audio.ClipEnd = ValueConversionHelper.GetConvertedTimeSpan
                        (audioElement.Attribute("clipEnd").Value);
                }

                pages.Add(currentPage);
            }
            //TODO: Do we really need to sort?
            pages.Sort();

            return pages;
        }

        #endregion Methods
    }
}