namespace Control.MVP
{
    public class SearchResult
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for SearchResult object, which holds the basic information
        /// of a search result.
        /// </summary>
        /// <param name="searchstring">String of the terms that were searched for</param>
        /// <param name="text">The text for the matching search result</param>
        /// <param name="id">The reference id for the search result</param>
        public SearchResult(string searchstring, string text, string id)
        {
            SearchString = searchstring;
            Text = text;
            ID = id;
        }

        #endregion Constructors

        #region Properties (3)

        public string ID { get; private set; }

        public string SearchString { get; private set; }

        public string Text { get; private set; }

        #endregion Properties
    }
}