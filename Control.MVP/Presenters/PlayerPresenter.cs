using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Buttercup.Control.Common;
using Buttercup.Control.Model;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.UI;
using Control.MVP.Views;
using Control.UI;
using Control.UI.Gestures;
using System.Windows.Threading;

namespace Control.MVP.Presenters
{
    public class PlayerPresenter : Presenter
    {
		#region Fields (3) 

        private readonly ApplicationPresenter _mainPresenter;
        private const string _SMIL_REF_ATTR_NAME = "smilref";
        private readonly PlayerState _state;

        private DispatcherTimer _timer;

		#endregion Fields 

		#region Constructors (1) 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="mainPresenter"></param>
        /// <param name="mainState"></param>
        public PlayerPresenter(IPlayerView view, ApplicationPresenter mainPresenter, ApplicationState mainState)
        {
            View = view;
            View.SetPresenterReference(this);

            // View events
            View.DownLevel += View_DownLevel;
            View.UpLevel += View_UpLevel;
            View.NextSection += View_NextSection;
            View.PreviousSection += View_PreviousSection;
            View.TogglePlayPause += View_TogglePlayPause;
            View.SetBookmark += View_SetBookmark;
            View.AudioCompleted += View_AudioCompleted;
            View.VolumeChanged += View_VolumeChanged;
            View.PreviousPage += View_PreviousPage;
            View.NextPage += view_NextPage;
            View.ToggleSelfVoicing += new EventHandler(View_ToggleSelfVoicing);
            View.ToggleMuting += new EventHandler(View_ToggleMuting);

            View.SelfVoicingSpeakText += View_SelfVoicingSpeakText;
            View.SpeakableElementSelected += View_SpeakableElementSelected;
            View.SectionChanged += View_SectionChanged;

            // Hook into relevent dependent view events
            View.ApplicationView.BookChanged += ApplicationView_BookChanged;
            View.ApplicationView.BookLoadStarted += ApplicationView_BookLoadStarted;
            View.ApplicationView.BookLoadFailed += ApplicationView_BookLoadFailed;
            View.ApplicationView.BookDisplayed += ApplicationView_BookDisplayed;

            View.ApplicationView.DisplaySurface.ItemSelected += DisplaySurface_ItemSelected;
            View.ApplicationView.DisplaySurface.GestureRaised += DisplaySurface_GestureRaised;
            View.NavigationView.ItemSelected += NavigationView_ItemSelected;

            View.SearchView.NavigateToPage += SearchView_NavigateToPage;
            View.SearchView.SearchForSection += SearchView_SearchForSection;
            View.SearchView.SearchSelected += SearchView_SearchSelected;

            // Initialise Button State
            View.SetNavButtonState(false);
            View.SetPlayButtonState(false, false);


            _mainPresenter = mainPresenter;
            base.MainState = mainState;
            _state = MainState.PlayerState;
            _state.PresentPhrase = PlayPhrase;
            
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0);
            _timer.Tick += new EventHandler(MoveNextEvent);
        }

		#endregion Constructors 

		#region Properties (1) 

        private IPlayerView View { get; set; }

		#endregion Properties 

		#region Methods (43) 

		// Public Methods (1) 

        public PlayerMode GetCurrentPlayerState()
        {
            return _state.CurrentMode;
        }
		// Private Methods (38) 

        /// <summary>
        /// A new book has been loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplicationView_BookChanged(object sender, EventArgs e)
        {
            Book currentBook = base.MainState.CurrentBook;

            Surface displaySurface = View.ApplicationView.DisplaySurface;

            //displaySurface.BookFragment = currentBook.BookXml;

            //Determine the initial playback state.
            if (!_state.Navigator.AtEndOfBook)
                _state.CurrentMode = PlayerMode.Initialised;
            else
                _state.CurrentMode = PlayerMode.Finished;

            View.SetNavButtonState(!_state.Navigator.AtEndOfBook);
        }

