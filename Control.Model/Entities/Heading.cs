using System.Collections.Generic;

namespace Buttercup.Control.Model.Entities
{
    /// <summary>
    /// Represents a heading in a Table of Contents.
    /// </summary>
    public class Heading : ISpeakable
    {
        #region Fields (1)

        /// <summary>
        /// Holds the subheadings (headings of lower level) for this heading
        /// </summary>
        private List<Heading> _subHeadings;

        #endregion Fields

        #region Properties (6)

        public Audio Audio { get; set; }

        public string ID { get; set; }

        public int level { get; set; }

        /// <summary>
        /// Gets or sets the smil reference that links the NCX headers to the content in the book XML.
        /// </summary>
        /// <value>The smil reference.</value>
        public string SmilReference { get; set; }

        public List<Heading> SubHeadings
        {
            get
            {
                if (_subHeadings == null)
                {
                    _subHeadings = new List<Heading>();
                }
                return _subHeadings;
            }
            set { _subHeadings = value; }
        }

        public string Text { get; set; }

        #endregion Properties
    }
}