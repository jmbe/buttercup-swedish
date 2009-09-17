namespace Buttercup.Control.Model
{
    public class SearchState
    {
        public const int FocusGoToPage = 0;
        public const int FocusFindText = 1;

		#region Properties (1) 

        //TODO Should search and page navigation logic be place in here instead of in playerstate/playerpresenter?
        /// <summary>
        /// Gets the navigator used to navigate the current book.
        /// </summary>
        /// <value>The navigator.</value>
        public BookNavigator Navigator { get; internal set; }

        /// <summary>
        /// Holds the item which will be focused when the panel opens
        /// </summary>
        public int FocusedItem { get; set; }

		#endregion Properties 
    }
}