using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Buttercup.Control.Common;
using Buttercup.Control.Model;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.Resources;
using Buttercup.Control.UI;
using Buttercup.Control.UI.Extensions;
using Control.MVP;
using Control.MVP.Views;
using Control.UI;
using Control.UI.Utility;
using Buttercup.Control.UI.ViewModel;


namespace Buttercup.Control
{
	public partial class Navigation : UserControl, INavigationView
	{
		#region Fields (8) 

		private const int _contentsTabIndex = 0;
		private Book _currentBook;
		private Binding _defaultForegroundBinding;
		private KeyCommandHandler _keyCommandHandler;
		private bool _resetCurrentPositionToHeading;
		private ExtendedListBoxItem _selectedContentsItem;
		private Binding _selectionBackgroundBinding;
		private Binding _selectionForegroundBinding;
		private TableOfContents _tableOfContents;

		#endregion Fields 



		#region Constructors (1) 

		public Navigation()
		{
			InitializeComponent();

			Loaded += Navigation_Loaded;
			_selectionBackgroundBinding = new Binding("HighlightBackground");
			_selectionForegroundBinding = new Binding("HighlightForeground");
			_defaultForegroundBinding = new Binding("DefaultForeground");
			_selectedContentsItem = null;

			_resetCurrentPositionToHeading = true;

            KeyDown += Navigation_KeyDown;
            KeyUp += Navigation_KeyUp;
		}

		#endregion Constructors 



		#region Properties (7) 

		public IApplicationView ApplicationView { get; set; }

		public Book CurrentBook
		{
			get { return _currentBook; }
			set
			{
				_currentBook = value;
				PopulateBookInformation();
			}
		}

		public TableOfContents CurrentTableOfContents
		{
			get { return _tableOfContents; }
			set
			{
				_tableOfContents = value;
				PopulateTableOfContents();
			}
		}

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

		public IPlayerView PlayerView { get; set; }

		public int SelectedElement
		{
			get { return TableOfContentsListBox.SelectedIndex; }
			set
			{
				_resetCurrentPositionToHeading = false;
				try
				{
					TableOfContentsListBox.SelectedIndex = value;
				}
				catch(Exception e)
				{
					Logger.Log(e);
				}
			}
		}

		internal ScrollViewer ParentViewer
		{ get; set; }

		#endregion Properties 



		#region Delegates and Events (5) 

		// Events (5) 

		/// <summary>
		/// Raised when the list of search results has changed.
		/// </summary>
		public event EventHandler IndicateSearchResultsChanged;

		public event EventHandler<ItemSelectedEventArgs> ItemSelected;

		public event EventHandler<SpeechTextEventArgs> SelfVoicingSpeakText;

		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelected;

		/// <summary>
		/// Raised when a UI element's help text is required to be spoken.
		/// </summary>
		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelectedHelpText;

		#endregion Delegates and Events 



		#region Methods (15) 

		// Public Methods (1) 

		/// <summary>
		/// Refreshes the contrast setting for this control. Currently this is needed for maintaining the
		/// visual integrity of the navigation panel selector.
		/// </summary>
		public void RefreshContrast()
		{
			UpdateNavTabItems();
		}


		/// <summary>
		/// Called when the contents panel becomes visible to ensure the current item is visible.
		/// </summary>
		internal void RefreshContentsScrollPosition()
		{
			ExtendedListBoxItem currentItem = TableOfContentsListBox.SelectedItem as ExtendedListBoxItem;

			//Ensure the element is scrolled into view.
			if(ParentViewer != null)
				currentItem.ScrollIntoView(ParentViewer, DisplayAttributes.CommonInstance.VisualScale);
		}


		// Private Methods (14) 

