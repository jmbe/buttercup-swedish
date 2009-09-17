using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Buttercup.Control.Model.Entities
{
    public class TableOfContents
    {
		#region Fields (4) 

        private const string _NAV_ITEM_NODE_NAME = "navPoint";
        private const string _NAV_ROOT_NODE_NAME = "navMap";
        private static readonly XNamespace _ncxNamespace;

		#endregion Fields 

		#region Constructors (2) 

        /// <summary>
        /// Used to instantiate a new TableOfContents object based on a object
        /// representation of an NCX file.
        /// </summary>
        /// <param name="ncxXml">A reference to the serialised representation of the NCX 
        /// (Navigation Control) file</param>
        public TableOfContents(XDocument ncxXml)
        {
            XElement navRootElement = ncxXml.Element(_ncxNamespace + "ncx").Element(_ncxNamespace + _NAV_ROOT_NODE_NAME);

            Headings = GetHeadings(navRootElement);
            if (Headings == null)
            {
                //We want an empty list if there are no headings.
                Headings = new List<Heading>();
            }
            else
            {
                CalculateHeadingLevels(Headings);
                CalculateFlatHeadingList();
            }
        }

        static TableOfContents()
        {
            _ncxNamespace = "http://www.daisy.org/z3986/2005/ncx/";
        }

		#endregion Constructors 

		#region Properties (2) 

        public List<Heading> FlatHeadingList { get; private set; }

        /// <summary>
        /// The XML representation of the TOC - i.e. the ncx xml file.
        /// </summary>
        public XDocument Xml { get; set; }

		#endregion Properties 

		#region Methods (5) 

		// Private Methods (5) 

        /// <summary>
        /// Recursively adds the headings and subheadings to the given flat list.
        /// </summary>
        /// <param name="flatList">The flat list to build.</param>
        private void AddHeadingsToFlatList(List<Heading> flatList, List<Heading> headingList)
        {
            if (flatList == null)
                throw new ArgumentNullException("flatList");

            if (headingList == null)
                return;

            foreach (Heading currentHeading in headingList)
            {
                flatList.Add(currentHeading);
                AddHeadingsToFlatList(flatList, currentHeading.SubHeadings);
            }
        }

        /// <summary>
        /// Gets a flat list of all headings and subheadings in DFS-order.
        /// </summary>
        /// <returns>A flat list of all headings and subheadings in the current book.</returns>
        private void CalculateFlatHeadingList()
        {
            FlatHeadingList = new List<Heading>();
            AddHeadingsToFlatList(FlatHeadingList, Headings);
        }

        /// <summary>
        /// Recursively calculates the levels of current table of contents headings and subheadings
        /// </summary>
        private void CalculateHeadingLevels(List<Heading> headingList)
        {
            if (headingList == null)
                return;

            CalculateHeadingLevels(headingList, 0);
        }

        private void CalculateHeadingLevels(List<Heading> headingList, int currentLevel)
        {
            foreach (Heading heading in headingList)
            {
                heading.level = currentLevel;
                CalculateHeadingLevels(heading.SubHeadings, currentLevel + 1);
            }
        }

        /// <summary>
        /// Recursively builds the structure of and returns the TableOfContents headings.
        /// </summary>
        /// <param name="parentElement">The parent XML element containing the NavPoints holding each heading.</param>
        /// <returns>The list of (sub)headings in the current level only.
        /// (Each heading may contain subheadings.)</returns>
        private List<Heading> GetHeadings(XElement parentElement)
        {
            if (parentElement == null)
                return null;

            IEnumerable<XElement> navPoints = parentElement.Elements(_ncxNamespace + _NAV_ITEM_NODE_NAME);
            if (navPoints == null || navPoints.Count() == 0)
                return null;

            List<Heading> headings = new List<Heading>();

            foreach (XElement point in navPoints)
            {
                Heading heading = new Heading();

                //This will throw an exception if the 'id' is missing - the ID must be specified (A2S)
                string id = point.Attribute("id").Value;
                heading.ID = id;

                XElement contentLink = point.Element(_ncxNamespace + "content");
                //An exception will be thrown if the content element and/or its src attribute is missing.
                // These must be specified (A2S).
                heading.SmilReference = contentLink.Attribute("src").Value;


                // TODO: for now assume only one language - if multiple
                // languages then there may be more than one naLabel for
                // each navPoint

                XElement label = point.Element(_ncxNamespace + "navLabel");
                //An exception will be thrown if the label is missing - a Label element must be specified (A2S)

                string text = String.Empty;
                XElement textElement = label.Element(_ncxNamespace + "text");
                if (textElement != null)
                {
                    text = textElement.Value;
                }
                heading.Text = text;

                XElement audioElement = label.Element(_ncxNamespace + "audio");
                if (audioElement != null)
                {
                    heading.Audio = new Audio();
                    heading.Audio.SourceUri = new Uri(audioElement.Attribute("src").Value, UriKind.Relative);
                    heading.Audio.ClipStart = ValueConversionHelper.GetConvertedTimeSpan
                        (audioElement.Attribute("clipBegin").Value);
                    heading.Audio.ClipEnd = ValueConversionHelper.GetConvertedTimeSpan
                        (audioElement.Attribute("clipEnd").Value);
                }

                // Recursively process the SubHeadings of this Heading
                List<Heading> subHeadings = GetHeadings(point);
                if (subHeadings != null)
                {
                    heading.SubHeadings = subHeadings;
                }
                headings.Add(heading);
            }

            return headings;
        }

		#endregion Methods 


        public List<Heading> Headings
        {
            get; private set;
        }
    }
}