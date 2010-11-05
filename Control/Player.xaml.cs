using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using Buttercup.Control.Model;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.Resources;
using Buttercup.Control.UI;
using Control.MVP;
using Control.MVP.Presenters;
using Control.MVP.Views;
using Control.UI;
using Control.UI.Utility;
using Control.UI.Voice;


namespace Buttercup.Control
{
	public partial class Player : UserControl, IPlayerView
	{
		#region Fields (13) 

		/// <summary>
		/// Used to track whether the application voice is speaking
		/// </summary>
		private bool _appVoiceSpeaking;

		private string _currentHeadingID;
		private Binding _foregroundBinding;
		private KeyCommandHandler _keyCommandHandler;

		private Path _pauseIcon;
		private Path _playIcon;

		private PlayerPresenter _presenter;
		private int _rate;
		private bool _selfVoicingEnabled;
		private bool _shouldPauseVoice;
		private bool _isVoicePlaying;
		private double _volume;

		#endregion Fields 



		#region Constructors (1) 

		public Player()
		{
			InitializeComponent();

			//TODO : Save/Load this value from preferences?
			VolumeSlider.Value = 8;

			//TODO : View shouldn't have direct access to the voice factory? move to player presenter along with other
			// voice related code.
			Volume = 0.5;
			VoiceFactory.Volume = 0.5;
			ApplicationVoice.MainInstance.Volume = 0.5;

			_foregroundBinding = new Binding("DefaultForeground");

			//Construct play and pause icons
			//Because setting Content to {StaticResource <whatever>} does not work in the XAML
			//http://silverlight.net/forums/t/13160.aspx
			_playIcon = playIcon();
			_pauseIcon = pauseIcon();

			playPauseButton.Content = _playIcon;
			AutomationProperties.SetHelpText(playPauseButton, Strings.AutomationHelp_Play);
			AutomationProperties.SetName(playPauseButton, Strings.AutomationName_Play);
			toolTipPlayPauseButton.Content = Strings.ToolTip_Play + " (Spacebar)";
			//ToolTipService.SetToolTip(playPauseButton, Strings.ToolTip_Play);

			VolumeSlider.ValueChanged += VolumeSlider_VolumeChanged;
			VolumeSlider.ThumbDragCompleted += VolumeSlider_ThumbDragCompleted;
			ApplicationVoice.MainInstance.PhraseFinished += MainInstance_PhraseFinished;

            PlayerSpeed.TextChanged += PlayerSpeed_SpeedChanged;
           
			_appVoiceSpeaking = false; //initially false
			_selfVoicingEnabled = true;
			_isVoicePlaying = false;
			AudioMuted = false;
		}

		#endregion Constructors 



		#region Properties (9) 

		public IApplicationView ApplicationView { get; set; }

		public string CurrentHeadingID
		{
			get { return _currentHeadingID; }
			set
			{
				bool changed = (_currentHeadingID != value);

				_currentHeadingID = value;

				// If the heading has changed (by the presenter for example) then we need to 
				// let listeners know the section has changed.
				if(changed && SectionChanged != null)
				{
					SectionChanged(this, new EventArgs());
				}
			}
		}

		public KeyCommandHandler KeyCommandHandler
		{
			get { return _keyCommandHandler; }
			set
			{
				_keyCommandHandler = value;
				if(_keyCommandHandler != null)
				{
					_keyCommandHandler.KeyDown += KeyCommandHandler_KeyIsPressed;
					_keyCommandHandler.KeyUp += KeyCommandHandler_KeyIsReleased;
				}
			}
		}

		public INavigationView NavigationView { get; set; }

		public int Rate
		{
			get { return _rate; }
			set { _rate = value; }
		}

		public ISearchView SearchView { get; set; }

		public bool AudioMuted { get; set; }

		public bool SelfVoicingEnabled
		{
			get { return _selfVoicingEnabled; }
			set
			{
				if(value)
				{
					_selfVoicingEnabled = value;
					SelfVoicingSpeakText(new object(), new SpeechTextEventArgs("Self-Voicing is now Enabled."));
				}
				else
				{
					SelfVoicingSpeakText(new object(), new SpeechTextEventArgs("Self-Voicing is now Disabled."));
					_selfVoicingEnabled = value;
				}
			}
		}

