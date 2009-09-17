using Buttercup.Control.Common.IO;
using Buttercup.Control.Model.Entities;

namespace Buttercup.Control.Model
{
    public class ApplicationState
    {
        #region Fields (2)

        private readonly object _atomicBookLock = new object();
        private Book _currentBook;

        #endregion Fields

        #region Constructors (1)

        public ApplicationState()
        {
            IsServerHostedMode = false; //Assume locally-hosted book mode by default.
            SidePanelMode = SidePanelDisplayMode.Hidden;

            PlayerState = new PlayerState(this);
            DisplaySettingsState = new DisplaySettingsState();
            SearchState = new SearchState();
            NavigationState = new NavigationState();
        }

        #endregion Constructors

        #region Properties (9)

        public IFileSystem ApplicationFileSystem { get; set; }

        public IFileSystem BookFileSystem { get; set; }

        /// <summary>
        /// The currently loaded book
        /// </summary>
        public Book CurrentBook
        {
            get
            {
                Book tempBook;

                lock (_atomicBookLock)
                {
                    tempBook = _currentBook;
                }
                return tempBook;
            }
            set
            {
                lock (_atomicBookLock)
                {
                    _currentBook = value;
                    PlayerState.Navigator = new BookNavigator(_currentBook);
                    SearchState.Navigator = PlayerState.Navigator;
                }
            }
        }

        /// <summary>
        /// Gets the state relating to the display settings.
        /// </summary>
        /// <value>The state relating to the display settings.</value>
        public DisplaySettingsState DisplaySettingsState { get; private set; }

        public IFileSystem FileSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Buttercup instance is in server-hosted mode.
        /// </summary>
        /// <value>
        /// True if the Buttercup instance is in server-hosted mode, otherwise false.
        /// </value>
        public bool IsServerHostedMode { get; set; }

        /// <summary>
        /// Whether we were in server hosted mode prior to attempting to load a book
        /// </summary>
        public bool PreviousServerHostedMode { get; set; }

        /// <summary>
        /// Gets the state relating to the player.
        /// </summary>
        public PlayerState PlayerState { get; private set; }

        /// <summary>
        /// Gets the state relating to the search component.
        /// </summary>
        /// <value>The state relating to the search component.</value>
        public SearchState SearchState { get; private set; }

        public NavigationState NavigationState { get; private set; }

        /// <summary>
        /// Gets or sets the display mode of the side panel.
        /// </summary>
        /// <value>The side panel display mode.</value>
        public SidePanelDisplayMode SidePanelMode { get; set; }

        #endregion Properties
    }
}