using System;
using System.Collections.Generic;
using Buttercup.Control.UI;
using Control.UI;

namespace Control.MVP.Views
{
    public interface ISearchView
    {
        #region Data Members (3)

        /// <summary>
        /// Handler for shortcut key events
        /// </summary>
        KeyCommandHandler KeyCommandHandler { get; set; }

        /// <summary>
        /// Holds the search results list that the view uses to display.
        /// </summary>
        List<SearchResult> SearchResults { get; set; }

        /// <summary>
        /// Holds an integer describing which control should be the focus upon display on this view
        /// </summary>
        int FocusedItem { get; set; }

        IApplicationView ApplicationView { get; set; }

        /// <summary>
        /// Inform the user that the page number they want to navigate to is invalid
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="totalPages"></param>
        void PageNavigationOutOfRange(int pageNum, int totalPages);

        #endregion Data Members

        #region Delegates and Events (6)

        // Events (6) 

		/// <summary>
		/// Raised when the list of search results has changed.
		/// </summary>
    	event EventHandler IndicateSearchResultsChanged;

        /// <summary>
        /// Raised when wanting to navigate to a different page
        /// </summary>
        event EventHandler<PageNumEventArgs> NavigateToPage;

        /// <summary>
        /// Raised when wanting to search the book for sections matching the search terms
        /// </summary>
        event EventHandler<SearchEventArgs> SearchForSection;

        /// <summary>
        /// Raised when wanting to navigate to a selected search result
        /// </summary>
        event EventHandler<ItemSelectedEventArgs> SearchSelected;

        /// <summary>
        /// Raised when some contextual speech text is to be spoken.
        /// </summary>
        event EventHandler<SpeechTextEventArgs> SelfVoicingSpeakText;

        /// <summary>
        /// Raised when a UI element is required to be spoken.
        /// </summary>
        event EventHandler<ElementSelectedEventArgs> SpeakableElementSelected;

        /// <summary>
        /// Raised when a UI element help text is required to be spoken.
        /// </summary>
        event EventHandler<ElementSelectedEventArgs> SpeakableElementSelectedHelpText;

        #endregion Delegates and Events



		/// <summary>
		/// Clears the list of search results. Usually called when opening a new book.
		/// </summary>
    	void ClearSearchResults();
    }
}