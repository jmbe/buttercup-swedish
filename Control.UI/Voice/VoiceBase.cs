using System;
using Buttercup.Control.Model.Entities;


namespace Control.UI.Voice
{
	public abstract class VoiceBase
	{
		#region Fields (1) 

		/// <summary>
		/// Called when the current phrase has finished playing.
		/// </summary>
		/// <remarks>If program control has interrupted playback, this delegate will not be called.</remarks>
		public EventHandler FinishedPlaying;

		#endregion Fields 



		#region Properties (2) 

		/// <summary>
		/// Set the speaking rate for the voice, an integer between -10 and 10.
		/// </summary>
		/// <param></param>
		public virtual int Rate { get; set; }

		/// <summary>
		/// Set the volume for the voice, a double between 0 and 1.
		/// </summary>
		/// <param></param>
		public virtual double Volume { get; set; }

		#endregion Properties 



		#region Delegates and Events (1) 

		// Events (1) 

		/// <summary>
		/// Raised when an immediate phrase (initiated via InterruptPlayback) has finished playing.
		/// </summary>
		public event EventHandler FinishedImmediatePhrase;

		#endregion Delegates and Events 



		#region Methods (8) 

		// Public Methods (7) 

		/// <summary>
		/// Interrupts the existing playback with the given phrase. If the voice is not currently playing, 
		/// the given phrase is simply spoken.
		/// </summary>
		/// <param name="immediatePhrase">The immediate phrase to speak.</param>
		/// <remarks>If another voice is speaking, it is the presenter's responsibility to pause and resume
		/// playback as needed.</remarks>
		public abstract void InterruptPlayback(Phrase immediatePhrase);



		/// <summary>
		/// Provides a mechanism for stopping the interrupted playback i.e. for halting self-voicing manually
		/// </summary>
		public abstract void InterruptStop();



		/// <summary>
		/// Load the given phrase without playing.
		/// </summary>
		/// <param name="currentPhrase">The current phrase.</param>
		public abstract void LoadPhrase(Phrase currentPhrase);



		/// <summary>
		/// Pauses playback of the current phrase.
		/// </summary>
		public abstract void Pause();



		/// <summary>
		/// Resumes playing the current phrase from where it was previously paused.
		/// </summary>
		public abstract void ResumePlaying();



		/// <summary>
		/// Starts playing the given phrase.
		/// </summary>
		/// <param name="currentPhrase">The current phrase.</param>
		public abstract void StartPlaying(Phrase currentPhrase);



		/// <summary>
		/// Stops playback of the current phrase.
		/// </summary>
		public abstract void Stop();



		// Protected Methods (1) 

		/// <summary>
		/// Need to define this as derived classes cannot invoke events directly.
		/// </summary>
		protected void OnFinishedImmediatePhrase()
		{
			if(FinishedImmediatePhrase != null)
			{
				FinishedImmediatePhrase(this, new EventArgs());
			}
		}

		#endregion Methods 
	}
}