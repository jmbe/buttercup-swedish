using System;
using Buttercup.Control.Model;
using Control.MVP.Views;


namespace Control.MVP.Presenters
{
	public class SearchPresenter
	{
		#region Fields (2) 

		private readonly ApplicationPresenter _mainPresenter;
		private SearchState _state;

		#endregion Fields 



		#region Constructors (1) 

		/// <summary>
		/// This constructor initialises the dependent Search view, and maintains references to
		/// the main Application Presenter and the Search state.
		/// </summary>
		/// <param name="mainPresenter"></param>
		/// <param name="viewReference"></param>
		/// <param name="state"></param>
		internal SearchPresenter(ISearchView viewReference, ApplicationPresenter mainPresenter,
			SearchState state)
		{
			_mainPresenter = mainPresenter;
			View = viewReference;
			_state = state;

			viewReference.IndicateSearchResultsChanged += IndicateSearchResultsChanged;
			viewReference.ApplicationView.SearchViewFocusChanged += ApplicationView_SearchViewFocusChanged;
		}

		#endregion Constructors 



		#region Properties (1) 

		private ISearchView View { get; set; }

		#endregion Properties 



		#region Methods (3) 

		// Private Methods (2) 

		private void ApplicationView_SearchViewFocusChanged(object sender, PanelFocusedItemEventArgs e)
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



		// Internal Methods (1) 

		/// <summary>
		/// Called when the book is changed.
		/// </summary>
		internal void ClearSearchResults()
		{
			View.ClearSearchResults();
		}

		#endregion Methods 
	}
}