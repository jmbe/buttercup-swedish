using System;
using System.Collections.Generic;
using Buttercup.Control.Model;
using Buttercup.Control.Model.Entities;
using Control.MVP.Views;


namespace Control.MVP.Presenters
{
	public class NavigationPresenter : Presenter
	{
		#region Fields (1) 

		private readonly ApplicationPresenter _mainPresenter;

		#endregion Fields 



		#region Constructors (1) 

		/// <summary>
		/// This constructor initialises the dependent Navigation view, and maintains references to
		/// the main Application Presenter and the Main Application state (which holds a needed reference
		/// to the current book).
		/// </summary>
		/// <param name="view"></param>
		/// <param name="mainPresenter"></param>
		/// <param name="mainState"></param>
		public NavigationPresenter(INavigationView view, ApplicationPresenter mainPresenter, ApplicationState mainState)
		{
			View = view;

			View.IndicateSearchResultsChanged += IndicateSearchResultsChanged;
			View.PlayerView.SectionChanged += PlayerView_SectionChanged;
			View.ApplicationView.BookChanged += ApplicationView_BookChanged;
			View.ApplicationView.NavigationViewFocusChanged += ApplicationView_NavigationViewFocusChanged;

			//view.SelfVoicingSpeakText += view_SelfVoicingSpeakText;

			_mainPresenter = mainPresenter;
			base.MainState = mainState;
		}

		#endregion Constructors 



		#region Properties (1) 

		/// <summary>
		/// A reference to the dependent navigation view.
		/// </summary>
		private INavigationView View { get; set; }

		#endregion Properties 



		#region Methods (4) 

		// Private Methods (4) 

		/// <summary>
		/// Responds to the event where a new book has been loaded. Updates the reference to the
		/// new book's table of contents.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ApplicationView_BookChanged(object sender, EventArgs e)
		{
			//Set the current book so we  can retrieve the current book information
			View.CurrentBook = base.MainState.CurrentBook;
			// Reset the table of contents to the new book.
			View.CurrentTableOfContents = base.MainState.CurrentBook.TableOfContents;
			//Notify that the panel contents have changed.
			_mainPresenter.NotifyPanelContentsChanged();
		}



		void ApplicationView_NavigationViewFocusChanged(object sender, PanelFocusedItemEventArgs e)
		{
			View.FocusedItem = e.FocusedItem;
		}



		/// <summary>
		/// Indicates that the search results list has changed.
		/// </summary>
		private void IndicateSearchResultsChanged(object sender, EventArgs e)
		{
			_mainPresenter.NotifyPanelContentsChanged();
		}



		/// <summary>
		/// Responds to the player view when it has moved to playing a new section. This presenter then
		/// updates the view to highlight the correct heading for the section being played.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayerView_SectionChanged(object sender, EventArgs e)
		{
			//change selected element in the navigation listbox to the heading of the current section
			if(View.CurrentTableOfContents != null)
			{
				List<Heading> headings = View.CurrentTableOfContents.FlatHeadingList;
				for(int i = 0; i < headings.Count; i++)
				{
					if(headings[i].ID == View.PlayerView.CurrentHeadingID)
					{
						if(View.SelectedElement != i)
						{
							View.SelectedElement = i;
						}
						break;
					}
				}
			}
		}

		#endregion Methods 
	}
}