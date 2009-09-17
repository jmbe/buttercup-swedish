using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Buttercup.Control.Model;
using Buttercup.Control.Resources;
using Buttercup.Control.UI;
using Buttercup.Control.UI.Extensions;
using Control.MVP;
using Control.MVP.Views;
using Control.UI;
using Control.UI.Utility;


namespace Buttercup.Control
{
	public partial class Search : UserControl, ISearchView
	{
		#region Fields (8) 

		private Binding _defaultForegroundBinding;
		private KeyCommandHandler _keyCommandHandler;
		private int _oldPageNumTextLength;
		private int _oldSearchTextLength;
		private List<SearchResult> _searchResults;
		private ExtendedListBoxItem _selectedResultsItem;
		private Binding _selectionBackgroundBinding;
		private Binding _selectionForegroundBinding;

		#endregion Fields 



		#region Constructors (1) 

		public Search()
		{
			InitializeComponent();
			Loaded += Search_Loaded;
			_selectionBackgroundBinding = new Binding("HighlightBackground");
			_selectionForegroundBinding = new Binding("HighlightForeground");
			_defaultForegroundBinding = new Binding("DefaultForeground");
			_selectedResultsItem = null;

            KeyDown += Search_KeyDown;
            KeyUp += Search_KeyUp;
		}

		#endregion Constructors 



		#region Properties (4) 

		public IApplicationView ApplicationView { get; set; }

		public int FocusedItem { get; set; }

		public KeyCommandHandler KeyCommandHandler
		{
			get { return _keyCommandHandler; }
			set
			{
				_keyCommandHandler = value;
				_keyCommandHandler.KeyDown += _keyCommandHandler_KeyIsPressed;
			}
		}

		public List<SearchResult> SearchResults
		{
			get { return _searchResults; }
			set
			{
				_searchResults = value;
				PopulateSearchResults();
			}
		}

		#endregion Properties 



		#region Delegates and Events (7) 

		// Events (7) 

		/// <summary>
		/// Raised when the list of search results has changed.
		/// </summary>
		public event EventHandler IndicateSearchResultsChanged;

		public event EventHandler<PageNumEventArgs> NavigateToPage;

		public event EventHandler<SearchEventArgs> SearchForSection;

		public event EventHandler<ItemSelectedEventArgs> SearchSelected;

		public event EventHandler<SpeechTextEventArgs> SelfVoicingSpeakText;

		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelected;

		/// <summary>
		/// Raised when a UI element's help text is required to be spoken.
		/// </summary>
		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelectedHelpText;

		#endregion Delegates and Events 



		#region Methods (23) 

		// Public Methods (1) 

		/// <summary>
		/// Means that the book navigator has been given an invalid page number to navigate to, and the playerpresenter
		/// needs to inform the user
		/// </summary>
		public void PageNavigationOutOfRange(int pageNum, int totalPages)
		{
			//TODO: Visual message to alert user that the page number entered is invalid
			SelfVoicingSpeakText(this, new SpeechTextEventArgs(String.Format(Speech.PageNumberInvalidMessage, pageNum, totalPages)));
		}



		// Private Methods (22) 

