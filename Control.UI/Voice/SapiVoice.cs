using System;
using System.Text.RegularExpressions;
using System.Windows.Browser;
using Buttercup.Control.Common;
using Buttercup.Control.Model.Entities;


namespace Control.UI.Voice
{
	public class SapiVoice : VoiceBase
	{
		#region Fields (1)

		/// <summary>
		/// Stores the volume for this voice
		/// </summary>
		private double _volume;

		private int _rate;

		#endregion Fields



		#region Constructors (2)

		/// <summary>
		/// Constructor for the SapiVoice. Takes a boolean to determine which SAPI voice
		/// to use, as we have one voice for reading the book and a second voice for reading
		/// application speech.
		/// </summary>
		/// <param name="isApplicationVoice"></param>
		public SapiVoice(bool isApplicationVoice)
		{
			string objectName = isApplicationVoice? "ButtercupAppVoice" : "ButtercupVoice";
			HtmlPage.RegisterScriptableObject(objectName, this);
		}

		#endregion Constructors



		#region Properties (1)

		/// <summary>
		/// Set the volume for the voice.
		/// </summary>
		public override double Volume
		{
			get { return _volume; }
			set
			{
				_volume = value;
				//actual sapi volume is between 0 and 100, but will re-adjust that range
				//in the javascript function
				try
				{
					HtmlPage.Window.Invoke("SetVolume", _volume);
				}
				catch(Exception e)
				{
					Logger.Log(e);
				}
			}
		}

		/// <summary>
		/// Set the speaking rate for the voice, an integer between -10 and 10.
		/// </summary>
		/// <param></param>
		public override int Rate
		{
			get { return _rate; }
			set
			{
				_rate = value;
				try
				{
					HtmlPage.Window.Invoke("SetRate", _rate);
				}
				catch(Exception e)
				{
				}
			}
		}

		#endregion Properties



		#region Methods (8)

		// Public Methods (8) 

		/// <summary>
		/// Interrupts the existing playback with the given phrase. If the voice is not currently playing, 
		/// the given phrase is simply spoken.
		/// </summary>
		/// <param name="immediatePhrase">The immediate phrase to speak.</param>
		/// <remarks>If another voice is speaking, it is the presenter's responsibility to pause and resume
		/// playback as needed.</remarks>
		public override void InterruptPlayback(Phrase immediatePhrase)
		{
			try
			{
				HtmlPage.Window.Invoke("PlayHighPrioritySpeech", immediatePhrase.Text);
			}
			catch(Exception)
			{
			}
		}



		/// <summary>
		/// Provides a mechanism for stopping the interrupted playback i.e. for halting self-voicing manually
		/// </summary>
		public override void InterruptStop()
		{
			try
			{
				HtmlPage.Window.Invoke("StopHighPrioritySpeech");
			}
			catch(Exception)
			{
			}
		}



		/// <summary>
		/// Determines whether the SAPI capability is enabled in the browser.
		/// </summary>
		/// <returns>
		/// True if SAPI capability is enabled in the browser, otherwise false.
		/// </returns>
		public static bool IsEnabled()
		{
			if(HtmlPage.BrowserInformation.Name != "Microsoft Internet Explorer")
			{
				return false;
			}

			bool isEnabled = false;
			try
			{
				//Need to be sure SAPI ActiveX component is able to be used.
				object result = HtmlPage.Window.Invoke("IsSapiEnabled");
				isEnabled = (bool)result;
			}
			catch(Exception)
			{
				//isEnabled is false.
			}

			return isEnabled;
		}



		/// <summary>
		/// Pauses playback of the current phrase.
		/// </summary>
		public override void Pause()
		{
			try
			{
				HtmlPage.Window.Invoke("PauseSpeech");
			}
			catch(Exception)
			{
			}
		}



		/// <summary>
		/// Indicates that the SAPI has finished playing the current phrase.
		/// </summary>
		[ScriptableMember]
		public void PlayAppSpeechCompleted()
		{
			OnFinishedImmediatePhrase();
		}



		/// <summary>
		/// Indicates that the SAPI has finished playing the current phrase.
		/// </summary>
		[ScriptableMember]
		public void PlaySpeechCompleted()
		{
			if(FinishedPlaying != null)
				FinishedPlaying(this, new EventArgs());
		}



		/// <summary>
		/// Resumes playing the current phrase from where it was previously paused.
		/// </summary>
		public override void ResumePlaying()
		{
			try
			{
				HtmlPage.Window.Invoke("ResumeSpeech");
			}
			catch(Exception)
			{
			}
		}



		/// <summary>
		/// Load the given phrase without playing.
		/// </summary>
		/// <param name="currentPhrase">The current phrase.</param>
		public override void LoadPhrase(Phrase currentPhrase)
		{
			//since SAPI voices properly queue themselves
			//we are able to do nothing different than start playing
			StartPlaying(currentPhrase);
		}



		/// <summary>
		/// Starts playing the given phrase.
		/// </summary>
		/// <param name="currentPhrase">The current phrase.</param>
		public override void StartPlaying(Phrase currentPhrase)
		{
            //TODO Should check for unspeakable characters, not just dots and spaces
            string pattern = "^['.' ]*$";
            Match m = Regex.Match(currentPhrase.Text, pattern);

            if (!m.Success)
            {
                try
                {
                    HtmlPage.Window.Invoke("PlaySpeech", currentPhrase.Text);
                }
                catch (Exception)
                {
                }
            } else
            {
                if (FinishedPlaying != null)
                    FinishedPlaying(this, new EventArgs());
            }
		}



		/// <summary>
		/// Stops playback of the current phrase.
		/// </summary>
		public override void Stop()
		{
			try
			{
				HtmlPage.Window.Invoke("StopSpeech");
			}
			catch(Exception)
			{
			}
		}

		#endregion Methods
	}
}