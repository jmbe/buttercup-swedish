using Buttercup.Control.Model.Entities;

namespace Buttercup.Control.UI
{
    /// <summary>
    /// This class is the primary class for acting on a book.
    /// </summary>
    public class Reader
    {
        #region Constructors (1)

        /// <summary>
        /// Construct a reader with a give Surface
        /// </summary>
        /// <param name="bookSurface"></param>
        public Reader(Surface bookSurface)
        {
            BookSurface = bookSurface;
        }

        #endregion Constructors

        #region Properties (2)

        public Book Book { get; set; }

        public Surface BookSurface { get; set; }

        #endregion Properties
    }
}