		public Surface Surface { get; set; }

		public VoiceBase CurrentVoice { get; set; }

		/// <summary>
		/// This is the current volume for the speaking voices, set by whatever control in the view.
		/// </summary>
		public double Volume
		{
			get { return _volume; }
			set { _volume = value; }
		}

		#endregion Properties 



		#region Delegates and Events (17) 

		// Events (17) 

		public event EventHandler AudioCompleted;

		public event EventHandler DownLevel;

		public event EventHandler NextPage;

		public event EventHandler NextSection;

		public event EventHandler Pause;

		public event EventHandler TogglePlayPause;

		public event EventHandler PreviousPage;

		public event EventHandler PreviousSection;

		public event EventHandler SectionChanged;

		public event EventHandler<SpeechTextEventArgs> SelfVoicingSpeakText;

		public event EventHandler SetBookmark;

		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelected;

		/// <summary>
		/// Raised when a UI element's help text is required to be spoken.
		/// </summary>
		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelectedHelpText;

		/// <summary>
		/// Handles the event when tooltips are to be spoken.
		/// </summary>
		public event EventHandler<ResolveEventArgs> SpeakToolTip;

		public event EventHandler ToggleMuting;

		public event EventHandler ToggleSelfVoicing;

		public event EventHandler UpLevel;

		public event EventHandler<VolumeChangedEventArgs> VolumeChanged;

		#endregion Delegates and Events 



		#region Methods (35) 

		// Public Methods (13) 

		/// <summary>
		/// Pauses the playback of the current book.
		/// </summary>
		public void PauseAudioPlayback()
		{
			if(CurrentVoice != null)
			{
				CurrentVoice.Pause();
			}
		}



		/// <summary>
		/// Constructs the PathGeometry of the Pause Icon
		/// </summary>
		/// <returns></returns>
		private Path pauseIcon()
		{
			Path path =
				XamlReader.Load(
					"<Path xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Height=\"14\" Width=\"11\" Stretch=\"Fill\" Data=\"F1 M 101.848,150.813L 106.093,150.813L 106.093,136.147L 101.848,136.147M 94.9013,150.813L 99.1467,150.813L 99.1467,136.147L 94.9013,136.147L 94.9013,150.813 Z \" />")
					as Path;
			path.SetBinding(Shape.FillProperty, _foregroundBinding);

			return path;
		}



		/// <summary>
		/// Constructs the PathGeometry of the Play Icon
		/// </summary>
		/// <returns></returns>
		private Path playIcon()
		{
			Path path =
				XamlReader.Load(
					"<Path xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Height=\"14.524\" Width=\"12.576\" Stretch=\"Fill\" Data=\"F1 M 286.129,296.767L 298.705,304.024L 286.129,311.291L 286.129,296.767 Z \" />")
					as Path;
			path.SetBinding(Shape.FillProperty, _foregroundBinding);

			return path;
		}



		/// <summary>
		/// Resumes the audio playback of the current phrase.
		/// </summary>
		public void ResumeSpeakingPhrase()
		{
			if(CurrentVoice != null)
			{
				CurrentVoice.ResumePlaying();
			}
		}



		/// <summary>
		/// Sets the state of the navigation buttons.
		/// </summary>
		/// <param name="isEnabled">Determines whether the navigation buttons should be enabled.</param>
		public void SetNavButtonState(bool isEnabled)
		{
			Dispatcher.BeginInvoke(delegate()
			                       {
			                       	PrevSectionButton.IsEnabled = isEnabled;
			                       	PrevSentenceButton.IsEnabled = isEnabled;
			                       	NextSentenceButton.IsEnabled = isEnabled;
			                       	NextSectionButton.IsEnabled = isEnabled;
			                       });
		}



