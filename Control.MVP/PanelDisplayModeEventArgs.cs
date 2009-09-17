using System;
using Buttercup.Control.Model.Entities;

namespace Control.MVP
{
    public class PanelDisplayModeEventArgs : EventArgs
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for the PanelDisplayModeEventArgs, this EventArgs will be used to pass
        /// a display mode.
        /// </summary>
        /// <param name="displayMode"></param>
        public PanelDisplayModeEventArgs(SidePanelDisplayMode displayMode)
        {
            PanelDisplayMode = displayMode;
        }

        #endregion Constructors

        #region Properties (1)

        public SidePanelDisplayMode PanelDisplayMode { get; private set; }

        #endregion Properties
    }
}