		/// <summary>
		/// Raised by the keyboard command handler in response to a keypress to disambiguate the command.
		/// </summary>
		void _keyCommandHandler_KeyIsPressed(object sender, KeyCommandEventArgs e)
		{
            Shortcut keysPressed = new Shortcut(e.keyEventArgs.PlatformKeyCode, e.keyEventArgs.Key, Keyboard.Modifiers);

			//Panel Specific Shortcuts

			//Find parent of the control sending this keypress to see if it is part of this views context.
			Search searchContext = UIElementExtensions.GetUIParentOfType<Search>(e.keyEventArgs.OriginalSource as UIElement);

			//If not null, then we are in the search context
			if(searchContext != null && e.SubViewHandled == false)
			{
				//Block key presses on textboxes in this view from triggering key shortcuts
				if(e.keyEventArgs.OriginalSource == searchTextBox && !keysPressed.Equals(Shortcuts.SpeakHelpText) && !keysPressed.Equals(Shortcuts.ClosePanel))
				{
					e.SubViewHandled = true;
					return;
				}

				//Block up, down, left, right when inside the listboxes on this view
				if(e.keyEventArgs.OriginalSource.ToString() == KeyCommandHandler.ListBoxItem)
				{
					if(keysPressed.Equals(Shortcuts.PreviousHeader) || keysPressed.Equals(Shortcuts.NextHeader) || keysPressed.Equals(Shortcuts.PreviousSection) ||
						keysPressed.Equals(Shortcuts.NextSection))
					{
						e.SubViewHandled = true;
					}
				}

				//Page and Find shortcuts should just switch between those two textboxes?
				if(keysPressed.Equals(Shortcuts.FindPanelPage))
				{
					pageNumTextBox.Focus();
					e.SubViewHandled = true;
				}
				if(keysPressed.Equals(Shortcuts.FindPanelSearch))
				{
					searchTextBox.Focus();
					e.SubViewHandled = true;
				}

				//Enter key for executing page navigation and search is handled in their respective text box keydown events
			}
		}



		/// <summary>
		/// Default handler raised when a button loses focus. Adjusts the foreground binding.
		/// </summary>
		private void Button_LostFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.HandleMouseOutLoseFocus(sender);
		}



		/// <summary>
		/// Default handler raised when the mouse enters a button's region. Adjusts the foreground binding.
		/// </summary>
		private void Button_MouseEnter(object sender, MouseEventArgs e)
		{
			CommonEventHandlerUtility.HandleMouseOver(sender);
		}



		/// <summary>
		/// Default handler raised when the mouse leaves a button's region. Adjusts the foreground binding.
		/// </summary>
		private void Button_MouseLeave(object sender, MouseEventArgs e)
		{
			CommonEventHandlerUtility.HandleMouseOutLoseFocus(sender);
		}



		/// <summary>
		/// This method clears the results list and search textbox of the results
		/// </summary>
		public void ClearSearchResults()
		{
			searchTextBox.Text = "";
			resultsHeader.Text = "Results:";

			searchResultsListBox.Items.Clear();
			if(IndicateSearchResultsChanged != null)
			{
				IndicateSearchResultsChanged(this, new EventArgs());
			}
		}



		/// <summary>
		/// Determines whether the given key is for a numeric digit, for self-voicing and so that the page number text box
		/// can prevent other key commands from bubbling up.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		/// True if the given key is a number, otherwise false.
		/// </returns>
		private Boolean isDigitKey(Key key)
		{
			if(key != Key.Tab || Keyboard.Modifiers != ModifierKeys.Shift)
			{
				//Only allow digit input into the page number text box

				// Determine whether the keystroke is a number from the top of the keyboard.
				// Also allow the Tab key for form navigation
				if((key < Key.D0 || key > Key.D9) && key != Key.Tab)
				{
					// Determine whether the keystroke is a number from the keypad.
					if(key < Key.NumPad0 || key > Key.NumPad9)
					{
						// Determine whether the keystroke is a backspace.
						if(key != Key.Back)
						{
							// A non-numerical keystroke was pressed.
							// Set the flag to true and evaluate in KeyPress event.
							return false;
						}
					}
				}
				//If shift key was pressed, it's not a number.
				if(Keyboard.Modifiers == ModifierKeys.Shift)
				{
					return false;
				}
			}

			return true;
		}



