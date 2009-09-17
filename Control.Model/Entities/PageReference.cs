using System;

namespace Buttercup.Control.Model.Entities
{
    public class PageReference : ISpeakable, IComparable<PageReference>
    {
		#region Properties (6) 

        /// <summary>
        /// Gets or sets the absolute page order.
        /// </summary>
        /// <value>The absolute order value for the page in the page collection.</value>
        public int AbsoluteOrder { get; set; }

        public Audio Audio { get; set; }

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        /// <value>The page number.</value>
        /// <remarks>(A2S) There may be other pages with the same number in the same collection.
        /// If that is the case, the PageType MUST be different.</remarks>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the page type (i.e. front, normal or special).
        /// </summary>
        /// <value>The type of the page.</value>
        public PageType PageType { get; set; }

        public string SmilReference { get; set; }

        public string Text { get; set; }

		#endregion Properties 

		#region Methods (1) 

		// Public Methods (1) 

        /// <summary>
        /// Compares the current PageReference with another given PageReference.
        /// </summary>
        /// <param name="other">The other given PageReference.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the PageReferences being compared.
        /// The return value has the following meanings:
        /// Less than zero: This instance is less than / before the other PageReference.
        /// Zero: This instance is equivalent to the other PageReference.
        /// Greater than zero: This instance is greater than / after the other PageReference.
        /// </returns>
        public int CompareTo(PageReference other)
        {
            return (AbsoluteOrder - other.AbsoluteOrder);
        }

		#endregion Methods 
    }
}