		/// <summary>
		/// Sets the state of the play button.
		/// </summary>
		/// <param name="isEnabled">Determines whether the button should be enabled.</param>
		/// <param name="isPlaying">Determines whether the button should indicate that the player
		/// is currently playing.</param>
		public void SetPlayButtonState(bool isEnabled, bool isPlaying)
		{
			// This code is sometimes run from a non-UI thread - ensure it still executes on the 
			// UI thread.
			Dispatcher.BeginInvoke(delegate()
			                       {
			                       	if(!isPlaying)
			                       	{
			                       		playPauseButton.Content = _playIcon;
			                       		AutomationProperties.SetHelpText(playPauseButton, Strings.AutomationHelp_Play);
			                       		AutomationProperties.SetName(playPauseButton, Strings.AutomationName_Play);
			                       		toolTipPlayPauseButton.Content = Strings.ToolTip_Play + " (Spacebar)";
			                       	}
			                       	else
			                       	{
			                       		playPauseButton.Content = _pauseIcon;
			                       		AutomationProperties.SetHelpText(playPauseButton, Strings.AutomationHelp_Pause);
			                       		AutomationProperties.SetName(playPauseButton, Strings.AutomationName_Pause);
			                       		toolTipPlayPauseButton.Content = Strings.ToolTip_Pause + " (Spacebar)";
			                       	}

			                       	if(playPauseButton.IsMouseOver)
			                       	{
			                       		CommonEventHandlerUtility.HandleMouseOver(playPauseButton);
			                       	}
			                       		//else if(playPauseButton.IsFocused)
			                       		//    CommonEventHandlerUtility.HandleGetFocus(playPauseButton);
			                       	else
			                       	{
			                       		CommonEventHandlerUtility.HandleMouseOutLoseFocus(playPauseButton);
			                       	}

			                       	playPauseButton.IsEnabled = isEnabled;
			                       });
		}



		/// <summary>
		/// Sets the presenter reference for the view.
		/// </summary>
		/// <param name="presenter">The presenter to interact with.</param>
		public void SetPresenterReference(PlayerPresenter presenter)
		{
			_presenter = presenter;
		}



		/// <summary>
		/// Speaks the name of the given element.
		/// </summary>
		/// <param name="dependencyObject">The dependency object containing details of what to speak.</param>
		/// <param name="isBookPlaying">Indicates whether or not the current book is currently playing.</param>
		public void SpeakElement(DependencyObject dependencyObject, bool isBookPlaying)
		{
			if(_selfVoicingEnabled)
			{
				_shouldPauseVoice = isBookPlaying;
				//PlayerMode currentPlayerMode = _presenter.GetCurrentPlayerState();
				//System.Diagnostics.Debug.WriteLine(String.Format("Starting System Voice; 'shouldPauseVoice'={0}; PlayerState={1}", _shouldPauseVoice, currentPlayerMode.ToString()));
				if(_shouldPauseVoice)
				{
					//System.Diagnostics.Debug.WriteLine("Pausing Book Voice");
					PauseAudioPlayback();
				}
				if(SapiVoice.IsEnabled())
				{
					_appVoiceSpeaking = true;
				}
				ApplicationVoice.MainInstance.Speak(dependencyObject);
			}
		}



		/// <summary>
		/// Speaks the name of the given element.
		/// </summary>
		/// <param name="dependencyObject">The dependency object containing details of what to speak.</param>
		/// <param name="isBookPlaying">Indicates whether or not the current book is currently playing.</param>
		public void SpeakElementHelpText(DependencyObject dependencyObject, bool isBookPlaying)
		{
			if(_selfVoicingEnabled)
			{
				_shouldPauseVoice = isBookPlaying;
				//PlayerMode currentPlayerMode = _presenter.GetCurrentPlayerState();
				//System.Diagnostics.Debug.WriteLine(String.Format("Starting System Voice; 'shouldPauseVoice'={0}; PlayerState={1}", _shouldPauseVoice, currentPlayerMode.ToString()));
				if(_shouldPauseVoice)
				{
					//System.Diagnostics.Debug.WriteLine("Pausing Book Voice");
					PauseAudioPlayback();
				}
				if(SapiVoice.IsEnabled())
				{
					_appVoiceSpeaking = true;
				}
				ApplicationVoice.MainInstance.SpeakHelpText(dependencyObject);
			}
		}



		public void SpeakTextSpeech(string Speech, bool isBookPlaying)
		{
			if(_selfVoicingEnabled)
			{
				_shouldPauseVoice = isBookPlaying;
				//PlayerMode currentPlayerMode = _presenter.GetCurrentPlayerState();
				//System.Diagnostics.Debug.WriteLine(String.Format("Starting System Voice; 'shouldPauseVoice'={0}; PlayerState={1}", _shouldPauseVoice, currentPlayerMode.ToString()));
				if(_shouldPauseVoice)
				{
					//System.Diagnostics.Debug.WriteLine("Pausing Book Voice");
					PauseAudioPlayback();
				}
				if(SapiVoice.IsEnabled())
				{
					_appVoiceSpeaking = true;
				}
				ApplicationVoice.MainInstance.Speak(Speech);
			}
		}



