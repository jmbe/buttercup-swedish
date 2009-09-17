using System;

namespace Control.UI
{
    public class BookElementSelectedEventArgs : EventArgs
    {
        #region Constructors (1)

        public BookElementSelectedEventArgs(string id)
        {
            Id = id;
        }

        #endregion Constructors

        #region Properties (1)

        public string Id { get; set; }

        #endregion Properties
    }
}
