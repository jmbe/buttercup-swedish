using System;

namespace Control.MVP
{
    public class PanelFocusedItemEventArgs : EventArgs
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for the SearchEventArgs, this EventArgs is used to pass
        /// a string of terms which will be used in a search.
        /// </summary>
        /// <param name="focusedItem"></param>
        public PanelFocusedItemEventArgs(int focusedItem)
        {
            FocusedItem = focusedItem;
        }

        #endregion Constructors

        #region Properties (1)

        public int FocusedItem { get; set; }

        #endregion Properties
    }
}