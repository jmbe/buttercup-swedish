using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Buttercup.Control.Model
{
    public static class XmlHelper
    {
        #region Methods (7)

        // Public Methods (7) 

        /// <summary>
        /// Filters a list of XElements to a subset which match ALL of the given attributes
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="attributeNames"></param>
        /// <returns></returns>
        public static List<XElement> FilterByAllAttributes(this IEnumerable<XElement> elements, List<string> attributeNames)
        {
            List<XElement> filteredElements = new List<XElement>();

            foreach (XElement currentElement in elements)
            {
                bool satisfiesAllAttributes = true;

                //Check each attribute for the element
                foreach (string attribute in attributeNames)
                {
                    XAttribute nextAttribute = currentElement.Attribute(attribute);
                    if (nextAttribute == null)
                    {
                        satisfiesAllAttributes = false;
                        break;
                    }
                }

                if (satisfiesAllAttributes)
                {
                    filteredElements.Add(currentElement);
                }
            }

            return filteredElements;
        }

        /// <summary>
        /// Filters a list of XElements to a subset which match ANY of the given attributes
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="attributeNames"></param>
        /// <returns></returns>
        public static List<XElement> FilterByAnyAttributes(this IEnumerable<XElement> elements, List<string> attributeNames)
        {
            List<XElement> filteredElements = new List<XElement>();

            foreach (XElement currentElement in elements)
            {
                //Check each attribute for the element
                foreach (string attribute in attributeNames)
                {
                    XAttribute nextAttribute = currentElement.Attribute(attribute);
                    if (nextAttribute != null)
                    {
                        filteredElements.Add(currentElement);
                        break;
                    }
                }
            }

            return filteredElements;
        }

        /// <summary>
        /// Filter by one attribute _OR_ a second attribute
        /// TODO: rename this to something else, and make a list version that takes a list/array of attributes and ors over
        /// TODO: all of them
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="attributeName1"></param>
        /// <param name="attributeName2"></param>
        /// <returns></returns>
        public static List<XElement> FilterByAnyAttributes(this IEnumerable<XElement> elements,
                                                           string attributeName1, string attributeName2)
        {
            List<XElement> filteredElements = new List<XElement>();

            foreach (XElement currentElement in elements)
            {
                XAttribute targetAttribute1 = currentElement.Attribute(attributeName1);
                XAttribute targetAttribute2 = currentElement.Attribute(attributeName2);

                if (targetAttribute1 != null || targetAttribute2 != null)
                {
                    filteredElements.Add(currentElement);
                }
            }

            return filteredElements;
        }

        /// <summary>
        /// Filters the collection of elements by attribute.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns>A filtered list of elements containing the given attribute.</returns>
        public static List<XElement> FilterByAttribute(this IEnumerable<XElement> elements,
                                                       string attributeName)
        {
            List<XElement> filteredElements = new List<XElement>();

            foreach (XElement currentElement in elements)
            {
                XAttribute targetAttribute = currentElement.Attribute(attributeName);

                if (targetAttribute != null)
                {
                    filteredElements.Add(currentElement);
                }
            }

            return filteredElements;
        }

        /// <summary>
        /// Filters the collection of elements by attribute.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="value">The value.</param>
        /// <returns>A filtered list of elements containing the given attribute with the given value.</returns>
        public static List<XElement> FilterByAttribute(this IEnumerable<XElement> elements,
                                                       XName attributeName, string value)
        {
            List<XElement> filteredElements = new List<XElement>();

            foreach (XElement currentElement in elements)
            {
                XAttribute targetAttribute = currentElement.Attribute(attributeName);

                if (targetAttribute != null && targetAttribute.Value == value)
                {
                    filteredElements.Add(currentElement);
                }
            }

            return filteredElements;
        }

        /// <summary>
        /// Filters the collection of elements by attribute.
        /// </summary>
        /// <param name="elements">The elements to filter.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="value">The value.</param>
        /// <returns>A filtered list of elements containing the given attribute with the given value.</returns>
        public static List<XElement> FilterByAttribute(this IEnumerable<XElement> elements,
                                                       string attributeName, string value)
        {
            return FilterByAttribute(elements, (XName)attributeName, value);
        }

        /// <summary>
        /// Filter by the given attributes OR the local name
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="localName"></param>
        /// <param name="attributeName1"></param>
        /// <param name="attributeName2"></param>
        /// <returns></returns>
        public static List<XElement> FilterByLocalNameOrAttributes(this IEnumerable<XElement> elements,
                                                                   string localName, string attributeName1, string attributeName2)
        {
            List<XElement> filteredElements = new List<XElement>();

            foreach (XElement currentElement in elements)
            {
                XAttribute targetAttribute1 = currentElement.Attribute(attributeName1);
                XAttribute targetAttribute2 = currentElement.Attribute(attributeName2);

                if (currentElement.Name.LocalName == localName || targetAttribute1 != null || targetAttribute2 != null)
                {
                    filteredElements.Add(currentElement);
                }
            }

            return filteredElements;
        }
       
        /// <summary>
        /// Gets the previous element of a given element, ignoring any non-element nodes
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XElement PreviousSibling(XElement element)
        {
            XNode siblingNode = element;
            siblingNode = siblingNode.PreviousNode;

            while (siblingNode != null && siblingNode as XElement == null)
            {
                siblingNode = siblingNode.PreviousNode;
            }

            return siblingNode as XElement;
        }

        /// <summary>
        /// Gets the next element of a given element, ignoring any non-element nodes
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XElement NextSibling(XElement element)
        {
            XNode siblingNode = element;
            siblingNode = siblingNode.NextNode;

            while (siblingNode != null && siblingNode as XElement == null)
            {
                siblingNode = siblingNode.NextNode;
            }

            return siblingNode as XElement;
        }

        /// <summary>
        /// Gets the next element of a given element, ignoring any non-element nodes
        /// Returns either the first valid descendant or the next sibling.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XElement NextElement(XElement element)
        {
            if (element.Descendants().Count() > 0)
            {
                return element.Descendants().First();
            }

            return NextSibling(element);
        }

        /// <summary>
        /// Gets the previous element of a given element, ignoring any non-element nodes
        /// Returns either the last previous valid descendant or the next sibling.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XElement PreviousElement(XElement element)
        {
            XElement prevSibling = PreviousSibling(element);

            if (prevSibling != null && prevSibling.Descendants().Count() > 0)
            {
                return prevSibling.Descendants().Last();
            }

            return prevSibling;
        }

        #endregion Methods
    }
}