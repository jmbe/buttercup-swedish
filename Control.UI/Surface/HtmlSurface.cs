using System;
using System.IO;
using System.Windows.Browser;
using Buttercup.Control.Model;

namespace Buttercup.Control.UI
{
    /// <summary>
    /// This class represents the HTML surface onto which a book fragment will be
    /// displayed.
    /// </summary>
    public class HtmlSurface : Surface
    {
        #region Fields (4)

        public HtmlElement _container;
        private string _currentElementClass;
        private HtmlElement _currentlyHighlightedElement;
        private const string _HIGHLIGHT_STYLE_CLASS = "highlight";

        #endregion Fields

        #region Constructors (1)

        /// <summary>
        /// Constructor to instantiate the surface with specifc book fragment.
        /// </summary>
        public HtmlSurface(HtmlElement container)
        {
            if (container == null)
            {
                // TODO: Specialized exception?
                throw new Exception("Yikes");
            }

            Container = container;
            _currentlyHighlightedElement = null;
            _currentElementClass = String.Empty;
        }

        #endregion Constructors

        #region Properties (2)

        /// <summary>
        /// Gets/sets the HTML container that the book fragment should be displayed in.
        /// </summary>
        public HtmlElement Container
        {
            get { return _container; }
            set { _container = value; }
        }

        /// <summary>
        /// Returns the HTML for the given book fragment. This is acheived through
        /// transforming the XML fragment into HTML using the DaisyToXhtml xsl
        /// transformation. Since Silverlight doesn't have native XSL transformation
        /// functionality we need to call out to a javascript implementation.
        /// </summary>
        public string Html { get; private set; }

        #endregion Properties

        #region Methods (7)

        // Public Methods (6) 

        /// <summary>
        /// Draws the book fragment on the surface. This is usually called when the surface
        /// is first initialised.
        /// </summary>
        public override void Draw()
        {
            Html = TransformBookFragment();
        }

        /// <summary>
        /// Highlights a specific element within the book fragment. Highlighting can be
        /// achieved by adding a class the the corresponding HTML element called, say,
        /// highligh. This can be picked up by the buttercup.css stylesheet to set the
        /// background of the element.
        /// </summary>
        /// <param name="id">The id as specified in the HTML element.</param>
        public override void Highlight(string id)
        {
            //Unhighlight the currently-highlighted item - restore the original style.
            if (_currentlyHighlightedElement != null)
            {
                _currentlyHighlightedElement.CssClass = _currentElementClass;
            }

            if (String.IsNullOrEmpty(id))
            {
                //No element is highlighted
                _currentlyHighlightedElement = null;
                return;
            }

            HtmlElement targetElement = HtmlPage.Document.GetElementById(id);

            if (targetElement != null)
            {
                //Cache existing CSS class(es)
                _currentElementClass = targetElement.CssClass;

                if (!String.IsNullOrEmpty(_currentElementClass))
                    targetElement.CssClass = _currentElementClass + " " + _HIGHLIGHT_STYLE_CLASS;
                else
                    targetElement.CssClass = _HIGHLIGHT_STYLE_CLASS;

                _currentlyHighlightedElement = targetElement;
            }
            else
            {
                // What if we can't?
            }
        }

        /// <summary>
        /// Changes the contrast level of the surface. Typically this is a matter of
        /// changing the background and foreground colours. This could be acheived by
        /// setting CSS classes on the DivContainer.
        /// </summary>
        /// <param name="contrast">The contrast level to set on the surface.</param>
        public override void SetContrast(ContrastLevel level)
        {
            Container.CssClass = level.ToString();
        }

        /// <summary>
        /// Sets the focus to the currently highlighted element.
        /// Even if the element is not visible within
        /// the scrollable DivContainer, it must be moved to the top of the container (via
        /// javascript?).
        /// </summary>
        public override void SetFocus()
        {
			if(_currentlyHighlightedElement != null)
			{
				HtmlPage.Window.CreateInstance("ScrollToElement", _currentlyHighlightedElement.Id);
			}
        }

        public override void SetImageSource(string id, Stream source)
        {
            throw new NotImplementedException();
        }

        public override bool ElementExists(string id)
        {
            throw new NotImplementedException();
        }

        public override bool ElementHighlightable(string id)
        {
            throw new NotImplementedException();
        }

        public override string GetHighlightableElementOfElement(string id)
        {
            throw new NotImplementedException();
        }

        public override string CurrentHighlightableElementID()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implemented by subclasses to set the scale of the output of the book.
        /// </summary>
        /// <param name="scale"></param>
        public override void SetScale(double scale)
        {
            int percentage = Convert.ToInt32(100 * scale);

            string percentageExpression = String.Format("{0}%", percentage);
            Container.SetStyleAttribute("fontSize", percentageExpression);
        }
        // Private Methods (1) 

        /// <summary>
        /// Transforms the BookFragment into HTML
        /// </summary>
        /// <returns></returns>
        private string TransformBookFragment()
        {
            string html = String.Empty;
            try
            {
                string xmlFragment = BookFragment.ToString();


                // Transform the book's xml using an xsl transformation (Web/dtbook2html.xsl)
                // that is invoked on the client (since Silverlight doesn't have this functionality)
                HtmlPage.Window.Invoke("TransformFragment", xmlFragment, _container.Id);

                //ScriptObject output = HtmlPage.Window.CreateInstance("TransformFragment", _bookFragment);
            }
            catch (Exception e)
            {
                html =
                    "<p>This is some sample text to show rendering the HTML content of a DAISY DTB. This placeholder text is displayed because there was a problem processing the book fragment for output.</p><p>Error message details are as follows:</p><p>" +
                    e.Message + "</p>";
            }

            return html;
        }

        #endregion Methods
    }
}