		void _keyCommandHandler_KeyIsPressed(object sender, KeyCommandEventArgs e)
		{
            Shortcut keysPressed = new Shortcut(e.keyEventArgs.PlatformKeyCode, e.keyEventArgs.Key, Keyboard.Modifiers);

			//Universal Shortcuts

			//Single key shortcuts
			if(e.SubViewHandled == false)
			{
				if(keysPressed.Equals(Shortcuts.FirstHeading))
				{
					//Navigate to the first heading in the book
					if(TableOfContentsListBox.Items.Count > 0)
					{
						SelectedElement = 0;
						_resetCurrentPositionToHeading = true;
						NavigateToHeadingID(0);
					}
				}
				if(keysPressed.Equals(Shortcuts.LastHeading))
				{
					//Navigate to the last heading in the book
					if(TableOfContentsListBox.Items.Count > 0)
					{
						SelectedElement = TableOfContentsListBox.Items.Count - 1;
						_resetCurrentPositionToHeading = true;
						NavigateToHeadingID(TableOfContentsListBox.Items.Count - 1);
					}
				}
			}

			//Panel Specific Shortcuts

			//Find parent of the control sending this keypress to see if it is part of this views context.
			Navigation navigationContext = UIElementExtensions.GetUIParentOfType<Navigation>(e.keyEventArgs.OriginalSource as UIElement);

			//If not null, then we are in the search context
			if(navigationContext != null && e.SubViewHandled == false)
			{
				//Block up, down, left, right when inside the listboxes on this view
				if(e.keyEventArgs.OriginalSource.ToString() == KeyCommandHandler.ListBoxItem)
				{
					if(keysPressed.Equals(Shortcuts.PreviousHeader) || keysPressed.Equals(Shortcuts.NextHeader) || keysPressed.Equals(Shortcuts.PreviousSection) ||
						keysPressed.Equals(Shortcuts.NextSection))
					{
						e.SubViewHandled = true;
					}
				}

				//Contents, Book Information and Bookmarks shortcuts should just switch between those tabs?
				if(keysPressed.Equals(Shortcuts.ContentsPanel))
				{
					if(navTabs.SelectedIndex != (int)NavigationState.Focus.Contents)
					{
						navTabs.SelectedIndex = (int)NavigationState.Focus.Contents;
						navTabs.Focus();
						e.SubViewHandled = true;
					}
				}
				if(keysPressed.Equals(Shortcuts.BookInformationPanel))
				{
					if(navTabs.SelectedIndex != (int)NavigationState.Focus.BookInformation)
					{
						navTabs.SelectedIndex = (int)NavigationState.Focus.BookInformation;
						navTabs.Focus();
						e.SubViewHandled = true;
					}
				}
				if(keysPressed.Equals(Shortcuts.BookmarkPanel))
				{
					if(navTabs.SelectedIndex != (int)NavigationState.Focus.Bookmarks)
					{
						navTabs.SelectedIndex = (int)NavigationState.Focus.Bookmarks;
						navTabs.Focus();
						e.SubViewHandled = true;
					}
				}
			}
		}



		/// <summary>
		/// Raised when the selected bookmark is changed.
		/// </summary>
		/// <remarks>INCOMPLETE</remarks>
		private void BookmarksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int SelectedBookmarkID = ((sender as ListBox).SelectedIndex);
		}



