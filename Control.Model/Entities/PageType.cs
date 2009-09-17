namespace Buttercup.Control.Model.Entities
{
    public enum PageType
    {
        /// <summary>
        /// Pages in the front section of the book, usually given in Roman numerals.
        /// </summary>
        Front,
        /// <summary>
        /// Pages in the main section of the book, usually given in standard digits.
        /// </summary>
        Normal,
        /// <summary>
        /// 'Special' pages that may be used in the book, e.g. blank separator pages between chapters.
        /// </summary>
        Special
    }
}