        /// <summary>
        /// This handler is raised when the ApplicationView has finished rendering the book. This
        /// is a cue for the Player to start playing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplicationView_BookDisplayed(object sender, EventArgs e)
        {
            // The book should play as soon as it has been rendered on the surface.

            // 1. Set the highlight on the Surface to the CurrentPosition
            HighlightCurrentPosition();
            TryPlay();
        }

        /// <summary>
        /// This handler will reset the UI based on a failed Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplicationView_BookLoadFailed(object sender, EventArgs e)
        {
            if (base.MainState.CurrentBook != null)
            {
                // A load failed but a book was already loaded. Make sure we can still play
                // the book.
                _state.CurrentMode = PlayerMode.Paused;
                View.SetNavButtonState(true);
                View.SetPlayButtonState(true, false);
            }
            else
            {
                // A load failed and their was no book loaded. We can leave everything as it
                // was because the ApplicationView_BookLoadStarted method will have disabled everything.
            }
        }

        void ApplicationView_BookLoadStarted(object sender, EventArgs e)
        {
            // Disable the player
            DisablePlayer();
        }

        /// <summary>
        /// Disables the player to prevent user interaction while an essential operation completes.
        /// </summary>
        private void DisablePlayer()
        {
            _state.CurrentMode = PlayerMode.Disabled;
            View.SetNavButtonState(false);
            View.SetPlayButtonState(false, false);
        }

        void DisplaySurface_GestureRaised(object sender, Control.UI.Gestures.MouseGestureEventArgs e)
        {
            switch (e.Gesture)
            {
                case MouseGesture.Up:
                    {
                        UpLevel();
                        break;
                    }
                case MouseGesture.Down:
                    {
                        DownLevel();
                        break;
                    }
                case MouseGesture.Left:
                    {
                        PreviousSection();
                        break;
                    }
                case MouseGesture.Right:
                    {
                        NextSection();
                        break;
                    }
            }
        }

        /// <summary>
        /// This Handler is called when a user selects a specific item on the book surface.
        /// The surface will highlight the item but we need to tell the Player to start playing
        /// it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplaySurface_ItemSelected(object sender, BookElementSelectedEventArgs e)
        {
            if (_state.Navigator != null)
            {
                _state.Navigator.SeekToElementId(e.Id);

                SetCurrentSectionHeading();

                // 1. Set the highlight on the Surface to the CurrentPosition
                HighlightCurrentPosition();
                TryPlay();
            }
        }

        private void DownLevel()
        {
            if (_state.Navigator != null)
            {
                _state.Navigator.DownLevel();
                SetCurrentSectionHeading();

                // 1. Set the highlight on the Surface to the CurrentPosition
                HighlightCurrentPosition();
                TryPlay();
            }
        }

        /// <summary>
        /// Finishes the book.
        /// </summary>
        private void FinishBook()
        {
            _state.CurrentMode = PlayerMode.Finished;

            Surface displaySurface = View.ApplicationView.DisplaySurface;
            displaySurface.Highlight(String.Empty); //This will remove the highlight from the current element.

            View.SetPlayButtonState(_state.CanPlay, _state.IsPlaying);
        }

        private void HighlightCurrentPosition()
        {
            Surface displaySurface = View.ApplicationView.DisplaySurface;
            //displaySurface.SetFocus(_state.Navigator.CurrentElementID);
            displaySurface.Highlight(_state.Navigator.CurrentElementID);
        }

        private void MoveNext()
        {
            if (_state.Navigator != null)
            {
                _timer.Start();

                /*_state.Navigator.MoveNext();

                SetCurrentSectionHeading();

                if (_state.IsPlaying)
                {
                    // 1. Set the highlight on the Surface to the CurrentPosition
                    HighlightCurrentPosition();
                    TryPlay();
                }*/
            }
        }

        private void MoveNextEvent(object sender, EventArgs e)
        {
            _state.Navigator.MoveNext();

            SetCurrentSectionHeading();

            if (_state.IsPlaying)
            {
                // 1. Set the highlight on the Surface to the CurrentPosition
                HighlightCurrentPosition();
                TryPlay();
            }
        }

