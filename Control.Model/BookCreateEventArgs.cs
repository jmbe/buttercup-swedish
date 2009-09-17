using System;
using Buttercup.Control.Model.Entities;

namespace Buttercup.Control.Model
{
    public class BookCreateEventArgs : EventArgs
    {
		#region Constructors (1) 

        public BookCreateEventArgs(Book book)
        {
            Book = book;
        }

		#endregion Constructors 

		#region Properties (1) 

        public Book Book { get; set; }

		#endregion Properties 
    }
}