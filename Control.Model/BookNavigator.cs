using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Buttercup.Control.Model.Entities;

namespace Buttercup.Control.Model
{
    /// <summary>
    /// The Book Navigator is used for keeping track of the current element in the book to be played,
    /// advancing that element during playback and changing the current element when navigating around
    /// the book.
    /// </summary>
    public class BookNavigator
    {
        #region Fields (8)

        private readonly XDocument _bookXml;
        private readonly Book _currentBook;
        private int _currentLevel;
        private XElement _firstElement;
        private readonly int _innermostLevel;
        private XElement _lastElement;
        private const string _PARAGRAPH_LOCAL_NAME = "p";
        private const string _SMIL_REF_ATTR_NAME = "smilref";

        #endregion Fields

        #region Constructors (1)

        public BookNavigator(Book currentBook)
        {
            if (currentBook == null)
                throw new ArgumentNullException("currentBook", "The book cannot be null.");

            // By default we don't navigate to inlineElements (because the XAML surface can't highlight Runs!)
            NavigateInlineElements = false;

            _currentBook = currentBook;
            _bookXml = currentBook.BookXml;
            //TODO: Remove if we are not using this type of level navigation
            _innermostLevel = 6;
            MoveToFirstSection();
        }

        public BookNavigator(Book currentBook, bool navigateInlineElements)
        {
            NavigateInlineElements = navigateInlineElements;
        }

        #endregion Constructors

        #region Properties (5)

        public bool NavigateInlineElements { get; set; }

