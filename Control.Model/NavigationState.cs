namespace Buttercup.Control.Model
{
    public class NavigationState
    {
        public enum Focus
        {
            Contents,
            Bookmarks,
            BookInformation
        } ;

		#region Properties (1) 

        /// <summary>
        /// Holds the item which will be focused when the panel opens
        /// </summary>
        public int FocusedItem { get; set; }

		#endregion Properties 
    }
}