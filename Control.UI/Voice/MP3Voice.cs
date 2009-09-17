using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Buttercup.Control.Model.Entities;


namespace Control.UI.Voice
{
	/// <summary>
	/// Implements the voice used to play embedded audio. This relies on the VoiceFactory to enforce that
	/// phrases passed to this voice have an Audio component.
	/// </summary>
	public class MP3Voice : VoiceBase
	{
		#region Fields (5) 

		/// <summary>
		/// The current phrase being played
		/// </summary>
		private Phrase _currentPhrase;

		/// <summary>
		/// The MediaElement player used to play the phrase
		/// </summary>
		private readonly MediaElement _player;

		private bool _playOnMediaLoad;
		private bool _resetPlayerPosition;

		/// <summary>
		/// Stores the volume for this voice
		/// </summary>
		private double _volume;

		#endregion Fields 



		#region Constructors (1) 

		/// <summary>
		/// The main constructor for an MP3Voice. Attaches to events on a given MediaElement player
		/// to know when audio is playing and when audio has finished playing.
		/// </summary>
		/// <param name="player"></param>
		public MP3Voice(MediaElement player)
		{
			_player = player;
			_player.MediaEnded += Player_MediaEnded;
			_player.MediaOpened += Player_MediaOpened;
			_player.MarkerReached += Player_MarkerReached;
			_resetPlayerPosition = true;
		}

		#endregion Constructors 



		#region Properties (2) 

		/// <summary>
		/// Set the speaking rate for the voice, an integer between -10 and 10.
		/// </summary>
		/// <param></param>
		public override int Rate { get; set; }

		/// <summary>
		/// Set the volume for the voice.
		/// </summary>
		public override double Volume
		{
			get { return _volume; }
			set
			{
				_volume = value;
				_player.Volume = _volume * 0.75; //mp3 player volume is between 0 and 1
			}
		}

		#endregion Properties 



		#region Methods (11) 

		// Public Methods (7) 

		/// <summary>
		/// Interrupts the existing playback with the given phrase. If the voice is not currently playing, 
		/// the given phrase is simply spoken.
		/// </summary>
		/// <param name="immediatePhrase">The immediate phrase to speak.</param>
		/// <remarks>If another voice is speaking, it is the presenter's responsibility to pause and resume
		/// playback as needed.</remarks>
		public override void InterruptPlayback(Phrase immediatePhrase)
		{
			//Do nothing.
			OnFinishedImmediatePhrase();
		}



		/// <summary>
		/// Provides a mechanism for stopping the interrupted playback i.e. for halting self-voicing manually
		/// </summary>
		public override void InterruptStop()
		{
			//do nothing
		}



		/// <summary>
		/// Load the given phrase without playing.
		/// </summary>
		/// <param name="currentPhrase">The current phrase.</param>
		public override void LoadPhrase(Phrase currentPhrase)
		{
			_resetPlayerPosition = true;
			_currentPhrase = currentPhrase;

			Audio currentAudio = _currentPhrase.Audio;
			if(currentAudio.SourceStream != null)
			{
                //reset source stream to beginning
			    currentAudio.SourceStream.Seek(0, SeekOrigin.Begin);
				_player.SetSource(currentAudio.SourceStream);
			}
			else
			{
				_player.Source = currentAudio.SourceUri;
			}
			_playOnMediaLoad = false;
		}



		/// <summary>
		/// Pauses playback of the current phrase.
		/// </summary>
		public override void Pause()
		{
			_player.Pause();
		}



		/// <summary>
		/// Resumes playing the current phrase from where it was previously paused.
		/// </summary>
		public override void ResumePlaying()
		{
			_resetPlayerPosition = false;

			_player.Play();
		}



		/// <summary>
		/// Starts playing the given phrase.
		/// </summary>
		/// <param name="currentPhrase">The current phrase.</param>
		public override void StartPlaying(Phrase currentPhrase)
		{
			LoadPhrase(currentPhrase);
			_playOnMediaLoad = true; //will start playing when the media is loaded
			_player.Play();
		}



		/// <summary>
		/// Stops playback of the current phrase.
		/// </summary>
		public override void Stop()
		{
			_player.Stop();
		}



		// Private Methods (3) 

		private void Player_MarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
		{
			// The current phrase has completed.
			_player.Pause();

			// Let subscribers know that the current phrase has finished playing.
			if(FinishedPlaying != null)
			{
				FinishedPlaying(this, new EventArgs());
			}
		}



		/// <summary>
		/// Handles the MediaEnded event of the _player control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void Player_MediaEnded(object sender, RoutedEventArgs e)
		{
			// Let subscribers know that the current phrase has finished playing.
			if(FinishedPlaying != null)
			{
				FinishedPlaying(this, new EventArgs());
			}
		}



		/// <summary>
		/// Handles the MediaOpened event of the player, raised when a new source has been set and Play()
		/// has been called.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e"></param>
		private void Player_MediaOpened(object sender, RoutedEventArgs e)
		{
			if(_resetPlayerPosition)
			{
				//Can only set positions and markers in this event for playback to perform properly.
				TimelineMarker marker = new TimelineMarker();
				marker.Time = _currentPhrase.Audio.ClipEnd;
				marker.Text = "StopMarker";
				marker.Type = "StopMarkerType";
				_player.Markers.Add(marker);
				_player.Position = _currentPhrase.Audio.ClipStart;
				if(_playOnMediaLoad)
				{
					_player.Play();
				}
			}
		}



		// Internal Methods (1) 

		internal void ResetVoice()
		{
			_resetPlayerPosition = true;
		}

		#endregion Methods 
	}
}