using System;

namespace Control.MVP
{
    public class SpeechTextEventArgs : EventArgs
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for the SpeechTextEventArgs, this EventArgs is used
        /// for passing the Text Speech.
        /// </summary>
        /// <param name="speech"></param>
        public SpeechTextEventArgs(string speech)
        {
            Speech = speech;
        }

        #endregion Constructors

        #region Properties (1)

        public string Speech { get; set; }

        #endregion Properties
    }
}