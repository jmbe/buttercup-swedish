using System;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.UI;


namespace Control.MVP.Views
{
	public interface INavigationView
	{
		#region Data Members (7) 

		/// <summary>
		/// This view requires access to the application and player views to respond to application
		/// events like new books loading. The ApplicationPresenter will be responsible for
		/// setting this property.
		/// </summary>
		IApplicationView ApplicationView { get; set; }

		/// <summary>
		/// Holds the current book that we are displaying information for.
		/// </summary>
		Book CurrentBook { get; set; }

		/// <summary>
		/// Holds the table of contents for the currently loaded book, so the view
		/// can be populated correctly. This is set when a new book has been loaded.
		/// </summary>
		TableOfContents CurrentTableOfContents { get; set; }

		int FocusedItem { get; set; }

		/// <summary>
		/// Handler for shortcut key events
		/// </summary>
		KeyCommandHandler KeyCommandHandler { get; set; }

		/// <summary>
		/// The navigation view needs to know about the player view in order to respond correctly to
		/// the section changing as the book plays, so the navigation can correctly highlight the
		/// section heading.
		/// </summary>
		IPlayerView PlayerView { get; set; }

		/// <summary>
		/// Holds the index of the currently selected heading
		/// TODO: Rename this to selectedheading?
		/// </summary>
		int SelectedElement { get; set; }

		#endregion Data Members 



		#region Delegates and Events (5) 

		// Events (5) 

		/// <summary>
		/// Raised when the list of search results has changed.
		/// </summary>
		event EventHandler IndicateSearchResultsChanged;

		/// <summary>
		/// Raised when a heading has been selected in the view to navigate to
		/// </summary>
		event EventHandler<ItemSelectedEventArgs> ItemSelected;

		/// <summary>
		/// Raised when some contextual speech text is to be spoken.
		/// </summary>
		event EventHandler<SpeechTextEventArgs> SelfVoicingSpeakText;

		/// <summary>
		/// Raised when a UI element is required to be spoken.
		/// </summary>
		event EventHandler<ElementSelectedEventArgs> SpeakableElementSelected;

		/// <summary>
		/// Raised when a UI element help text is required to be spoken.
		/// </summary>
		event EventHandler<ElementSelectedEventArgs> SpeakableElementSelectedHelpText;

		#endregion Delegates and Events 
	}
}