using System;

namespace Control.MVP
{
    public class VolumeChangedEventArgs : EventArgs
    {
        #region Constructors (1)

        public VolumeChangedEventArgs(double volume)
        {
            Volume = volume;
        }

        #endregion Constructors

        #region Properties (1)

        public double Volume { get; set; }

        #endregion Properties
    }
}