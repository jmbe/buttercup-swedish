using System;
using Buttercup.Control.Model;

namespace Control.MVP
{
    public class ContrastSchemeEventArgs : EventArgs
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for the ContrastSchemeEventArgs, this EventArgs will be used to pass
        /// the contrast scheme.
        /// </summary>
        /// <param name="contrastScheme"></param>
        public ContrastSchemeEventArgs(ContrastLevel contrastScheme)
        {
            ContrastScheme = contrastScheme;
        }

        #endregion Constructors

        #region Properties (1)

        public ContrastLevel ContrastScheme { get; private set; }

        #endregion Properties
    }
}