using System.Windows.Controls;
using Buttercup.Control.Model.Entities;
using Control.UI.Voice;

namespace Control.UI
{
    public class VoiceFactory
    {
        #region Fields (3)

        /// <summary>
        /// The voice used for playing MP3 audio
        /// </summary>
        private static MP3Voice _mp3Voice;
        /// <summary>
        /// The voice used for playing SAPI audio, or no audio if in FireFox or SAPI not supported
        /// </summary>
        private static VoiceBase _sapiVoice;
        /// <summary>
        /// Stores the volume for the voices produced in this factory
        /// </summary>

        #endregion Fields

        #region Properties (1)

        /// <summary>
        /// Stores the volume for the voices produced in this factory
        /// </summary>
        public static double Volume { get; set; }

        public static int Rate { get; set; }

        #endregion Properties

        #region Methods (1)

        // Public Methods (1) 

        /// <summary>
        /// Determines and returns the voice to use for the given phrase. Also takes in a MediaElement
        /// as we cannot easily programmatically instantiate a MediaElement (since in order to function
        /// a MediaElement must be inserted into the UI Tree).
        /// </summary>
        /// <param name="currentPhrase">The current phrase to speak.</param>
        /// <param name="player">The MediaElement player used to play MP3 audio.</param>
        /// <returns>The IVoice to use to speak the given phrase.</returns>
        public static VoiceBase GetVoice(Phrase currentPhrase, MediaElement player)
        {
            if (currentPhrase == null) return null;

            if (SapiVoice.IsEnabled() && !currentPhrase.Silent)
                _sapiVoice = new SapiVoice(false);
            else
                _sapiVoice = new SilentVoice(); //Text-to-speech unavailable - use a silent voice instead.

            //If there is no included audio use the SAPi/Silent voice, otherwise the MP3
            if (currentPhrase.Audio == null)
            {
                _sapiVoice.Volume = Volume;
                return _sapiVoice;
            }
            if (_mp3Voice == null)
	            _mp3Voice = new MP3Voice(player) { Volume = Volume };
            else
            	_mp3Voice.ResetVoice();
            return _mp3Voice;
        }

        #endregion Methods
    }
}