		/// <summary>
		/// Starts the audio playback of the given phrase.
		/// </summary>
		/// <param name="currentPhrase">The phrase to speak.</param>
		public void StartSpeakingPhrase(Phrase currentPhrase)
		{
			if(CurrentVoice != null)
			{
				CurrentVoice.Stop();
			}

			CurrentVoice = VoiceFactory.GetVoice(currentPhrase, AudioPlayer);

			if(CurrentVoice != null)
			{
				//Set handler delegate and start playing
				CurrentVoice.FinishedPlaying = currentVoice_FinishedPlaying;

				//if the application voice is currently speaking, then we only want to load the new phrase
				if(!_appVoiceSpeaking)
				{
					CurrentVoice.StartPlaying(currentPhrase);
				}
				else
				{
					CurrentVoice.LoadPhrase(currentPhrase);
				}
			}
		}



		/// <summary>
		/// Stops the audio playback of the current phrase.
		/// </summary>
		public void StopAudioPlayback()
		{
			if(CurrentVoice != null)
			{
				CurrentVoice.Stop();
			}
		}



		public void StopSelfVoiceSpeaking()
		{
			ApplicationVoice.MainInstance.StopSpeaking();
		}



		// Private Methods (22) 

		/// <summary>
		/// Default handler raised when a button loses focus. Adjusts the foreground binding.
		/// </summary>
		private void Button_LostFocus(object sender, RoutedEventArgs e)
		{
			if(sender is Button && ((Button)sender).IsMouseOver)
			{
				CommonEventHandlerUtility.HandleMouseOver(sender);
			}
			else
			{
				CommonEventHandlerUtility.HandleMouseOutLoseFocus(sender);
			}
		}



		/// <summary>
		/// Default handler raised when the mouse enters a button's region. Adjusts the foreground binding.
		/// </summary>
		private void Button_MouseEnter(object sender, MouseEventArgs e)
		{
			CommonEventHandlerUtility.HandleMouseOver(sender);
		}



		/// <summary>
		/// Default handler raised when the mouse leaves a button's region. Adjusts the foreground binding.
		/// </summary>
		private void Button_MouseLeave(object sender, MouseEventArgs e)
		{
			CommonEventHandlerUtility.HandleMouseOutLoseFocus(sender);
		}



		private void ChangeRate(int newRate)
		{
			Rate = newRate;
			VoiceFactory.Rate = Rate;
			if(CurrentVoice != null)
			{
				CurrentVoice.Rate = Rate;
			}

			if(ApplicationVoice.MainInstance != null)
			{
				ApplicationVoice.MainInstance.Rate = Rate;
			}
		}



		/// <summary>
		/// Called in response to a number of event handlers invoked by the UI.
		/// </summary>
		private void ChangeVolume(double newVolume)
		{
			//Reminder: Volume is a double between 0 and 1

			//Convert volume slider values into the 0..1 range which is what the voices operate on.
			double adjustedVolume = newVolume / (VolumeSlider.Maximum - VolumeSlider.Minimum);
			Volume = adjustedVolume;

			if(VolumeChanged != null)
			{
				VolumeChanged(this, new VolumeChangedEventArgs(adjustedVolume));
			}
		}



		void currentVoice_FinishedPlaying(object sender, EventArgs e)
		{
			if(AudioCompleted != null)
			{
				AudioCompleted(this, new EventArgs());
				if(SectionChanged != null)
				{
					SectionChanged(this, new EventArgs());
				}
			}
		}



