using System;

namespace Control.MVP
{
    public class PageNumEventArgs : EventArgs
    {
        #region Constructors (1)

        /// <summary>
        /// Constructor for the PageNumEventArgs, this EventArgs will be used to pass
        /// an integer indicating a page number.
        /// </summary>
        /// <param name="pageNum"></param>
        public PageNumEventArgs(int pageNum)
        {
            PageNum = pageNum;
        }

        #endregion Constructors

        #region Properties (1)

        public int PageNum { get; set; }

        #endregion Properties
    }
}