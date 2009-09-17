using System;
using Buttercup.Control.Model;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.UI;
using Control.MVP.Presenters;

namespace Control.MVP.Views
{
    public interface IApplicationView
    {
        #region Data Members (8)

        /// <summary>
        /// The Reader to use for the application.
        /// TODO: Get rid of this (obsolete)
        /// </summary>
        Reader BookReader { get; set; }

        /// <summary>
        /// Gets the display surface.
        /// </summary>
        /// <value>The display surface where the book fragment is rendered.</value>
        Surface DisplaySurface { get; }

        /// <summary>
        /// Handler for shortcut key events
        /// </summary>
        KeyCommandHandler KeyCommandHandler { get; set; }

        /// <summary>
        /// The percentage progess of a Save operation.
        /// </summary>
        double SaveProgress { get; set; }

        /// <summary>
        /// A message to accompany the SaveProgress 
        /// </summary>
        string SaveProgressMessage { get; set; }

        /// <summary>
        /// Set by the View to indicate to the presenter that we want to load a book from the 
        /// server.
        /// </summary>
        string ServerBookReference { get; set; }

        /// <summary>
        /// Indicates whether we are waiting on something to finish running/loading, and reflect that status to the user
        /// </summary>
        bool WaitingOnProgress { set; }

        #endregion Data Members

        #region Delegates and Events (7)

        // Events (7) 

        /// <summary>
        /// Event raised when the CurrentBook has changed.
        /// </summary>
        event EventHandler BookChanged;

        /// <summary>
        /// Event raised with a book has been completed rendered.
        /// </summary>
        event EventHandler BookDisplayed;

        /// <summary>
        /// This event is raised when a user starts to load a new book. This gives this view
        /// (and dependent ones) a chance to change their interface. For example, the playerView will
        /// disable its buttons to prevent conflicting user actions.
        /// </summary>
        event EventHandler BookLoadStarted;

        /// <summary>
        /// This event is raised after the BookLoadStarted event if the load process fails. This will
        /// enable this view (and dependent ones) to reset their user interfaces. For example, the 
        /// Applicationview can re-enable the Open button.
        /// </summary>
        event EventHandler BookLoadFailed;

        /// <summary>
        /// Event raised to advise that a stream referencing the zip pacakge is ready to be loaded.
        /// </summary>
        event EventHandler<LoadZipPackageEventArgs> LoadZipPackage;

        /// <summary>
        /// Event raised to advise that a package file (opf) has been selected to load.
        /// </summary>
        event EventHandler<LoadPackageEventArgs> LoadPackage;

        /// <summary>
        /// Raised when a particular panel is selected, or when the panel is hidden.
        /// </summary>
        /// <remarks>A display mode of Hidden will be specified when hiding the panel.</remarks>
        event EventHandler<PanelDisplayModeEventArgs> SelectPanel;

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


        event EventHandler<PanelFocusedItemEventArgs> DisplaySettingsFocusChanged;
        event EventHandler<PanelFocusedItemEventArgs> SearchViewFocusChanged;
        event EventHandler<PanelFocusedItemEventArgs> NavigationViewFocusChanged;

        #endregion Delegates and Events

        #region Operations (8)

        /// <summary>
        /// Applies the contrast setting to the entire interface.
        /// </summary>
        /// <param name="contrastScheme">The contrast scheme.</param>
        void ApplyContrastSetting(ContrastLevel contrastScheme);

        /// <summary>
        /// Applies the interface size to the entire interface.
        /// </summary>
        /// <param name="interfaceSize">The interface size.</param>
        void ApplyInterfaceSize(int interfaceSize);

        /// <summary>
        /// Get's a reference to a SearchView. The ApplicationPresenters requests this from the 
        /// ApplicationView to configure the other dependent Presenters.
        /// </summary>
        ISearchView SearchView { get; }

        /// <summary>
        /// Get's a reference to a NavigationView. The ApplicationPresenters requests this from the 
        /// ApplicationView to configure the other dependent Presenters.
        /// </summary>
        INavigationView NavigationView { get; }

        /// <summary>
        /// Get's a reference to a DisplaySettingsView. The ApplicationPresenters requests this from the 
        /// ApplicationView to configure the other dependent Presenters.
        /// </summary>
        IDisplaySettingsView DisplaySettingsView { get; }

        /// <summary>
        /// Get's a reference to a Player View. The ApplicationPresenters requests this from the 
        /// ApplicationView to configure the other dependent Presenters.
        /// </summary>
        IPlayerView PlayerView { get; }

        /// <summary>
        /// Hides the side panel.
        /// </summary>
        /// <remarks>If the panel is already hidden, it will stay hidden.</remarks>
        void HideSidePanel();



		/// <summary>
		/// Notifies that the panel contents have changed.
		/// </summary>
    	void NotifyPanelContentsChanged();

        /// <summary>
        /// Sets the state of the control buttons (i.e. Open Book).
        /// </summary>
        /// <param name="isEnabled">If true, enables the control buttons that relate to the current book.
        /// This does not affect buttons relating to the application itself, e.g. contrast.</param>
        void SetControlButtonState(bool isEnabled);

        /// <summary>
        /// Toggle (Hide/Show) the side panel corresponding to the given display mode.
        /// </summary>
        /// <param name="displayMode">The display mode.</param>
        /// <remarks>If the display mode is Hidden, then the side panel will be hidden.</remarks>
        void ToggleSidePanel(SidePanelDisplayMode displayMode);

        #endregion Operations
    }
}