		/// <summary>
		/// Raised when the source ListBox receives focus. This is to allow the focus state
		/// to be visually emphasised.
		/// </summary>
		private void ListBoxGotFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.ChangeFocusBorder(sender, true);
		}



		/// <summary>
		/// Raised when the source ListBox loses focus. This is to allow the previously focussed
		/// Listbox to be rendered in its default state.
		/// </summary>
		private void ListBoxLostFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.ChangeFocusBorder(sender, false);
		}



		private void pageNumButton_Click(object sender, RoutedEventArgs e)
		{
			if(!String.IsNullOrEmpty(pageNumTextBox.Text))
			{
				int pageNum = Int32.Parse(pageNumTextBox.Text);
				SelfVoicingSpeakText(this, new SpeechTextEventArgs(String.Format("Navigating to page {0}", pageNum)));
				NavigateToPage(this, new PageNumEventArgs(pageNum));
				searchTextBox.SelectAll();
			}
		}



		/// <summary>
		/// Handles the GotFocus event of the pageNumTextBox control to provide self-voicing.
		/// </summary>
		private void pageNumTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if(pageNumTextBox.Text != "")
			{
				ApplicationVoice.MainInstance.QueueTextToSpeak(String.Format(Speech.TextBoxContentsMessage, pageNumTextBox.Text));
			}

			SpeakControlOnFocus(sender, e);
		}



		/// <summary>
		/// Handles the KeyDown event of the page number textbox control to perform navigation on the Enter keypress.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
		private void pageNumTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				pageNumButton_Click(this, new RoutedEventArgs());
			}

			if(!isDigitKey(e.Key) && e.Key != Shortcuts.SpeakHelpText.ShortcutKey && e.Key != Shortcuts.ClosePanel.ShortcutKey)
			{
				e.Handled = true;
			}
		}



		/// <summary>
		/// Voicing of characters as the user types them into the page text box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void pageNumTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			//Only read out characters when valid character keys (e.g. alphanumeric, symbols) are entered
			if(e.Key != Key.Shift && e.Key != Key.Enter && e.Key != Key.Tab && e.Key != Key.Back && isDigitKey(e.Key))
			{
				if(pageNumTextBox.Text.Length > 0 && pageNumTextBox.Text.Length != _oldPageNumTextLength)
				{
					string keyPressed = "" + pageNumTextBox.Text[pageNumTextBox.Text.Length - 1];
					SelfVoicingSpeakText(this, new SpeechTextEventArgs(keyPressed));
				}
			}

			//read out the letters that they are deleting
			if(e.Key == Key.Back && pageNumTextBox.LastCharDeleted != (char)0)
			{
				string keyDeleted = "" + pageNumTextBox.LastCharDeleted;
				SelfVoicingSpeakText(this, new SpeechTextEventArgs(keyDeleted));
			}

			_oldPageNumTextLength = pageNumTextBox.Text.Length;
		}



		/// <summary>
		/// Updates the list of search results when the SearchResults property is set.
		/// </summary>
		private void PopulateSearchResults()
		{
			searchResultsListBox.Items.Clear();
			Style listItemStyle = Application.Current.Resources[Strings.ListItemStyleKey] as Style;

			if(SearchResults == null || SearchResults.Count == 0)
			{
				TextBlock noResults = new TextBlock();
				noResults.Text = "No search results found.";
				resultsHeader.Text = "Results:";

				ExtendedListBoxItem newItem = new ExtendedListBoxItem();
				newItem.Content = noResults;
				AutomationProperties.SetHelpText(newItem, "No results found for this search, please go back and try a different search.");
				searchResultsListBox.Items.Add(newItem);
				if(IndicateSearchResultsChanged != null)
				{
					IndicateSearchResultsChanged(this, new EventArgs());
				}

				SelfVoicingSpeakText(this, new SpeechTextEventArgs(noResults.Text));
				return;
			}

			//Populate the search results box
			foreach(SearchResult result in SearchResults)
			{
				//Construct ListBoxItem from textblock of multiple runs
				ExtendedListBoxItem currentItem = new ExtendedListBoxItem();
				currentItem.Style = listItemStyle;
				currentItem.SetBinding(ExtendedListBoxItem.SelectionBackgroundProperty, _selectionBackgroundBinding);
				currentItem.SetBinding(ForegroundProperty, _defaultForegroundBinding);

				TextBlock displayedResult = new TextBlock();

				int currentIndex = 0;

				while(currentIndex < result.Text.Length)
				{
					// get the next occurance of the search string in the search result
					int nextSearchStringIndex = result.Text.Substring(currentIndex).ToLower().IndexOf(result.SearchString.ToLower());

					if(nextSearchStringIndex == -1)
					{
						break;
					}

					// construct a run for the text leading up to the match
					if(currentIndex != nextSearchStringIndex)
					{
						Run preSearchWord = new Run();
						preSearchWord.Text = result.Text.Substring(currentIndex, nextSearchStringIndex);
						displayedResult.Inlines.Add(preSearchWord);
					}

					//if (nextSearchStringIndex)
					Run SearchWord = new Run();

					SearchWord.Text = result.Text.Substring(currentIndex + nextSearchStringIndex, result.SearchString.Length);
					SearchWord.FontWeight = FontWeights.Bold;
					displayedResult.Inlines.Add(SearchWord);

					currentIndex += (nextSearchStringIndex + result.SearchString.Length);
				}

				Run postSearchWord = new Run();

				if(currentIndex != result.Text.Length)
				{
					postSearchWord.Text = result.Text.Substring(currentIndex);
					displayedResult.Inlines.Add(postSearchWord);
				}

				displayedResult.TextWrapping = TextWrapping.Wrap;
				currentItem.Content = displayedResult;
				AutomationProperties.SetHelpText(currentItem, result.Text + ". Use the up and down arrow keys to scroll through the other search results.");
			    
                //We don't want to have to tab through all of the search result items, since up and down already do that
                currentItem.TabNavigation = KeyboardNavigationMode.Once;
			    currentItem.IsTabStop = false;

				searchResultsListBox.Items.Add(currentItem);
			}
            
			searchResultsListBox.InvalidateMeasure();
			if(IndicateSearchResultsChanged != null)
			{
				IndicateSearchResultsChanged(this, new EventArgs());
			}

			//TODO: Distinguish plural and singular (i.e. result from results)
			resultsHeader.Text = String.Format("Results: ({0} results found)", SearchResults.Count);

			SelfVoicingSpeakText(this, new SpeechTextEventArgs(string.Format(Speech.ResultsFoundMessage, SearchResults.Count)));
		}

        /// <summary>
        /// Capture KeyDown events, needed in each subview as the Scrollviewer control
        /// that the panels are situated in will not let certain keypresses through
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		void Search_KeyDown(object sender, KeyEventArgs e)
		{
            ScrollViewer panelScrollViewer = UIElementExtensions.GetUIParentOfType<ScrollViewer>(e.OriginalSource as UIElement);

			//If we are not in a scrollviewer, or the scrollviewer has a vertical scrollbar visible, then arrows won't be let through
            if (panelScrollViewer == null || panelScrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            {
                if (e.Handled == false &&
                    (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.PageUp || e.Key == Key.PageDown || e.Key == Key.Home ||
                     e.Key == Key.End))
                {
                    KeyCommandHandler.KeyPressed(new KeyCommandEventArgs(e));
                }
            }
		}


        /// <summary>
        /// Capture KeyUp events, needed in each subview as the Scrollviewer control
        /// that the panels are situated in will not let certain keypresses through
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		void Search_KeyUp(object sender, KeyEventArgs e)
		{
            ScrollViewer panelScrollViewer = UIElementExtensions.GetUIParentOfType<ScrollViewer>(e.OriginalSource as UIElement);

			//If we are not in a scrollviewer, or the scrollviewer has a vertical scrollbar visible, then arrows won't be let through
            if (panelScrollViewer == null || panelScrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            {
                if (e.Handled == false &&
                    (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.PageUp || e.Key == Key.PageDown || e.Key == Key.Home ||
                     e.Key == Key.End))
                {
                    KeyCommandHandler.KeyReleased(new KeyCommandEventArgs(e));
                }
            }
		}



		void Search_Loaded(object sender, RoutedEventArgs e)
		{
			//Set the focus for a newly loaded control here...
			//Depends on what state has been set
			if(FocusedItem == SearchState.FocusGoToPage)
			{
				pageNumTextBox.Focus();
			}
			if(FocusedItem == SearchState.FocusFindText)
			{
				searchTextBox.Focus();
			}

			AutomationProperties.SetLabeledBy(pageNumTextBox, pageNumLabel);
			ApplicationVoice.MainInstance.QueueTextToSpeak(Speech.FindPanelMessage);
		}



		private void searchButton_Click(object sender, RoutedEventArgs e)
		{
			String searchString = searchTextBox.Text;
			searchString = searchString.Trim(); //Trim off leading and trailing white space

			//Only search if trimmed search string not null, not empty
			if(!String.IsNullOrEmpty(searchString))
			{
				ApplicationVoice.MainInstance.QueueTextToSpeak("Searching for " + searchString);
				SearchForSection(this, new SearchEventArgs(searchString));
				searchTextBox.SelectAll(); // Do a select all on the search string, makes it easier for user to clear the box
			}
			else
			{
				//Clear search results and indicate to user a valid search parameter needs to be entered.
				ClearSearchResults();
				SelfVoicingSpeakText(this, new SpeechTextEventArgs("Please enter text into find text box"));
			}
		}



		private void searchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            //if(searchResultsListBox != null) //Need to focus the parent listbox
            //    searchResultsListBox.Focus();

			if(searchResultsListBox.SelectedIndex > -1)
			{
				ExtendedListBoxItem currentItem = searchResultsListBox.SelectedItem as ExtendedListBoxItem;

				if(currentItem != null)
				{
					//Remove highlighting from the previously selected item.
					if(_selectedResultsItem != null)
					{
						_selectedResultsItem.SetBinding(ForegroundProperty, _defaultForegroundBinding);
					}

					currentItem.SetBinding(ForegroundProperty, _selectionForegroundBinding);
					_selectedResultsItem = currentItem;
				}

				SearchSelected(sender, new ItemSelectedEventArgs(SearchResults[searchResultsListBox.SelectedIndex].ID));
			}
		}



		private void searchTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if(searchTextBox.Text != "")
			{
				ApplicationVoice.MainInstance.QueueTextToSpeak(String.Format(Speech.TextBoxContentsMessage, searchTextBox.Text));
			}
			SpeakControlOnFocus(sender, e);
		}



		private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				searchButton_Click(this, new RoutedEventArgs());
			}
		}



		/// <summary>
		/// Voicing of characters as the user types them into the search text box
		/// TODO On deletion should speak _all_ characters that have been selected and deleted
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void searchTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.Key != Key.Shift && e.Key != Key.Enter && e.Key != Key.Tab && e.Key != Key.Back)
			{
				if(searchTextBox.Text.Length > 0 && searchTextBox.Text.Length != _oldSearchTextLength)
				{
					string keyPressed = "" + searchTextBox.Text[searchTextBox.Text.Length - 1];
					SelfVoicingSpeakText(this, new SpeechTextEventArgs(keyPressed));
				}
			}

			//read out the letters that they are deleting
			if(e.Key == Key.Back && searchTextBox.LastCharDeleted != (char)0)
			{
				string keyDeleted = "" + searchTextBox.LastCharDeleted;
				SelfVoicingSpeakText(this, new SpeechTextEventArgs(keyDeleted));
			}

			_oldSearchTextLength = searchTextBox.Text.Length;
		}



		/// <summary>
		/// Handles the GotFocus event of a button. This is used to initiate speaking the voice command.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e"></param>
		private void SpeakControlOnFocus(object sender, RoutedEventArgs e)
		{
			//Change content's foreground binding only for buttons (since this handler is also called by textboxes)
			if(sender is Button)
			{
				CommonEventHandlerUtility.HandleGetFocus(sender);
			}

			if(SpeakableElementSelected != null)
			{
				SpeakableElementSelected(this, new ElementSelectedEventArgs(sender as DependencyObject));
			}
		}

		#endregion Methods 
	}
}