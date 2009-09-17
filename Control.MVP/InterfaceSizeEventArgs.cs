using System;

namespace Control.MVP
{
    public class InterfaceSizeEventArgs : EventArgs
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for the InterfaceSizeEventArgs, this EventArgs will be used to pass
        /// the interface size.
        /// </summary>
        /// <param name="interfaceSize"></param>
        public InterfaceSizeEventArgs(int interfaceSize)
        {
            InterfaceSize = interfaceSize;
        }

        #endregion Constructors

        #region Properties (1)

        public int InterfaceSize { get; private set; }

        #endregion Properties
    }
}