		/// <summary>
		/// Raised by the UI when the selected/current navigation tab changes. Causes the list item 
		/// contents to be updated to show the correct colours.
		/// </summary>
		private void CurrentNavTabChanged(object sender, SelectionChangedEventArgs e)
		{
			if(navTabs != null)	//Need to focus the parent listbox
				navTabs.Focus();

			UpdateNavTabItems();
			if(contentsSectionContainer != null && contentsSectionContainer.SelectedIndex == _contentsTabIndex)
				RefreshContentsScrollPosition();
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



		/// <summary>
		/// Navigates to the heading at the given index.
		/// TODO: The Presenter should receive the index and perform the lookup of the ID (possibly with the Model's help).
		/// </summary>
		/// <param name="headingIndex">The index of the heading being selected.</param>
		private void NavigateToHeadingID(int headingIndex)
		{
			if(_tableOfContents.FlatHeadingList != null && headingIndex != -1 && _resetCurrentPositionToHeading)
			{
				ItemSelectedEventArgs itemArgs = new ItemSelectedEventArgs(_tableOfContents.FlatHeadingList.ElementAt(headingIndex).ID);
				ItemSelected(new object(), itemArgs);
			}
			_resetCurrentPositionToHeading = true;
		}


        /// <summary>
        /// Capture KeyDown events, needed in each subview as the Scrollviewer control
        /// that the panels are situated in will not let certain keypresses through
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		void Navigation_KeyDown(object sender, KeyEventArgs e)
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
		void Navigation_KeyUp(object sender, KeyEventArgs e)
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



		/// <summary>
		/// Raised when the control is loaded, so that the 'tabs' are automatically focussed and the
		/// self-voicing is performed.
		/// </summary>
		void Navigation_Loaded(object sender, RoutedEventArgs e)
		{
			navTabs.Focus();

			//Select the right tab depending on the set focus
			if(FocusedItem == (int)NavigationState.Focus.BookInformation)
			{
				navTabs.SelectedIndex = (int)NavigationState.Focus.BookInformation;
			}
			if(FocusedItem == (int)NavigationState.Focus.Contents)
			{
				navTabs.SelectedIndex = (int)NavigationState.Focus.Contents;
			}
			if(FocusedItem == (int)NavigationState.Focus.Bookmarks)
			{
				navTabs.SelectedIndex = (int)NavigationState.Focus.Bookmarks;
			}

			ApplicationVoice.MainInstance.QueueTextToSpeak(Speech.NavigationPanelMessage);
		}



		/// <summary>
		/// Updates the displayed information from the current book data.
		/// This is invoked automatically when the CurrentBook reference is changed.
		/// TODO: This should be a public interface method called by the respective presenter when the underlying authoritative data changes.
		/// </summary>
		private void PopulateBookInformation()
		{
			string nameForScreenReaders = "";

			if(_currentBook == null)
			{
				return;
			}

			//display the stored information in book
			//TODO Someday, this will be a list or a hashtable or something, with a loop to set out a stackpanel/list layout
			BookInfo.BookInfoTitle.Text = "Title: " + (_currentBook.Title != null? _currentBook.Title : "(not available)");
			nameForScreenReaders += BookInfo.BookInfoTitle.Text + ". ";
			BookInfo.BookInfoCreator.Text = "Creator: " + (_currentBook.Creator != null? _currentBook.Creator : "(not available)");
			nameForScreenReaders += BookInfo.BookInfoCreator.Text + ". ";
			BookInfo.BookInfoPublisher.Text = "Publisher: " + (_currentBook.Publisher != null? _currentBook.Publisher : "(not available)");
			nameForScreenReaders += BookInfo.BookInfoPublisher.Text + ". ";
			BookInfo.BookInfoLanguage.Text = "Language: " + (_currentBook.Language != null? _currentBook.Language : "(not available)");
			nameForScreenReaders += BookInfo.BookInfoLanguage.Text + ". ";
			BookInfo.BookInfoDate.Text = "Date: " + (_currentBook.Date != null? _currentBook.Date : "(not available)");
			nameForScreenReaders += BookInfo.BookInfoDate.Text + ". ";
			BookInfo.BookInfoPages.Text = "Number of Pages: " + (_currentBook.PageList.Pages != null? _currentBook.PageList.Pages.Count + "" : "(not available)");
			nameForScreenReaders += BookInfo.BookInfoPages.Text + ". ";
			BookInfo.BookInfoHeadingLevels.Text = "Levels of Heading: " + _currentBook.HeadingLevels;
			nameForScreenReaders += BookInfo.BookInfoHeadingLevels.Text + ". ";
			BookInfo.BookInfoMultimediaType.Text = "Media Type: " + (_currentBook.MultimediaType != null? _currentBook.MultimediaType : "(not available)");
			nameForScreenReaders += BookInfo.BookInfoMultimediaType.Text + ". ";
			BookInfo.BookInfoMultimediaContent.Text = "Media Content: " + (_currentBook.MultimediaContent != null? _currentBook.MultimediaContent : "(not available)");
			nameForScreenReaders += BookInfo.BookInfoMultimediaContent.Text + ". ";
			BookInfo.BookInfoTotalTime.Text = "Total Time: " + (_currentBook.TotalTime != null? _currentBook.TotalTime : "(not available)");
			nameForScreenReaders += BookInfo.BookInfoTotalTime.Text + ". ";

			AutomationProperties.SetName(BookInfo, nameForScreenReaders);
			if(IndicateSearchResultsChanged != null)
			{
				IndicateSearchResultsChanged(this, new EventArgs());
			}
		}



		/// <summary>
		/// Refreshes the items displayed in the Table of Contents. Called when the TOC reference is (re)set.
		/// </summary>
		private void PopulateTableOfContents()
		{
			// empty out the table of contents listbox and render new table of contents
			TableOfContentsListBox.SelectedItem = null;
			TableOfContentsListBox.Items.Clear();

			foreach(Heading heading in _tableOfContents.FlatHeadingList)
			{
				ExtendedListBoxItem newItem = new ExtendedListBoxItem();
				if(Application.Current.Resources.Contains(Strings.ListItemStyleKey))
				{
					newItem.Style = Application.Current.Resources[Strings.ListItemStyleKey] as Style;
				}
				newItem.SetBinding(ExtendedListBoxItem.SelectionBackgroundProperty, _selectionBackgroundBinding);
				newItem.SetBinding(ForegroundProperty, _defaultForegroundBinding);
				newItem.Content = heading.Text;
				// margin for header, set as multiple of level, currently fixed indentation size
				newItem.Margin = new Thickness(heading.level * 15, 0, 0, 0);
				newItem.Padding = new Thickness(0, 0, 0, 0);
				newItem.Height = 20;

				//Set the automation name for each list box item
				AutomationProperties.SetAutomationId(newItem, heading.Text);
				AutomationProperties.SetHelpText(newItem, heading.Text + ". Use the up and down arrow keys to scroll through the other headings in this book.");
				//newItem.GotFocus += SpeakControlOnFocus;

				TableOfContentsListBox.Items.Add(newItem);
			}
			if(IndicateSearchResultsChanged != null)
			{
				IndicateSearchResultsChanged(this, new EventArgs());
			}

			//Reset to first heading.
			SelectedElement = 0;
		}



		/// <summary>
		/// Handles the GotFocus event of a button. This is used to initiate speaking the voice command.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e"></param>
		private void SpeakControlOnFocus(object sender, RoutedEventArgs e)
		{
			if(SpeakableElementSelected != null)
			{
				SpeakableElementSelected(this, new ElementSelectedEventArgs(sender as DependencyObject));
			}

			//If this is the Book Information Tab, begin reading the Book Information
			if((e.OriginalSource as FrameworkElement).Tag as string == "2")
			{
				String BookInfoName = AutomationProperties.GetName(e.OriginalSource as DependencyObject);
				//This all occurs _before_ the GotFocus event on the navtabs...
				//Read the book information contents
				if(BookInfo.BookInfoTitle != null)
				{
					ApplicationVoice.MainInstance.QueueTextToSpeak(BookInfoName + ".");
					ApplicationVoice.MainInstance.QueueTextToSpeak(BookInfo.BookInfoTitle.Text);
					ApplicationVoice.MainInstance.QueueTextToSpeak(BookInfo.BookInfoCreator.Text);
					ApplicationVoice.MainInstance.QueueTextToSpeak(BookInfo.BookInfoPublisher.Text);
					ApplicationVoice.MainInstance.QueueTextToSpeak(BookInfo.BookInfoLanguage.Text);
					ApplicationVoice.MainInstance.QueueTextToSpeak(BookInfo.BookInfoDate.Text);
					ApplicationVoice.MainInstance.QueueTextToSpeak(BookInfo.BookInfoPages.Text);
					ApplicationVoice.MainInstance.QueueTextToSpeak(BookInfo.BookInfoHeadingLevels.Text);
					ApplicationVoice.MainInstance.QueueTextToSpeak(BookInfo.BookInfoMultimediaType.Text);
					ApplicationVoice.MainInstance.QueueTextToSpeak(BookInfo.BookInfoMultimediaContent.Text);
					SelfVoicingSpeakText(sender, new SpeechTextEventArgs(BookInfo.BookInfoTotalTime.Text));
				}
			}
		}



		/// <summary>
		/// Navigate the book to the given heading index of the table of contents when a TOC heading
		/// is selected.
		/// </summary>
		/// <remarks>NOTE: Currently there is no unwiring of this event when the TOC is updated,
		///  so it may be triggered when the list is repopulated.
		/// </remarks>
		private void TableOfContentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			ListBox targetListBox = sender as ListBox;

			if(targetListBox != null) //Need to focus the parent listbox
				targetListBox.Focus();

			// When the listbox is first being populated the selected item could be null.
			if(targetListBox.SelectedItem != null)
			{
				//Remove highlightin from the previously sleected item.
				if(_selectedContentsItem != null)
				{
					_selectedContentsItem.SetBinding(ForegroundProperty, _defaultForegroundBinding);
				}

				//Highlight the current item & save reference
				ExtendedListBoxItem currentItem = targetListBox.SelectedItem as ExtendedListBoxItem;

				currentItem.SetBinding(ForegroundProperty, _selectionForegroundBinding);
				//Ensure the element is scrolled into view.
				if(ParentViewer != null)
					currentItem.ScrollIntoView(ParentViewer, DisplayAttributes.CommonInstance.VisualScale);

				_selectedContentsItem = currentItem;

				// set the selected item args to the toc id and raise event
				int SelectedHeadingID = (targetListBox.SelectedIndex);
				NavigateToHeadingID(SelectedHeadingID);
			}
		}



		/// <summary>
		/// Updates the navigation 'tab' items ListBox to reflect changes in contrast settings or when the
		/// selected item changes.
		/// </summary>
		private void UpdateNavTabItems()
		{
			if(navTabs == null)
			{
				return;
			}
			if(navTabs.SelectedItem == null)
			{
				return;
			}

			ListBoxItem selectedItem = navTabs.SelectedItem as ListBoxItem;
			FocusedItem = navTabs.SelectedIndex;

			string tabIndex = selectedItem.Tag.ToString();
			contentsSectionContainer.SelectedIndex = Int32.Parse(tabIndex);
			if(IndicateSearchResultsChanged != null)
			{
				IndicateSearchResultsChanged(this, new EventArgs());
			}
		}

		#endregion Methods 
	}
}
