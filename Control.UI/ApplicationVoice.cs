using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.UI.Extensions;
using Control.UI.Voice;

namespace Control.UI
{
    public class ApplicationVoice
    {
		#region Fields (8) 

        private const string _BUTTON = "Buttercup.Control.MediaButton";
        private readonly bool _isEnabled;
        private const string _LISTBOXITEM = "Buttercup.Control.ExtendedListBoxItem";
        private static ApplicationVoice _mainInstance;
        private const string _SLIDER = "System.Windows.Controls.Primitives.Thumb";
        private const string _MEDIASLIDER = "Buttercup.Control.MediaSlider";
        private const string _TEXTBOX = "Buttercup.Control.MediaTextBox";
        private readonly VoiceBase _voice;
        private static readonly Queue<string> speakingQueue = new Queue<string>();

		#endregion Fields 

		#region Constructors (1) 

        /// <summary>
        /// Application voice uses a separate SAPI voice to speak self-voicing text.
        /// </summary>
        private ApplicationVoice()
        {
            _isEnabled = false;

            if (SapiVoice.IsEnabled())
            {
                _voice = new SapiVoice(true);
                _voice.FinishedImmediatePhrase += OnSpeakFinished;

                _isEnabled = true;
            }
        }

		#endregion Constructors 

		#region Properties (3) 

        public static ApplicationVoice MainInstance
        {
            get
            {
                if (_mainInstance == null)
                {
                    _mainInstance = new ApplicationVoice();
                }
                return _mainInstance;
            }
        }

        private VoiceBase Voice
        {
            get { return _voice; }
        }

        /// <summary>
        /// Sets the volume for the SAPI application voice
        /// </summary>
        public double Volume
        {
            get { return _voice.Volume; }
            set
            {
                if (_isEnabled)
                    _voice.Volume = value;
            }
        }

        public int Rate 
        {
            get
            {
                return _voice.Rate;
            }
            set
            {
                if (_isEnabled)
                    _voice.Rate = value;
            }
        }

		#endregion Properties 

		#region Delegates and Events (1) 

		// Events (1) 

        public event EventHandler PhraseFinished;

		#endregion Delegates and Events 

		#region Methods (9) 

		// Public Methods (6) 

        //TODO Proper voice queuing in coordination with SpeakElement and other application speaking methods
        //TODO Otherwise, can probably remove the queueing altogether and rework the calling code in the presenters/views
        /// <summary>
        /// Clear the queue of text which will be spoken next
        /// </summary>
        public void ClearSpeakTextQueue()
        {
            speakingQueue.Clear();
        }

        /// <summary>
        /// Add text to the current queue of text to be spoken
        /// </summary>
        /// <param name="text"></param>
        public void QueueTextToSpeak(string text)
        {
            speakingQueue.Enqueue(text);
        }

        /// <summary>
        /// Speak the AutomationProperties.Name property of a given UI element
        /// </summary>
        /// <param name="element"></param>
        public void Speak(DependencyObject element)
        {
            if (element != null)
            {
                DependencyObject toSpeak = element;

                //We have special cases for various UI elements
                if ((element as FrameworkElement).ToString() == _SLIDER)
                {
                    //The keydown event originates from the Slider Thumb, so get the parent slider element
                    toSpeak = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(element)));
                }

                if (toSpeak != null && AutomationProperties.GetName(toSpeak) != null)
                {
                    // + ". " + AutomationProperties.GetAcceleratorKey(toSpeak)
                    QueueTextToSpeak(AutomationProperties.GetName(toSpeak) + " " + ControlName(toSpeak));
                    SpeakTextFromQueue();
                }
            }
        }

        /// <summary>
        /// Speak the given string of text, as well as anything stored in the speaking queue
        /// </summary>
        /// <param name="text"></param>
        public void Speak(string text)
        {
            QueueTextToSpeak(text);
            SpeakTextFromQueue();
        }

        /// <summary>
        /// Speak the AutomationProperties.HelpText property of a given UI element
        /// </summary>
        /// <param name="element"></param>
        public void SpeakHelpText(DependencyObject element)
        {
            if (element != null)
            {
                DependencyObject toSpeak = element;

                //We have special cases for various UI elements
                if ((element as FrameworkElement).ToString() == _SLIDER)
                {
                    //The keydown event originates from the Slider Thumb, so get the parent slider element
                    toSpeak = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(element)));
                }

                if (toSpeak != null && AutomationProperties.GetHelpText(toSpeak) != null)
                {
                    QueueTextToSpeak(AutomationProperties.GetHelpText(toSpeak));
                    SpeakTextFromQueue();
                }
            }
        }

        public void StopSpeaking()
        {
            if (Voice != null)
            {
                Voice.InterruptStop();
            }

            if (PhraseFinished != null)
            {
                PhraseFinished(this, new EventArgs());
            }
        }

        /// <summary>
        /// Speak all the text in the current speaking queue
        /// </summary>
        public void SpeakTextFromQueue()
        {
            //Check text queue to see if anything is left to speak.
            string wholePhrase = "";

            while (speakingQueue.Count > 0)
            {
                wholePhrase += ". " + speakingQueue.Dequeue();
            }

            if (_isEnabled)
                Voice.InterruptPlayback(CreatePhrase(wholePhrase));
        }
		// Private Methods (3) 

        private string ControlName(DependencyObject element)
        {
            string elementType = element.GetType().ToString();

            if (elementType == _BUTTON) return "Button";
            //if (elementType == _LISTBOXITEM) return "List Box";
            if (elementType == _SLIDER) return "Slider";
            if (elementType == _MEDIASLIDER) return "Slider";
            if (elementType == _TEXTBOX) return "Text Box";

            return "";
        }

        /// <summary>
        /// Create an audio phrase from text for the SAPI Voice to speak
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Phrase CreatePhrase(string text)
        {
            return new Phrase { Text = text, Audio = null };
        }

        /// <summary>
        /// Event raised when application voice has finished speaking, player subscribes to this
        /// so it knows when it can resume playing book after interruption.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeakFinished(object sender, EventArgs e)
        {
            if (PhraseFinished != null)
            {
                PhraseFinished(this, e);
            }
        }

		#endregion Methods 
    }
}