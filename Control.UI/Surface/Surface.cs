using System;
using System.IO;
using System.Xml.Linq;
using Buttercup.Control.Model;
using Control.UI;
using Control.UI.Gestures;

namespace Buttercup.Control.UI
{
    /// <summary>
    /// Represents the surface onto which a book will be displayed. Currently this is
    /// going to be an HTML serface only but could be extended to be XAML.
    /// </summary>
    public abstract class Surface
    {
		#region Properties (2) 

        /// <summary>
        /// Gets or sets the book fragment.
        /// </summary>
        /// <value>The book fragment.</value>
        public XDocument BookFragment { get; set; }

        public double Scale { get; protected set; }

		#endregion Properties 

		#region Delegates and Events (4) 

		// Events (4) 

        /// <summary>
        /// This event should be raised by sub-classes when a gesture is recognised on their surface
        /// interface.
        /// </summary>
        public virtual event EventHandler<MouseGestureEventArgs> GestureRaised;

        public virtual event EventHandler<BookElementSelectedEventArgs> ItemSelected;

        public virtual event EventHandler PreImageRendered;

        public virtual event EventHandler RenderComplete;

        public virtual event EventHandler DrawFailed;

		#endregion Delegates and Events 

		#region Methods (7) 

		// Public Methods (7) 

        /// <summary>
        /// Implemented by subclasses to check to see if a certain element exists on the surface
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract bool ElementExists(string id);

        /// <summary>
        /// Implemented by subclasses to check to see if a certain element is highlightable on the surface
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract bool ElementHighlightable(string id);

        /// <summary>
        /// Given a navigatable element, returns the hightable surface element that will contain that element
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract string GetHighlightableElementOfElement(string id);

        /// <summary>
        /// Implemented by subclasses to obtain the id of the currently highlightable element
        /// </summary>
        /// <returns></returns>
        public abstract string CurrentHighlightableElementID();

        /// <summary>
        /// Implemented by subclasses to draw the book fragment onto the surface.
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Implemented by subclasses to highlight a specific part of the book
        /// fragment, for example, a sentence.
        /// </summary>
        /// <param name="id">The id as specified in the Book Fragment XML node.</param>
        public abstract void Highlight(string id);

        /// <summary>
        /// Implemented by subclasses to set the contrast of the surface. Different
        /// levels of contrast are more readible to certain users.
        /// </summary>
        /// <param name="level">The contrast level to set on the surface.</param>
        public abstract void SetContrast(ContrastLevel level);

        /// <summary>
        /// Implemented by subclasses to ensure that the current element is
        /// visible within the surface. Surfaces will often be vertically scrollable
        /// to cater for large books.
        /// </summary>
        public abstract void SetFocus();

        /// <summary>
        /// Set the image source of a specific image to the specified Stream source. 
        /// </summary>
        /// <remarks>
        /// This is useful when the underlying img src values are not browesable. You can
        /// subscribe to the PreImageRendered event and set the image source programmatically.
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="source"></param>
        public abstract void SetImageSource(string id, Stream source);

        /// <summary>
        /// Implemented by subclasses to set the scale of the book surface rendering. Setting to
        /// 1 displays unscaled text/images. Increasing the scale increase the font/text size.
        /// </summary>
        public abstract void SetScale(double scale);

		#endregion Methods 
    }
}