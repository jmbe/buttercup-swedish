using System;
using System.Windows.Browser;

namespace Buttercup.Control.UI
{
    public class HtmlSurfaceProxy
    {
        #region Delegates and Events (1)

        // Events (1) 

        /// <summary>
        /// Occurs when the book fragment has finished being rendered.
        /// </summary>
        public event EventHandler BookDisplayed;

        #endregion Delegates and Events

        #region Methods (1)

        // Public Methods (1) 

        /// <summary>
        /// Indicates that the book fragment has finished being displayed.
        /// </summary>
        [ScriptableMember]
        public void DisplayBookCompleted()
        {
            if (BookDisplayed != null)
                BookDisplayed(this, new EventArgs());
        }

        #endregion Methods
    }
}