        private void NavigationView_ItemSelected(object sender, ItemSelectedEventArgs e)
        {
            if (_state.Navigator != null)
            {
                _state.Navigator.SeekToHeadingByID(e.ID);

                // 1. Set the highlight on the Surface to the CurrentPosition
                HighlightCurrentPosition();
                TryPlay();
            }
        }

        private void NextPage()
        {
            if (_state.Navigator != null)
            {
                _state.Navigator.NextPage();
                SetCurrentSectionHeading();

                // 1. Set the highlight on the Surface to the CurrentPosition
                HighlightCurrentPosition();
                TryPlay();
            }
        }

        private void NextSection()
        {
            if (_state.Navigator != null)
            {
                string currentHighlight = View.ApplicationView.DisplaySurface.GetHighlightableElementOfElement(_state.Navigator.CurrentElementID);
                _state.Navigator.MoveNext();
                string newHighlight = View.ApplicationView.DisplaySurface.GetHighlightableElementOfElement(_state.Navigator.CurrentElementID);

                if (_state.Navigator.AtEndOfBook)
                {
                    HighlightCurrentPosition();
                    TryPlay();
                    return;
                }

                //|| !View.ApplicationView.DisplaySurface.ElementHighlightable(_state.Navigator.CurrentElementID)
                if (!View.ApplicationView.DisplaySurface.ElementExists(_state.Navigator.CurrentElementID)
                    || (currentHighlight == newHighlight))
                {
                    NextSection();
                }
                else
                {
                    SetCurrentSectionHeading();
                    HighlightCurrentPosition();
                    TryPlay();
                }
            }
        }

        /// <summary>
        /// The player is being disabled (e.g. while a book is loading).
        /// </summary>
        private void PlayerDisabled(object sender, EventArgs e)
        {
            DisablePlayer();
        }

        /// <summary>
        /// This method is called when an asynchronous request for an audio element has completed.
        /// </summary>
        /// <param name="currentPhrase"></param>
        /// <param name="canResumePhrase"></param>
        private void PlayPhrase(Phrase currentPhrase, bool canResumePhrase)
        {
            Logger.Log("Playing phrase of element: {0}", currentPhrase.ElementID);

            // If in the meantime the navigator has moved to another element then don't even bother trying
            // to play it.
            if (_state.Navigator.CurrentElementID == currentPhrase.ElementID)
            {
                //Only want to execute speaking phrases if the element exists on the surface.
                if (View.ApplicationView.DisplaySurface.ElementExists(_state.Navigator.CurrentElementID) || currentPhrase.Silent)
                {
                    //Don't want to reset player position if the player was previously paused.
                    if (!canResumePhrase)
                        View.StartSpeakingPhrase(currentPhrase);
                    else
                        View.ResumeSpeakingPhrase();

                    View.SetPlayButtonState(_state.CanPlay, _state.IsPlaying);
                }
                else
                {
                    //Move to next element
                    MoveNext();
                }
            }
        }

        private void PreviousPage()
        {
            if (_state.Navigator != null)
            {
                _state.Navigator.PrevPage();
                SetCurrentSectionHeading();

                // 1. Set the highlight on the Surface to the CurrentPosition
                HighlightCurrentPosition();
                TryPlay();
            }
        }

        private void PreviousSection()
        {
            if (_state.Navigator != null)
            {
                string currentHighlight = View.ApplicationView.DisplaySurface.GetHighlightableElementOfElement(_state.Navigator.CurrentElementID);
                _state.Navigator.MovePrevious();
                string newHighlight = View.ApplicationView.DisplaySurface.GetHighlightableElementOfElement(_state.Navigator.CurrentElementID);

                if (_state.Navigator.AtStartOfBook)
                {
                    HighlightCurrentPosition();
                    TryPlay();
                    return;
                }

                if (!View.ApplicationView.DisplaySurface.ElementExists(_state.Navigator.CurrentElementID)
                    || (currentHighlight == newHighlight ))
                {
                    PreviousSection();
                }
                else
                {
                    //move to the beginning of the highlightable element
                    currentHighlight = newHighlight;
                    while (currentHighlight == newHighlight && !_state.Navigator.AtStartOfBook)
                    {
                        currentHighlight = View.ApplicationView.DisplaySurface.GetHighlightableElementOfElement(_state.Navigator.CurrentElementID);
                        _state.Navigator.MovePrevious();
                        newHighlight = View.ApplicationView.DisplaySurface.GetHighlightableElementOfElement(_state.Navigator.CurrentElementID);
                    }
                    _state.Navigator.MoveNext();

                    //Removed: Should moving to previous section go to _beginning_ of that section (i.e. the header?)
                    SetCurrentSectionHeading();
                    HighlightCurrentPosition();
                    TryPlay();
                }
            }
        }