		/// <summary>
		/// Handle Player Key Commands
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void KeyCommandHandler_KeyIsPressed(object sender, KeyCommandEventArgs e)
		{
			Shortcut keysPressed = new Shortcut(e.keyEventArgs.PlatformKeyCode, e.keyEventArgs.Key, Keyboard.Modifiers);

			//Universal Player Shortcuts

			if(e.SubViewHandled == false)
			{
				if(keysPressed.Equals(Shortcuts.PlayPause))
				{
					// Play/Pause the book reader
					playPauseButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.PreviousHeader))
				{
					// go to previous header
					PrevSectionButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.NextHeader))
				{
					// go to next header
					NextSectionButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.PreviousSection))
				{
					// go to previous section
					PrevSentenceButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.NextSection))
				{
					// go to next section
					NextSentenceButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.PreviousPage))
				{
					// go to previous page
					if(PreviousPage != null)
					{
						PreviousPage(this, new EventArgs());
						if(SectionChanged != null)
						{
							SectionChanged(this, new EventArgs());
						}
					}
				}
				if(keysPressed.Equals(Shortcuts.NextPage))
				{
					// go to next page
					if(NextPage != null)
					{
						NextPage(this, new EventArgs());
						if(SectionChanged != null)
						{
							SectionChanged(this, new EventArgs());
						}
					}
				}
				if(keysPressed.Equals(Shortcuts.IncreaseVolume))
				{
					// increase volume
					if(VolumeSlider.Value < VolumeSlider.Maximum)
					{
						ChangeVolume(VolumeSlider.Value += 1);
					}
				}
				if(keysPressed.Equals(Shortcuts.DecreaseVolume))
				{
					// decrease volume
					if(VolumeSlider.Value > VolumeSlider.Minimum)
					{
						ChangeVolume(VolumeSlider.Value -= 1);
					}
				}
				if(keysPressed.Equals(Shortcuts.MuteUnmute))
				{
					// mute/unmute volume
					ToggleMute();
				}

				if(keysPressed.Equals(Shortcuts.ToggleSelfVoicing))
				{
					if(ToggleSelfVoicing != null)
					{
						ToggleSelfVoicing(new object(), new EventArgs());
					}
				}
				if(keysPressed.Equals(Shortcuts.StopSelfVoice))
				{
					//TODO: Shift this _all_ to playerpresenter and playerstate
					StopSelfVoiceSpeaking();
				}

				//These shortcuts are not for the demo, increase and decrease SAPI voice rate
				//TODO: Comment these out!!
				//if (keysPressed.Equals(Shortcuts.DecreaseRate))
				//{
				//    if (Rate > -10)
				//    {
				//        ChangeRate(Rate -= 1);
				//        if (Rate == -10)
				//            SelfVoicingSpeakText(new object(), new SpeechTextEventArgs("Slowest."));
				//        else
				//            SelfVoicingSpeakText(new object(), new SpeechTextEventArgs("Slower."));

				//    }
				//}
				//if (keysPressed.Equals(Shortcuts.IncreaseRate))
				//{
				//    if (Rate < 10)
				//    {
				//        ChangeRate(Rate += 1);
				//        if (Rate == 10)
				//            SelfVoicingSpeakText(new object(), new SpeechTextEventArgs("Fastest."));
				//        else
				//            SelfVoicingSpeakText(new object(), new SpeechTextEventArgs("Faster."));
				//    }
				//}
			}
		}



		void KeyCommandHandler_KeyIsReleased(object sender, KeyCommandEventArgs e)
		{
			Shortcut keysPressed = new Shortcut(e.keyEventArgs.PlatformKeyCode, e.keyEventArgs.Key, Keyboard.Modifiers);

			if(e.SubViewHandled == false)
			{
				if(keysPressed.Equals(Shortcuts.IncreaseVolume))
				{
					SpeakVolume();
				}
				if(keysPressed.Equals(Shortcuts.DecreaseVolume))
				{
					SpeakVolume();
				}
			}
		}



		/// <summary>
		/// Raised when the Application voice finishes speaking its item, and the main voice needs to resume
		/// (unless paused in the meantime).
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e"></param>
		void MainInstance_PhraseFinished(object sender, EventArgs e)
		{
			PlayerMode currentPlayerMode = _presenter.GetCurrentPlayerState();

			if(SapiVoice.IsEnabled())
			{
				_appVoiceSpeaking = false;
			}

			//System.Diagnostics.Debug.WriteLine(String.Format("System Voice finished; 'shouldPauseVoice'={0}; PlayerState={1}", _shouldPauseVoice, currentPlayerMode.ToString()));
			if(currentPlayerMode == PlayerMode.Playing) //_shouldPauseVoice && 
			{
				//System.Diagnostics.Debug.WriteLine("Resuming playback of Book Voice");
				if(CurrentVoice != null)
				{
					//if there is a current voice to resume, resume playing
					CurrentVoice.ResumePlaying();
				}
			}
		}



		/// <summary>
		/// Event handler raised when the Next Section (Down Level) button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NextSectionButton_Click(object sender, RoutedEventArgs e)
		{
			if(DownLevel != null)
			{
				DownLevel(this, new EventArgs());
				if(SectionChanged != null)
				{
					SectionChanged(this, new EventArgs());
				}
			}
		}



		/// <summary>
		/// Event handler raised when the Next Sentence button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NextSentenceButton_Click(object sender, RoutedEventArgs e)
		{
			if(NextSection != null)
			{
				NextSection(this, new EventArgs());
				if(SectionChanged != null)
				{
					SectionChanged(this, new EventArgs());
				}
			}
		}



		private void playPauseButton_Click(object sender, RoutedEventArgs e)
		{
			// Raise the Play/Pause event for the Presenter to respond to.
			if(TogglePlayPause != null)
			{
				// Let others (presenter) know that the Play has been requested.
				TogglePlayPause(this, new EventArgs());
			}
		}



		/// <summary>
		/// Event handler raised when the Previous Section (Up Level) button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PrevSectionButton_Click(object sender, RoutedEventArgs e)
		{
			if(UpLevel != null)
			{
				UpLevel(this, new EventArgs());
				if(SectionChanged != null)
				{
					SectionChanged(this, new EventArgs());
				}
			}
		}



		/// <summary>
		/// Event handler raised when the Previous Sentence button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PrevSentenceButton_Click(object sender, RoutedEventArgs e)
		{
			if(PreviousSection != null)
			{
				PreviousSection(this, new EventArgs());
				if(SectionChanged != null)
				{
					SectionChanged(this, new EventArgs());
				}
			}
		}



		/// <summary>
		/// Handles the GotFocus event of a button. This is used to initiate speaking the voice command.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e"></param>
		/// <remarks>Since this event handler is used by multiple button sources, the Tag property
		/// is used to retrieve the command name.</remarks>
		private void SpeakControlOnFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.HandleGetFocus(sender);

			if(SpeakableElementSelected != null)
			{
				SpeakableElementSelected(this, new ElementSelectedEventArgs(sender as DependencyObject));
			}
		}



		private void SpeakVolume()
		{
			int percentage = (int)(100 * ((VolumeSlider.Value - VolumeSlider.Minimum) / (VolumeSlider.Maximum - VolumeSlider.Minimum)));

			if(SelfVoicingSpeakText != null)
			{
				SelfVoicingSpeakText(this, new SpeechTextEventArgs(String.Format("Volume is at {0} percent.", percentage)));
			}
		}



		/// <summary>
		/// Basic mute and un-mute function
		/// </summary>
		private void ToggleMute()
		{
			//Toggle the VolumeSlider Icon
			VolumeSlider.ToggleMute();
			if(ToggleMuting != null)
			{
				ToggleMuting(this, new EventArgs());
			}
		}



		/// <summary>
		/// Raised when the volume slider receives focus, to provide self-voicing.
		/// </summary>
		private void VolumeSlider_GotFocus(object sender, RoutedEventArgs e)
		{
			int percentage = (int)(100 * ((VolumeSlider.Value - VolumeSlider.Minimum) / (VolumeSlider.Maximum - VolumeSlider.Minimum)));

			if(SelfVoicingSpeakText != null)
			{
				SelfVoicingSpeakText(this, new SpeechTextEventArgs(String.Format(Speech.VolumeSliderMessage, percentage)));
			}
		}



		void VolumeSlider_ThumbDragCompleted(object sender, EventArgs e)
		{
			SpeakVolume();
		}



		/// <summary>
		/// Event handler raised when the volume slider has changed value
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void VolumeSlider_VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			ChangeVolume(VolumeSlider.Value);
		}

        private void PlayerSpeed_SpeedChanged(object sender, EventArgs e)
        {
            _presenter.SpeedChanged(System.Convert.ToInt32(PlayerSpeed.Text, 10));
        }

        #endregion Methods 
	}
}