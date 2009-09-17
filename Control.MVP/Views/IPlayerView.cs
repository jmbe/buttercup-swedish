using System;
using System.Windows;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.UI;
using Control.MVP.Presenters;
using Control.UI;
using Control.UI.Voice;

namespace Control.MVP.Views
{
    public interface IPlayerView
    {
        #region Data Members (8)

        // The Player view needs to be aware of the Application to respond to things like
        // new books loading or the contrast/interface size changing.
        IApplicationView ApplicationView { get; set; }

        /// <summary>
        /// This is the id of the heading for the current section being played.
        /// </summary>
        string CurrentHeadingID { get; set; }

        /// <summary>
        /// Handler for shortcut key events
        /// </summary>
        KeyCommandHandler KeyCommandHandler { get; set; }

        /// <summary>
        /// The Player also needs to know when a user has selected to jump to a new section
        /// via the NavigationView (Table of Contents)
        /// </summary>
        INavigationView NavigationView { get; set; }

        /// <summary>
        /// The Player also needs to know when a user has selected to jump to a new page
        /// via the SearchView
        /// </summary>
        ISearchView SearchView { get; set; }

        /// <summary>
        /// This is the current volume for the speaking voices, set by whatever control in the view.
        /// </summary>
        double Volume { get; set; }

        /// <summary>
        /// The current speaking voice for the player
        /// </summary>
        VoiceBase CurrentVoice { get; set; }

        /// <summary>
        /// Holds whether we are muting _all_ audio
        /// </summary>
        bool AudioMuted { get; set; }

        /// <summary>
        /// This holds whether self-voicing will be used
        /// </summary>
        bool SelfVoicingEnabled { get; set; }

        #endregion Data Members

        #region Delegates and Events (15)

        // Events (15) 

        /// <summary>
        /// Raised by the view when the Audio of the currently played element has
        /// completed.
        /// </summary>
        event EventHandler AudioCompleted;

        /// <summary>
        /// This event is raised when the user wants to go down a heading
        /// </summary>
        event EventHandler DownLevel;

        /// <summary>
        /// This event is raised when the user wants to go to the next page
        /// </summary>
        event EventHandler NextPage;

        /// <summary>
        /// This event is raised when the user wants to go to the next section
        /// </summary>
        event EventHandler NextSection;

        /// <summary>
        /// This event is raised when the user wants to start/resume playing the book (if the book
        /// is paused) or pause the book (if it is playing).
        /// </summary>
        event EventHandler TogglePlayPause;

        /// <summary>
        /// This event is raised when the user wants to go to the previous page
        /// </summary>
        event EventHandler PreviousPage;

        /// <summary>
        /// This event is raised when the user wants to go to the previous section
        /// </summary>
        event EventHandler PreviousSection;

        /// <summary>
        /// This event is raised when the current section being played has changed
        /// The navigation presenter uses this to update the currently highlighted heading
        /// </summary>
        event EventHandler SectionChanged;

        /// <summary>
        /// Raised when wanting to toggle self voicing on/off.
        /// </summary>
        event EventHandler ToggleSelfVoicing;

        /// <summary>
        /// Raised when wanting to mute audio.
        /// </summary>
        event EventHandler ToggleMuting;

        /// <summary>
        /// Raised when some contextual speech text is to be spoken.
        /// </summary>
        event EventHandler<SpeechTextEventArgs> SelfVoicingSpeakText;

        /// <summary>
        /// This event is raised when the user wishes to bookmark the currently playing section
        /// TODO: Actually raise this event
        /// </summary>
        event EventHandler SetBookmark;

        /// <summary>
        /// Raised when a UI element is required to be spoken.
        /// </summary>
        event EventHandler<ElementSelectedEventArgs> SpeakableElementSelected;

        /// <summary>
        /// Raised when a UI element help text is required to be spoken.
        /// </summary>
        event EventHandler<ElementSelectedEventArgs> SpeakableElementSelectedHelpText;

        /// <summary>
        /// Handles the event when tooltips are to be spoken.
        /// TODO: Are we even using this??
        /// </summary>
        event EventHandler<ResolveEventArgs> SpeakToolTip;

        /// <summary>
        /// This event is raised when the user wants to navigate up a heading
        /// </summary>
        event EventHandler UpLevel;

        /// <summary>
        /// This event is raised when the volume value on the view has changed
        /// </summary>
        event EventHandler<VolumeChangedEventArgs> VolumeChanged;

        #endregion Delegates and Events

        #region Operations (9)

        /// <summary>
        /// Pauses the playback of the current book.
        /// </summary>
        void PauseAudioPlayback();

        /// <summary>
        /// Resumes the audio playback of the current phrase.
        /// </summary>
        void ResumeSpeakingPhrase();

        /// <summary>
        /// Sets the state of the navigation buttons.
        /// </summary>
        /// <param name="isEnabled">Determines whether the navigation buttons should be enabled.</param>
        void SetNavButtonState(bool isEnabled);

        /// <summary>
        /// Sets the state of the play button.
        /// </summary>
        /// <param name="isEnabled">Determines whether the button should be enabled.</param>
        /// <param name="isPlaying">Determines whether the button should indicate that the player
        /// is currently playing.</param>
        void SetPlayButtonState(bool isEnabled, bool isPlaying);

        /// <summary>
        /// Sets the presenter reference for the view.
        /// </summary>
        /// <param name="presenter">The presenter to interact with.</param>
        void SetPresenterReference(PlayerPresenter presenter);

        /// <summary>
        /// Method to speak the given element. Indirectly used by all the presenters to speak UI elements.
        /// </summary>
        /// <param name="dependencyObject">The dependency object containing details of what to speak.</param>
        /// <param name="isBookPlaying">Indicates whether or not the current book is currently playing.</param>
        void SpeakElement(DependencyObject dependencyObject, bool isBookPlaying);

        /// <summary>
        /// Method to speak the given element's help text. Indirectly used by all the presenters to speak UI elements.
        /// </summary>
        /// <param name="dependencyObject">The dependency object containing details of what to speak.</param>
        /// <param name="isBookPlaying">Indicates whether or not the current book is currently playing.</param>
        void SpeakElementHelpText(DependencyObject dependencyObject, bool isBookPlaying);

        /// <summary>
        /// Method to speak the given text string. Indirectly used by all the presenters to speak contextual text speech.
        /// </summary>
        /// <param name="Speech">The text to be spoken.</param>
        /// <param name="isBookPlaying">Indicates whether or not the current book is currently playing.</param>
        void SpeakTextSpeech(string Speech, bool isBookPlaying);

        /// <summary>
        /// Starts the audio playback of the given phrase.
        /// </summary>
        /// <param name="currentPhrase">The phrase to speak.</param>
        void StartSpeakingPhrase(Phrase currentPhrase);

        /// <summary>
        /// Stops the audio playback of the current phrase.
        /// </summary>
        void StopAudioPlayback();

        #endregion Operations
    }
}