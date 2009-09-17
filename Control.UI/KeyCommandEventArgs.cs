using System;
using System.Windows.Input;

namespace Control.MVP
{
    public class KeyCommandEventArgs : EventArgs
    {
        #region Constructors (1)

        public KeyCommandEventArgs(KeyEventArgs e)
        {
            SubViewHandled = false;
            keyEventArgs = e;
        }

        #endregion Constructors

        #region Properties (2)

        public KeyEventArgs keyEventArgs { get; private set; }

        /// <summary>
        /// Indicates whether a sub view has already handled this key event
        /// Differs from regular KeyEventArgs Handled boolean in that UI elements
        /// don't use it, and so a key press event can be handled by a subview
        /// without interfering with UI control behaviours.
        /// </summary>
        public Boolean SubViewHandled { get; set; }

        #endregion Properties
    }
}