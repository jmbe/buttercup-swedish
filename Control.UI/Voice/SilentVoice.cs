using System;
using System.Windows.Threading;
using Buttercup.Control.Model.Entities;

namespace Control.UI.Voice
{
    /// <summary>
    /// Implements a 'silent' voice, which pauses the highlighting when MP3 or SAPI voices are
    /// unable to speak the current phrase.
    /// </summary>
    public class SilentVoice : VoiceBase
    {
        #region Fields (7)

        private const int _MILLISECONDS_PER_CHARACTER = 120;
        private const int _MIN_MILLISECONDS_DURATION = 1000;
        private const int _MIN_MILLISECONDS_REMAINING_DURATION = 100;
        private TimeSpan _startingInterval;
        private DateTime _startTime;
        private readonly DispatcherTimer _timer;
        private TimeSpan _totalElapsedtime;

        #endregion Fields


        #region Constructors (1)

        /// <summary>
        /// Constructor for the silent voice. Silent voice uses a timer to simulate the
        /// time it takes to speak a phrase.
        /// </summary>
        public SilentVoice()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += finishTimer;
        }

        #endregion Constructors


        #region Methods (6)

        // Public Methods (5) 

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
        /// Pauses playback of the current phrase.
        /// </summary>
		public override void Pause()
        {
            TimeSpan elapsedTime = DateTime.Now.Subtract(_startTime);
            _totalElapsedtime.Add(elapsedTime);

            _timer.Stop();
        }

        /// <summary>
        /// Resumes playing the current phrase from where it was previously paused.
        /// </summary>
		public override void ResumePlaying()
        {
            _startTime = DateTime.Now;

            //Ensure remaining interval is at least the specified minimum (in milliseconds)
            int remainingInterval = (int)_startingInterval.Subtract(_totalElapsedtime).TotalMilliseconds;
            remainingInterval = Math.Max(remainingInterval, _MIN_MILLISECONDS_REMAINING_DURATION);

            _timer.Interval = new TimeSpan(0, 0, 0, 0, remainingInterval);
            _timer.Start(); //Restart for remainder.
        }

        /// <summary>
        /// Load the given phrase without playing.
        /// </summary>
        /// <param name="currentPhrase">The current phrase.</param>
		public override void LoadPhrase(Phrase currentPhrase)
        {
            //do nothing different than start playing
            StartPlaying(currentPhrase);
        }

        /// <summary>
        /// Starts playing the given phrase.
        /// </summary>
        /// <param name="currentPhrase">The current phrase.</param>
		public override void StartPlaying(Phrase currentPhrase)
        {
            _startTime = DateTime.Now;
            _totalElapsedtime = new TimeSpan(0L); //Reset elapsed time.

            //Calculate appropriate interval to 'play' based on length of text.
            int totalInterval = 0;
            string text = currentPhrase.Text;
            if (!String.IsNullOrEmpty(text))
                totalInterval = text.Length * _MILLISECONDS_PER_CHARACTER;
            totalInterval = Math.Max(totalInterval, _MIN_MILLISECONDS_DURATION);
            _startingInterval = new TimeSpan(0, 0, 0, 0, totalInterval);

            _timer.Interval = _startingInterval;
            _timer.Start();
        }

        /// <summary>
        /// Stops playback of the current phrase.
        /// </summary>
		public override void Stop()
        {
            _timer.Stop();
            _totalElapsedtime = new TimeSpan(0L);
        }
        // Private Methods (1) 

        private void finishTimer(object source, EventArgs e)
        {
            _timer.Stop();
            _totalElapsedtime = new TimeSpan(0L); //Reset elapsed time.

            if (FinishedPlaying != null)
            {
                FinishedPlaying(this, new EventArgs());
            }
        }

        #endregion Methods
    }
}