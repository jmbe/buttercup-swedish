using System;

namespace Control.MVP
{
    public class SearchEventArgs : EventArgs
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for the SearchEventArgs, this EventArgs is used to pass
        /// a string of terms which will be used in a search.
        /// </summary>
        /// <param name="searchString"></param>
        public SearchEventArgs(string searchString)
        {
            SearchString = searchString;
        }

        #endregion Constructors

        #region Properties (1)

        public string SearchString { get; set; }

        #endregion Properties
    }
}