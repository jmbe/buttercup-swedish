using System;
using System.Windows;

namespace Control.MVP
{
    public class ElementSelectedEventArgs : EventArgs
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for the ElementSelectedEventArgs, this EventArgs will be used to pass
        /// a UI element.
        /// </summary>
        /// <param name="element"></param>
        public ElementSelectedEventArgs(DependencyObject element)
        {
            Element = element;
        }

        #endregion Constructors

        #region Properties (1)

        public DependencyObject Element { get; set; }

        #endregion Properties
    }
}