        private void SearchView_NavigateToPage(object sender, PageNumEventArgs e)
        {
            if (_state.Navigator != null)
            {
                bool success = _state.Navigator.SeekToPageNum(e.PageNum);

                if (success)
                {
                    SetCurrentSectionHeading();

                    // 1. Set the highlight on the Surface to the CurrentPosition
                    HighlightCurrentPosition();
                    TryPlay();
                } else
                {
                    View.SearchView.PageNavigationOutOfRange(e.PageNum, _state.GetNumberOfPages());
                }
            }
        }

        private void SearchView_SearchForSection(object sender, SearchEventArgs e)
        {
            List<SearchResult> searchResults = new List<SearchResult>();
            //TODO Should the presenter be using XElements?
            if (_state.Navigator != null)
            {
                List<XElement> navSearchResults = _state.Navigator.SearchForString(e.SearchString);
                foreach (XElement el in navSearchResults)
                {
                    XAttribute id = el.Attribute("id");
                    if (id != null)
                    {
                        if (View.ApplicationView.DisplaySurface.ElementExists(id.Value))
                        {
                            searchResults.Add(new SearchResult(e.SearchString, el.Value, el.Attribute(_SMIL_REF_ATTR_NAME).Value));
                        }
                    }
                }
            }

            //Set the view search results
            View.SearchView.SearchResults = searchResults;
        }

        private void SearchView_SearchSelected(object sender, ItemSelectedEventArgs e)
        {
            if (_state.Navigator != null)
            {
                _state.Navigator.SeekToSmilReference(e.ID);
                SetCurrentSectionHeading();

                // 1. Set the highlight on the Surface to the CurrentPosition
                HighlightCurrentPosition();
                TryPlay();
            }
        }

        private void SetCurrentSectionHeading()
        {
            Heading currentHeading = _state.Navigator.GetCurrentSectionHeading();
            if (currentHeading != null && View.CurrentHeadingID != currentHeading.ID) View.CurrentHeadingID = currentHeading.ID;
        }

        private void UpLevel()
        {
            if (_state.Navigator != null)
            {
                _state.Navigator.UpLevel();
                SetCurrentSectionHeading();

                // 1. Set the highlight on the Surface to the CurrentPosition
                HighlightCurrentPosition();
                TryPlay();
            }
        }

        /// <summary>
        /// Handles event raised when an audio clip has completed and we can move to the next
        /// element.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_AudioCompleted(object sender, EventArgs e)
        {
            MoveNext();
        }

        private void View_DownLevel(object sender, EventArgs e)
        {
            DownLevel();
        }

        private void view_NextPage(object sender, EventArgs e)
        {
            NextPage();
        }

        private void View_NextSection(object sender, EventArgs e)
        {
            NextSection();
        }


        /// <summary>
        /// Repond to the Play event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_TogglePlayPause(object sender, EventArgs e)
        {
			if(_state.IsPlaying)
			{
				//Can only pause when playing - not when disabled, already paused or finished.
				_state.CurrentMode = PlayerMode.Paused;

				View.PauseAudioPlayback();
				View.SetPlayButtonState(_state.CanPlay, _state.IsPlaying);
			}
			else
			{
				// Start playing the book
				HighlightCurrentPosition();
				TryPlay();
			}
        }

