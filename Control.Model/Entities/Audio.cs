using System;
using System.IO;

namespace Buttercup.Control.Model.Entities
{
    /// <summary>
    /// Defines an Audio object which makes up part of a Book Phrase
    /// Holds the audio stream as well as the timing for the audio clip
    /// </summary>
    public class Audio
    {
        #region Properties (5)

        public TimeSpan ClipEnd { get; set; }

        public TimeSpan ClipStart { get; set; }

        public bool IsStreamedSource
        {
            get { return SourceStream != null; }
        }

        public Stream SourceStream { get; set; }

        public Uri SourceUri { get; set; }

        #endregion Properties
    }
}