        /// <summary>
        /// Gets a value indicating whether the navigator is currently positioned at the end of the book.
        /// Used to stop the player from attempting to play past the end of the book, as well as indicating
        /// state so the views can reflect it appropriately (e.g. enabling/disabling buttons)
        /// </summary>
        /// <value>True if the end of the book has been reached, otherwise false.</value>
        public bool AtEndOfBook { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the navigator is currently positioned at the start of the book.
        /// </summary>
        /// <value>True if the navigator is at the start of the book, otherwise false.</value>
        public bool AtStartOfBook { get; private set; }

        /// <summary>
        /// Gets the current element.
        /// </summary>
        /// <value>The current element.</value>
        public XElement CurrentElement { get; private set; }

        /// <summary>
        /// Gets the ID of the current element.
        /// </summary>
        /// <value>The ID of the current element.</value>
        public string CurrentElementID
        {
            get
            {
                if (CurrentElement != null)
                {
                    //By definition, the current element will have an 'id' attribute since it is speakable.
                    XAttribute id = CurrentElement.Attribute("id");
                    if (id != null)
                        return id.Value;
                }
                return null;
            }
            set
            {
                XElement foundElement = GetSpeakableElementByID(value);
                if (foundElement != null)
                {
                    CurrentElement = foundElement;
                    AtEndOfBook = false;
                    AtStartOfBook = (CurrentElement == _firstElement);
                }
                else
                {
                    throw new ArgumentException("No speakable element exists with the given ID.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the current navigation level.
        /// TODO: Are we going to be using this?
        /// </summary>
        /// <value>The current level in the range [1, _innermostLevel] (maximum [1,6]).</value>
        public int CurrentLevel
        {
            get { return _currentLevel; }
            set { _currentLevel = Math.Max(0, Math.Min(value, _innermostLevel)); }
        }

        #endregion Properties

        #region Methods (29)

        // Public Methods (15) 

        /// <summary>
        /// Navigates to a lower level of the current book.
        /// </summary>
        public void DownLevel()
        {
            //if we are at the end of the book we cannot traverse down... return without doing anything
            if (AtEndOfBook) return;

            List<Heading> allHeadings = _currentBook.TableOfContents.FlatHeadingList;
            Heading currentSectionHeading = GetCurrentSectionHeading();
            int currentHeadingIndex = allHeadings.IndexOf(currentSectionHeading);

            if (currentHeadingIndex < allHeadings.Count - 1)
            {
                currentSectionHeading = allHeadings[currentHeadingIndex + 1];
                SeekToSmilReference(currentSectionHeading.SmilReference);

                AtStartOfBook = (CurrentElement == _firstElement);
                AtEndOfBook = (CurrentElement == _lastElement);
            }
        }

        /// <summary>
        /// Determines the heading that the current element is under in the current book.
        /// </summary>
        public Heading GetCurrentSectionHeading()
        {
            List<Heading> headings = _currentBook.TableOfContents.FlatHeadingList;
            return GetCurrentSectionHeading(headings);
        }

        /// <summary>
        /// Determines the page that the current element is on in the current book, used when navigating to next
        /// page or previous page.
        /// </summary>
        /// <returns></returns>
        public PageReference GetCurrentSectionPage()
        {
            List<PageReference> pages = _currentBook.PageList.Pages;
            return GetCurrentSectionPage(pages);
        }

        /// <summary>
        /// MoveNext v2.0, move to the previous speakable element in the book
        /// Systematically searching through each level of previous elements and their descendants
        /// </summary>
        public void MovePrevious()
        {
            //if we are at the beginning of the book we cannot traverse up to book... return without doing anything
            if (AtStartOfBook) return;

            if (CurrentElement != null)
            {
                //check the current element and its descendants
                XElement currentElement = CurrentElement;
                bool isElementSpeakable = false;

                while(currentElement != null && !isElementSpeakable)
                {
                    XElement siblingElement = currentElement;
                    while (siblingElement != null && !isElementSpeakable)
                    {
                        //move to next element and check for speakable
                        siblingElement = XmlHelper.PreviousSibling(siblingElement);

                        //check the descendants for the last speakable element
                        isElementSpeakable = GetLastSpeakableElementFromDescendants(ref siblingElement);

                        if (!isElementSpeakable)
                            isElementSpeakable = IsSpeakable(siblingElement);
                    }

                    //we've searched through an entire level, go up one level
                    if (!isElementSpeakable)
                    {
                        currentElement = currentElement.Parent;
                        isElementSpeakable = IsSpeakable(currentElement);
                    }
                    else
                    {
                        currentElement = siblingElement;
                    }
                }
                
                //If the next element hasn't been set, we are at the end of the book.
                if (isElementSpeakable)
                {
                    CurrentElement = currentElement;
                    AtEndOfBook = false;
                }
                else
                {
                    AtStartOfBook = true;
                }
            }
        }

        /// <summary>
        /// MoveNext v2.0, move to the next speakable element in the book
        /// Systematically searching through each level of elements and their descendants
        /// </summary>
        public void MoveNext()
        {
            if (AtEndOfBook) return;

            if (CurrentElement != null)
            {
                //check the current element and its descendants
                XElement currentElement = CurrentElement;
                bool isElementSpeakable = GetFirstSpeakableElementFromDescendants(ref currentElement);

                while (currentElement != null && !isElementSpeakable)
                {
                    XElement siblingElement = currentElement;
                    //exhaust _all_ sibling nodes on that level before going up another level
                    while (siblingElement != null && !isElementSpeakable)
                    {
                        //move to next element and check for speakable
                        siblingElement = XmlHelper.NextSibling(siblingElement);
                        isElementSpeakable = IsSpeakable(siblingElement);

                        //search the whole 'level' of sibling nodes for a speakable element
                        if (!isElementSpeakable)
                            isElementSpeakable = GetFirstSpeakableElementFromDescendants(ref siblingElement);
                    }

                    //we've searched through an entire level, go up one level
                    if (!isElementSpeakable) currentElement = currentElement.Parent;
                    else currentElement = siblingElement;
                }

                //If the next element hasn't been set, we are at the end of the book.
                if (isElementSpeakable)
                {
                    CurrentElement = currentElement;
                    AtStartOfBook = false;
                }
                else
                {
                    AtEndOfBook = true;
                }
            }
        }

        /// <summary>
        /// Navigates to the next page
        /// </summary>
        public void NextPage()
        {
            List<PageReference> allPages = _currentBook.PageList.Pages;
            if (allPages != null)
            {
                PageReference currentPage = GetCurrentSectionPage();

                int currentPageIndex = -1;

                if (currentPage != null)
                {
                    currentPageIndex = allPages.IndexOf(currentPage);
                }

                if (currentPageIndex < allPages.Count - 1)
                {
                    currentPage = allPages[currentPageIndex + 1];
                    SeekToSmilReference(currentPage.SmilReference);

                    AtStartOfBook = (CurrentElement == _firstElement);
                    AtEndOfBook = (CurrentElement == _lastElement);
                }
            }
        }

        /// <summary>
        /// Navigates to the previous page
        /// </summary>
        public void PrevPage()
        {
            List<PageReference> allPages = _currentBook.PageList.Pages;

            if (allPages != null)
            {
                PageReference page = GetCurrentSectionPage();

                //If currently in the middle of a section, pressing up a previous page simply goes to the beginning of that page
                XAttribute smilRef = CurrentElement.Attribute(_SMIL_REF_ATTR_NAME);
                PageReference isCurrentSectionAPage = null;
                if (smilRef != null)
                    isCurrentSectionAPage = GetPageBySmilReference(allPages, smilRef.Value);

                if (isCurrentSectionAPage == null)
                {
                    SeekToSmilReference(page.SmilReference);
                }
                else
                {
                    int currentPageIndex = allPages.IndexOf(page);

                    if (currentPageIndex > 0)
                    {
                        page = allPages[currentPageIndex - 1];
                        SeekToSmilReference(page.SmilReference);
                    }
                }

                AtStartOfBook = (CurrentElement == _firstElement);
                AtEndOfBook = (CurrentElement == _lastElement);
            }
        }

        /// <summary>
        /// Search all the speakable elements in the book for a given search string
        /// </summary>
        /// <param name="searchString">A String of search terms.</param>
        /// <returns>An List of XElements that match the search.</returns>
        public List<XElement> SearchForString(string searchString)
        {
            //Get all the speakable elements
            IEnumerable<XElement> allElements = _currentBook.BookXml.Descendants().FilterByAttribute(_SMIL_REF_ATTR_NAME);
            List<XElement> results = new List<XElement>();

            //TODO: Something more complex? currently just anything + searchterm + anything
            string regEx = ".*" + Regex.Escape(searchString) + ".*";

            foreach (XElement currentElement in allElements)
            {
                if (Regex.Match(currentElement.Value, regEx, RegexOptions.IgnoreCase).Success)
                    results.Add(currentElement);
            }

            return results;
        }

        /// <summary>
        /// Sets the CurrentElement to the element with the given id.
        /// </summary>
        /// <param name="id"></param>
        public void SeekToElementId(string id)
        {
            List<XElement> results = _currentBook.BookXml.Descendants().FilterByAttribute("id", id);
            if (results.Count >= 1)
            {
                XElement target = results[0];

                if (!IsSpeakable(target))
                {
                    GetFirstSpeakableElementFromDescendants(ref target);
                }

                CurrentElement = target;

                AtEndOfBook = false;
                AtStartOfBook = false;
            }
        }

        /// <summary>
        /// Sets the current element to the heading with the given id. 
        /// </summary>
        public void SeekToHeadingByID(string id)
        {
            //seeking to book headings, so not at end of book.
            AtEndOfBook = false;

            List<Heading> allHeadings = _currentBook.TableOfContents.FlatHeadingList;
            foreach (Heading heading in allHeadings)
            {
                if (heading.ID == id)
                {
                    SeekToSmilReference(heading.SmilReference);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the current element to the page of the given page number. 
        /// </summary>
        /// <param name="pageNum"></param>
        public bool SeekToPageNum(int pageNum)
        {
            List<PageReference> allPages = _currentBook.PageList.Pages;
            if (allPages != null && pageNum > 0 && pageNum <= allPages.Count)
            {
                //TODO Navigate to page should compare pagenum values, but this will affect how the
                //TODO page navigation functionality works and errors
                //foreach (PageReference pageRef in allPages)
                //{
                //    if (pageRef.PageNumber == pageNum)
                //    {
                //        targetPage = pageRef;
                //        break;
                //    }
                //}
                PageReference targetPage = allPages[pageNum - 1];
                SeekToSmilReference(targetPage.SmilReference);

                AtStartOfBook = (CurrentElement == _firstElement);
                AtEndOfBook = (CurrentElement == _lastElement);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the current element to the element with the given smil reference.
        /// </summary>
        /// <param name="smil">SMIL reference for element to seek to.</param>
        public void SeekToSmilReference(string smil)
        {
            List<XElement> matchingElements = _bookXml.Descendants().FilterByAttribute(_SMIL_REF_ATTR_NAME, smil);
            bool speakableElementFound;

            foreach (XElement bookElement in matchingElements)
            {
                XElement speakableElement = bookElement;
                if (!IsSpeakable(bookElement))
                {
                    speakableElementFound = GetFirstSpeakableElementFromDescendants(ref speakableElement);
                }
                else
                {
                    speakableElementFound = true;
                }

                if (speakableElementFound)
                {
                    CurrentElement = speakableElement;
                    return;
                }
            }
            throw new ArgumentException("No speakable element exists for the given SMIL reference.");
        }

        /// <summary>
        /// Navigates to an upper level of the current book.
        /// </summary>
        public void UpLevel()
        {
            //if we are at the beginning of the book we cannot traverse up to book... return without doing anything
            if (AtStartOfBook) return;

            List<Heading> allHeadings = _currentBook.TableOfContents.FlatHeadingList;
            Heading currentSectionHeading = GetCurrentSectionHeading();

            //If currentSectionHeading is null we have a situation where the current element takes place _before_
            //the heading of the current section (i.e. page numbers)
            while (currentSectionHeading == null && !AtStartOfBook)
            {
                //Since we are not getting anywhere with this current element, move to the previous element and try again
                MovePrevious();
                currentSectionHeading = GetCurrentSectionHeading();
            }

            //TODO: Find the cause of this and throw exception
            if (currentSectionHeading == null) return;

            //If currently in the middle of a section, pressing up a level simply goes to the heading of that section
            XAttribute smilRef = CurrentElement.Attribute(_SMIL_REF_ATTR_NAME);
            Heading isCurrentSectionAHeading = null;
            if (smilRef != null)
                isCurrentSectionAHeading = GetHeadingBySmilReference(allHeadings, smilRef.Value);

            if (isCurrentSectionAHeading == null)
            {
                SeekToSmilReference(currentSectionHeading.SmilReference);
            }
            else
            {
                //otherwise, we're at a section heading so traverse to previous heading
                int currentHeadingIndex = allHeadings.IndexOf(currentSectionHeading);

                if (currentHeadingIndex > 0)
                {
                    currentSectionHeading = allHeadings[currentHeadingIndex - 1];
                    SeekToSmilReference(currentSectionHeading.SmilReference);
                }
            }

            AtStartOfBook = (CurrentElement == _firstElement);
            AtEndOfBook = (CurrentElement == _lastElement);
        }
        // Private Methods (14) 

        /// <summary>
        /// Calculates the first and last elements and caches their references, used to determine whether we are
        /// at the beginning/end of the book when navigating/playing.
        /// </summary>
        private void CalculateFirstAndLastElements()
        {
            List<XElement> speakableElements = _bookXml.Descendants().FilterByAttribute(_SMIL_REF_ATTR_NAME);

            int numberOfElements = speakableElements.Count;
            if (numberOfElements >= 1)
            {
                _firstElement = speakableElements[0];
                //ignore the doctitle, as we are not speaking this anymore
                if (_firstElement.Name.LocalName == "doctitle")
                {
                    _firstElement = speakableElements[1];
                }
                _lastElement = speakableElements[numberOfElements - 1];
            }
        }

        /// <summary>
        /// Determines the heading that the current element is under in the current book, given a list of headings.
        /// TODO: Merge this method with GetCurrentSectionHeading() method... no real need to have both.
        /// </summary>
        /// <returns></returns>
        private Heading GetCurrentSectionHeading(List<Heading> flatHeadingList)
        {
            if (CurrentElement == null)
            {
                //At end of book - return last section. Throws an ArgumentOutOfRangeException if no headings exist.
                return flatHeadingList[flatHeadingList.Count - 1];
            }

            //Try current element
            XAttribute smilRef = CurrentElement.Attribute(_SMIL_REF_ATTR_NAME);
            Heading sectionHeading = null;
            if (smilRef != null)
                sectionHeading = GetHeadingBySmilReference(flatHeadingList, smilRef.Value);

            if (sectionHeading != null)
                return sectionHeading;

            //Get nearest level element - last instance found will be first in a traversal towards the root.
            IEnumerable<XElement> elementAncestors = CurrentElement.Ancestors();

            XElement levelElement = null;
            foreach (XElement currentElement in elementAncestors)
            {
                if (currentElement.Name.LocalName.StartsWith("level"))
                {
                    levelElement = currentElement;
                    break;
                }
            }

            //If level element is null then the current element has no ancestors
            if (levelElement == null) return null;

            //DTBs must have at least a level1 (A2S) - if not an exception will be thrown.
            List<XElement> speakableDescendants = levelElement.Descendants().FilterByAttribute(_SMIL_REF_ATTR_NAME);
            foreach (XElement currentDescendant in speakableDescendants)
            {
                XAttribute descendantSmilRef = currentDescendant.Attribute(_SMIL_REF_ATTR_NAME);
                XAttribute currentSmilRef = CurrentElement.Attribute(_SMIL_REF_ATTR_NAME);
                string smilReference = null;

                if (descendantSmilRef != null)
                    smilReference = descendantSmilRef.Value;

                if (currentSmilRef != null && smilReference == currentSmilRef.Value)
                    return null;
                //	throw new InvalidOperationException("No contents heading exists for the current section");

                sectionHeading = GetHeadingBySmilReference(flatHeadingList, smilReference);
                if (sectionHeading != null)
                    return sectionHeading;
            }

            throw new InvalidOperationException("An invalid internal state was reached: No contents heading exists for the current section.");
        }

        /// <summary>
        /// Determine the page that the current element is in, given a list of pages.
        /// TODO: Merge this method with GetCurrentSectionPage() method... no real need to have both.
        /// </summary>
        /// <param name="pageList"></param>
        /// <returns></returns>
        private PageReference GetCurrentSectionPage(List<PageReference> pageList)
        {
            if (CurrentElement == null)
            {
                //At end of book - return last section. Throws an ArgumentOutOfRangeException if no headings exist.
                return pageList[pageList.Count - 1];
            }

            //Try current element
            XAttribute smilRef = CurrentElement.Attribute(_SMIL_REF_ATTR_NAME);
            PageReference page = null;
            if (smilRef != null)
                page = GetPageBySmilReference(pageList, smilRef.Value);

            if (page != null)
                return page;

            //Get all elements in the document, in document order
            IEnumerable<XElement> allElements = _currentBook.BookXml.Descendants().FilterByAttribute(_SMIL_REF_ATTR_NAME);

            XElement currentPage = null;
            foreach (XElement element in allElements)
            {
                if (element.Name.LocalName == "pagenum")
                {
                    currentPage = element;
                }

                XAttribute elementSmilRef = element.Attribute(_SMIL_REF_ATTR_NAME);
                XAttribute currentSmilRef = CurrentElement.Attribute(_SMIL_REF_ATTR_NAME);

                if (elementSmilRef != null && currentSmilRef != null && elementSmilRef.Value == currentSmilRef.Value)
                {
                    if (currentPage != null)
                    {
                        XAttribute pageSmilRef = currentPage.Attribute(_SMIL_REF_ATTR_NAME);
                        if (pageSmilRef != null)
                            page = GetPageBySmilReference(pageList, pageSmilRef.Value);

                        return page;
                    }
                    return null;
                }
            }

            throw new InvalidOperationException("An invalid internal state was reached: No page reference exists for the current section.");
        }

        /// <summary>
        /// Gets the first speakable element from within the given node's descendants.
        /// </summary>
        /// <param name="targetElement">The reference for the target element.</param>
        /// <returns>True if the target element reference was updated, otherwise false.</returns>
        /// <remarks>The target element reference will only be updated if the element is
        /// definitely found to be a speakable node. Note that for some DTB structures,
        /// the actual text may be in a nested element (e.g. footnotes, prodnotes).</remarks>
        private bool GetFirstSpeakableElementFromDescendants(ref XElement targetElement)
        {
            if (targetElement != null)
            {
                //Speakable now means wither a smil reference or an alt tag
                List<XElement> speakableChildren = targetElement.Descendants().FilterByAnyAttributes(_SMIL_REF_ATTR_NAME, "alt");

                foreach (XElement testElement in speakableChildren)
                {
                    if (IsSpeakable(testElement))
                    {
                        targetElement = testElement;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the heading from the flat list with the given SMIL reference.
        /// </summary>
        /// <param name="flatList">The flat list to search.</param>
        /// <param name="smilReference">The SMIL reference.</param>
        /// <returns>The corresponding heading if found, otherwise null.</returns>
        private Heading GetHeadingBySmilReference(List<Heading> flatList, string smilReference)
        {
            foreach (Heading currentHeading in flatList)
            {
                if (currentHeading.SmilReference == smilReference)
                    return currentHeading;
            }

            return null;
        }

        /// <summary>
        /// Gets the last speakable element from within the given node's descendants.
        /// </summary>
        /// <param name="targetElement">The reference for the target element.</param>
        /// <returns>True if the target element reference was updated, otherwise false.</returns>
        /// <remarks>The target element reference will only be updated if the element is
        /// definitely found to be a speakable node. Note that for some DTB structures,
        /// the actual text may be in a nested element (e.g. footnotes, prodnotes).</remarks>
        private bool GetLastSpeakableElementFromDescendants(ref XElement targetElement)
        {
            bool isReferenceUpdated = false;

            if (targetElement != null)
            {
                List<XElement> speakableChildren = targetElement.Descendants().FilterByAnyAttributes(_SMIL_REF_ATTR_NAME, "alt");

                foreach (XElement testElement in speakableChildren)
                {
                    if (IsSpeakable(testElement))
                    {
                        targetElement = testElement;
                        isReferenceUpdated = true;
                    }
                }
            }
            return isReferenceUpdated;
        }

        /// <summary>
        /// Retrieve a page from a list of pages, given the SMIL reference for that page.
        /// </summary>
        /// <param name="pageList"></param>
        /// <param name="smilReference"></param>
        /// <returns></returns>
        private PageReference GetPageBySmilReference(List<PageReference> pageList, string smilReference)
        {
            if (pageList != null)
            {
                foreach (PageReference page in pageList)
                {
                    if (page.SmilReference == smilReference)
                        return page;
                }
            }

            return null;
        }

        private XElement GetSpeakableElementByID(string id)
        {
            //Double filter - first by ID (to narrow down), then by SMIL reference to validate.
            List<XElement> matchingElements = _bookXml.Descendants().FilterByAttribute("id", id)
                .FilterByAttribute(_SMIL_REF_ATTR_NAME);

            if (matchingElements.Count >= 1)
            {
                return matchingElements[0];
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified element is speakable.
        /// </summary>
        /// <param name="testElement">The test element.</param>
        /// <returns>
        /// True if the specified element is speakable, otherwise false.
        /// </returns>
        private bool IsSpeakable(XElement testElement)
        {
            if (testElement == null)
                return false;

            if (testElement.Attribute("id") == null || (testElement.Attribute(_SMIL_REF_ATTR_NAME) == null && testElement.Attribute("alt") == null))
                return false;

            //ignore the doctitle, as we are not speaking this anymore
            if (testElement.Name.LocalName == "doctitle")
                return false;

            return true;
        }

        /// <summary>
        /// Moves to the first speakable element in the document.
        /// </summary>
        private void MoveToFirstSection()
        {
            CurrentLevel = 0;
            AtStartOfBook = true;

            //Get first SMIL reference in book XML
            CalculateFirstAndLastElements();
            CurrentElement = _firstElement;

            AtStartOfBook = true;
            AtEndOfBook = (_firstElement == _lastElement);

            if (CurrentElement == null)
                AtEndOfBook = true; //No content exists - essentially we're at the end of an empty book.
        }

        #endregion Methods
    }
}