        private void View_PreviousPage(object sender, EventArgs e)
        {
            PreviousPage();
        }

        private void View_PreviousSection(object sender, EventArgs e)
        {
            PreviousSection();
        }

        private void View_SectionChanged(object sender, EventArgs e)
        {
            // Not sure we have to do anything here, however, there will be other presenters that
            // will be listening to this event.
        }

        private void View_SelfVoicingSpeakText(object sender, SpeechTextEventArgs e)
        {
            PlayTextSpeech(e);
        }

        private void View_SetBookmark(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void View_SpeakableElementSelected(object sender, ElementSelectedEventArgs e)
        {
            PlaySpeakableElement(e);
        }

        void View_ToggleSelfVoicing(object sender, EventArgs e)
        {
            if (View.SelfVoicingEnabled)
            {
                View.SelfVoicingEnabled = false;
            }
            else
            {
                View.SelfVoicingEnabled = true;
            }
        }

        private void View_UpLevel(object sender, EventArgs e)
        {
            UpLevel();
        }

        private void ChangeVolume(double newVolume)
        {
            // Set the voice factory volume and the volume of the current voice
            VoiceFactory.Volume = newVolume;
            if (View.CurrentVoice != null)
            {
                View.CurrentVoice.Volume = newVolume;
            }

            if (ApplicationVoice.MainInstance != null)
            {
                ApplicationVoice.MainInstance.Volume = newVolume;
            }
        }

        private void View_ToggleMuting(object sender, EventArgs e)
        {
            if (View.AudioMuted)
            {
                View.AudioMuted = false;
                ChangeVolume(View.Volume);

                int percentage = (int)(100 * View.Volume);
                PlayTextSpeech(new SpeechTextEventArgs(String.Format("Volume is at {0} percent.", percentage)));
            } else
            {
                View.AudioMuted = true;
                ChangeVolume(0);
            }
        }

        /// <summary>
        /// Handles event raised when volume setting has been changed so we can set the volumes for
        /// the voices (i.e. MP3 and SAPI).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_VolumeChanged(object sender, VolumeChangedEventArgs e)
        {
            if (!View.AudioMuted)
            {
                ChangeVolume(e.Volume);
            }
        }
		// Internal Methods (4) 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        internal void PlaySpeakableElement(ElementSelectedEventArgs args)
        {
            // Tell the View to speak the element - this method could contain business logic
            // about whether self voicing is enabled.
            View.SpeakElement(args.Element, _state.IsPlaying);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        internal void PlaySpeakableElementHelpText(ElementSelectedEventArgs args)
        {
            // Tell the View to speak the element - this method could contain business logic
            // about whether self voicing is enabled.
            View.SpeakElementHelpText(args.Element, _state.IsPlaying);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        internal void PlayTextSpeech(SpeechTextEventArgs args)
        {
            View.SpeakTextSpeech(args.Speech, _state.IsPlaying);
        }

        /// <summary>
        /// Construct the phrase for the current element, set the button state and try to play the phrase
        /// TODO: could probably be split up as this does multiple things
        /// </summary>
        internal void TryPlay()
        {
            _timer.Stop();
            //Abort playing if at the end of the book.
            if (_state.Navigator != null && _state.Navigator.AtEndOfBook)
            {
                FinishBook();
                return;
            }
            else
            {
				//Need to pause until next phrase is ready for speaking.
				View.PauseAudioPlayback();
            }

            PlayerMode previousState = _state.CurrentMode;

            //If the player had previously finished, change the state to Paused since we're no longer at the end
            // of the book.
            if (previousState == PlayerMode.Finished)
                _state.CurrentMode = PlayerMode.Paused;

            if (_state.CanPlay && _state.Navigator != null)
            {
                _state.CurrentMode = PlayerMode.Playing;

                _state.GetCurrentPhraseAsync(previousState);
            }
        }

        public void SpeedChanged(int speed)
        {
            _timer.Interval = new TimeSpan(0, 0, speed);            
        }

		#endregion Methods 
    }

 
}
