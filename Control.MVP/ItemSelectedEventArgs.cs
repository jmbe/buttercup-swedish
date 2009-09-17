using System;

namespace Control.MVP
{
    public class ItemSelectedEventArgs : EventArgs
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for the ItemSelectedEventArgs, this EventArgs will be used to pass the
        /// ID of an object.
        /// </summary>
        /// <param name="id"></param>
        public ItemSelectedEventArgs(string id)
        {
            ID = id;
        }

        #endregion Constructors

        #region Properties (1)

        public string ID { get; set; }

        #endregion Properties
    }
}