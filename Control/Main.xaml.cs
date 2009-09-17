using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Buttercup.Control.Common;
using Buttercup.Control.Common.IO;
using Buttercup.Control.Common.Net;
using Buttercup.Control.Model;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.Resources;
using Buttercup.Control.UI;
using Buttercup.Control.UI.Extensions;
using Buttercup.Control.UI.ViewModel;
using Control.MVP;
using Control.MVP.Presenters;
using Control.MVP.Views;
using Control.UI;
using Control.UI.Utility;


namespace Buttercup.Control
{
	/// <summary>
	/// The main application user control.
	/// </summary>
	public partial class Main : UserControl, IApplicationView
	{
		#region Fields (7) 

		private readonly DisplayAttributes _displayAttributes;
		private BackgroundWorker _fileOpenBackgroundWorker;
		//These constants are language-independent
		private const string _hostElementID = "buttercupHost";
		private const string _objectElementID = "buttercup";
		private ContentControl _restoreFocusTarget;
		private string _serverBookReference;
		private Surface _surface;

		#endregion Fields 



		#region Constructors (1) 

		/// <summary>
		/// Default constructor
		/// </summary>
		public Main()
		{
			// Initialise the controls of the control
			InitializeComponent();

			_displayAttributes = new DisplayAttributes();
			DisplayAttributes.CommonInstance = _displayAttributes;
			DataContext = _displayAttributes;

			// Initialise a new XamlSurface
			DisplaySurface = new XamlSurface(surfaceContainer);
			DisplaySurface.RenderComplete += DisplaySurface_RenderComplete;
			DisplaySurface.PreImageRendered += DisplaySurface_PreImageRendered;
            DisplaySurface.DrawFailed += new EventHandler(DisplaySurface_DrawFailed);

			sidePanelContents.LayoutUpdated += sidePanelContents_LayoutUpdated;

			//Establish connection between resize UI popup and display settings view.
			rescaleUIPopupContents.ClosePopup += HideRescalePopup;
			displaySettings.MainViewReference = this;

			navigation.ParentViewer = sidePanelContents;
			navigation.RefreshContrast();

			HtmlPage.Window.AttachEvent("DOMMouseScroll", Main_MouseWheelTurned);
			HtmlPage.Window.AttachEvent("onmousewheel", Main_MouseWheelTurned);
			HtmlPage.Document.AttachEvent("onmousewheel", Main_MouseWheelTurned);

            

			//// Detect Isolated Storage Quota, give user a button to increase storage if quota is < 900megs
			//var store = IsolatedStorageFile.GetUserStoreForApplication();

			//if(store.Quota < 900000000)
			//{
			//    increaseStorage.Visibility = Visibility.Visible;
			//}
			//else
			//{
			//    increaseStorage.Visibility = Visibility.Collapsed;
			//}

			// Wire up the Loaded event handler to set up the View
			Loaded += Main_Loaded;

			// Associate this View and its Presenter
			Presenter = new ApplicationPresenter(this);

			Presenter.BookChanged += Presenter_BookChanged;

			KeyDown += Main_KeyDown;
			KeyUp += Main_KeyUp;
			hidePanelButton.KeyDown += Main_KeyDown;
			hidePanelButton.KeyUp += Main_KeyUp;

			KeyCommandHandler = new KeyCommandHandler();

			//Setup the AutomationProperties.Name on the Help Shortcut Key Tab
			//TODO Needs to be changed to the entire thing.

			string helpContents = help.ContentsAsString();
			AutomationProperties.SetName(help, helpContents);

			Application.Current.Host.Content.Resized += Content_Resized;

			// Set the key command handlers for all co/sub-views
			// Order for attaching keycommandhandlers matters, views which should handle keys first
			// should be attached first.
			rescaleUIPopupContents.KeyCommandHandler = KeyCommandHandler;
			search.KeyCommandHandler = KeyCommandHandler;
			displaySettings.KeyCommandHandler = KeyCommandHandler;
			navigation.KeyCommandHandler = KeyCommandHandler;
			help.KeyCommandHandler = KeyCommandHandler;
			mainPlayer.KeyCommandHandler = KeyCommandHandler;

			KeyCommandHandler.KeyDown += KeyCommandHandler_KeyIsPressed;

			// Check the QueryString for an OPF reference if it's there
			// Then we need to open that book
			if(ServerBookReference != null)
			{
				surfaceContainer.Content = null; //Don't show initial content in Server book mode

				if(LoadPackage != null)
				{
					LoadPackage(this, new LoadPackageEventArgs(ServerBookReference));
				}
			}
			else
			{
				((XamlSurface)DisplaySurface).ArrangeInitialContent();
			}
		}

		#endregion Constructors 



		#region Properties (16) 

		/// <summary>
		/// A reference to the BookReader - I'm imagining this to be the central class the
		/// knows how to coordinate playing a book. It will have a reference to the book and
		/// a surface and contain the methods to play and navigate the book.
		/// </summary>
		public Reader BookReader
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Set the localisation culture for this view
		/// </summary>
		public CultureInfo Culture { get; set; }

		/// <summary>
		/// Gets the display surface.
		/// </summary>
		/// <value>The display surface where the book fragment is rendered.</value>
		public Surface DisplaySurface
		{
			get { return _surface; }
			private set { _surface = value; }
		}

		/// <summary>
		/// Returns the HtmlElement that is the parent of the
		/// Silverlight oject.
		/// </summary>
		private HtmlElement HostElement
		{
			get { return HtmlPage.Document.GetElementById(_hostElementID); }
		}

		IDisplaySettingsView IApplicationView.DisplaySettingsView
		{
			get { return displaySettings; }
		}

		ISearchView IApplicationView.SearchView
		{
			get { return search; }
		}

		public KeyCommandHandler KeyCommandHandler { get; set; }

		public INavigationView NavigationView
		{
			get { return navigation; }
		}

		/// <summary>
		/// Returns the HtmlElement for the Silverlight oject.
		/// </summary>
		private HtmlElement ObjectElement
		{
			get { return HtmlPage.Document.GetElementById(_objectElementID); }
		}

		public IPlayerView PlayerView
		{
			get { return mainPlayer; }
		}

		/// <summary>
		/// Presenter for this view
		/// </summary>
		private ApplicationPresenter Presenter { get; set; }

		internal RescaleUIPopupContents RescaleUIPopupContents
		{
			get { return rescaleUIPopupContents; }
		}

		/// <summary>
		/// Gets/Sets the Progress Bar's value.
		/// </summary>
		public double SaveProgress
		{
			get { return progressBar.Value; }
			set
			{
				if(value > 0)
				{
					progressBar.Visibility = Visibility.Visible;
					progressText.Visibility = Visibility.Visible;

					progressBar.Value = value;
				}
				else
				{
					// Setting the ProgressBar's value to less than 0 or greater than 100
					// will cause it to disappear.
					progressBar.Visibility = Visibility.Collapsed;
					progressText.Visibility = Visibility.Collapsed;
				}
			}
		}

		/// <summary>
		/// Get/Set the message to play next to the ProgressBar.
		/// </summary>
		public string SaveProgressMessage
		{
			get { return progressText.Text; }
			set { progressText.Text = value; }
		}

		/// <summary>
		/// Gets the reference to the server book (if it has been supplied by the QueryString
		/// </summary>
		public string ServerBookReference
		{
			get
			{
				if(_serverBookReference == null)
				{
					IDictionary<string, string> queryString = HtmlPage.Document.QueryString;
					if(queryString.ContainsKey("ref"))
					{
						_serverBookReference = queryString["ref"];

                        // This will throw an exception if the Uri is not wellformed
                        Uri uri = new Uri(_serverBookReference, UriKind.RelativeOrAbsolute);
                        Uri uri1 = App.Current.Host.Source;
                        //if (!uri.IsAbsoluteUri)
                        //{
                        //    // We are assuming the link is relative to the main website
                        //    Uri hostSite = HtmlPage.Document.DocumentUri;
                        //    Uri baseUri = new Uri(hostSite.Scheme + "://" + hostSite.Host);
                            
                        //    uri = new Uri(baseUri, uri.
                        //}

					}
				}
				return _serverBookReference;
			}
			set { throw new NotImplementedException(); }
		}

		/// <summary>
		/// The cursor mouse cursor shown for the application
		/// (i.e. hourglass when waiting on loading...)
		/// </summary>
		public bool WaitingOnProgress
		{
			set
			{
				// As this method can be called from another thread (and updates the UI) we need to make 
				// sure it executes on the main (UI) thread.
				Dispatcher.BeginInvoke(delegate
				                       {
				                       	if(value)
				                       	{
				                       		Cursor = Cursors.Wait;
				                       	}
				                       	else
				                       	{
				                       		Cursor = Cursors.Arrow;
				                       	}
				                       });
			}
		}

		#endregion Properties 



		#region Delegates and Events (13) 

		// Events (13) 

		/// <summary>
		/// Event is raised once the CurrentBook has changed - for example as a result of
		/// a zip packaged being loaded.
		/// </summary>
		public event EventHandler BookChanged;

		public event EventHandler BookDisplayed;

		public event EventHandler BookLoadFailed;

		public event EventHandler BookLoadStarted;

		public event EventHandler<PanelFocusedItemEventArgs> DisplaySettingsFocusChanged;

		public event EventHandler<LoadPackageEventArgs> LoadPackage;

		/// <summary>
		/// Event is raised when a book is ready to be loaded
		/// </summary>
		public event EventHandler<LoadZipPackageEventArgs> LoadZipPackage;

		public event EventHandler<PanelFocusedItemEventArgs> NavigationViewFocusChanged;

		public event EventHandler<PanelFocusedItemEventArgs> SearchViewFocusChanged;

		/// <summary>
		/// Raised when a particular panel is selected, or when the panel is hidden.
		/// </summary>
		/// <remarks>A display mode of Hidden will be specified when hiding the panel.</remarks>
		public event EventHandler<PanelDisplayModeEventArgs> SelectPanel;

		public event EventHandler<SpeechTextEventArgs> SelfVoicingSpeakText;

		/// <summary>
		/// Raised when a UI element is required to be spoken.
		/// </summary>
		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelected;

		/// <summary>
		/// Raised when a UI element's help text is required to be spoken.
		/// </summary>
		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelectedHelpText;

		#endregion Delegates and Events 



		#region Methods (43) 

		// Public Methods (9) 

		/// <summary>
		/// Applies the contrast setting to the entire interface.
		/// </summary>
		/// <param name="contrastScheme">The contrast scheme.</param>
		public void ApplyContrastSetting(ContrastLevel contrastScheme)
		{
			_displayAttributes.HighlightBackground = DisplayAttributes.YellowBrush;
			_displayAttributes.HighlightForeground = DisplayAttributes.BlackBrush;

			switch(contrastScheme)
			{
				case ContrastLevel.WhiteTextOnBlue:
				{
					_displayAttributes.DefaultBackground = DisplayAttributes.DarkBlueBrush;
					_displayAttributes.DefaultForeground = DisplayAttributes.LightWhiteBrush;
					_displayAttributes.ControlSectionBackground = DisplayAttributes.DarkBlueBrush;
					_displayAttributes.SelectionBackground = DisplayAttributes.YellowBrush;
					_displayAttributes.SelectionBorderBrush = DisplayAttributes.YellowBrush;
					_displayAttributes.SelectionForeground = DisplayAttributes.BlackBrush;
					break;
				}
				case ContrastLevel.BlackTextOnWhite:
				{
					_displayAttributes.DefaultBackground = DisplayAttributes.LightWhiteBrush;
					_displayAttributes.DefaultForeground = DisplayAttributes.BlackBrush;
					_displayAttributes.ControlSectionBackground = DisplayAttributes.LightWhiteBrush;
					_displayAttributes.SelectionBackground = DisplayAttributes.BlackBrush;
					_displayAttributes.SelectionBorderBrush = DisplayAttributes.YellowBrush;
					_displayAttributes.SelectionForeground = DisplayAttributes.YellowBrush;
					break;
				}
				case ContrastLevel.YellowTextOnBlack:
				{
					_displayAttributes.DefaultBackground = DisplayAttributes.BlackBrush;
					_displayAttributes.DefaultForeground = DisplayAttributes.YellowBrush;
					_displayAttributes.ControlSectionBackground = DisplayAttributes.BlackBrush;
					_displayAttributes.SelectionBackground = DisplayAttributes.YellowBrush;
					_displayAttributes.SelectionBorderBrush = DisplayAttributes.LightWhiteBrush;
					_displayAttributes.SelectionForeground = DisplayAttributes.BlackBrush;
					break;
				}
			}

			DisplaySurface.SetContrast(contrastScheme);
			navigation.RefreshContrast();
		}



		/// <summary>
		/// Applies the interface size to the entire interface.
		/// </summary>
		/// <param name="interfaceSize">The interface size.</param>
		public void ApplyInterfaceSize(int interfaceSize)
		{
			double scale = interfaceSize / 8.0;
			if(scale >= 1)
			{
				_displayAttributes.VisualScale = scale;
				DisplaySurface.SetScale(scale);

				//Don't scale the layout of the controls as much as the text.
				controlsContainer.ScaleMultiplier = (scale - 1.0) * 0.75 + 1.0;

				sidePanelContents.AdjustContentScale(scale);
			}
		}



		/// <summary>
		/// Gets the display settings view. This is required by the Presenter to store a reference to the
		/// view and be able to access its functionality.
		/// </summary>
		/// <returns></returns>
		public IDisplaySettingsView DisplaySettingsView()
		{
			return displaySettings;
		}



		/// <summary>
		/// Hides the side panel.
		/// </summary>
		/// <remarks>If the panel is already hidden, it will stay hidden.</remarks>
		public void HideSidePanel()
		{
			bool isHidden = (sidePanelHost.Visibility == Visibility.Collapsed);

			if(isHidden)
			{
				return; //No need to do any resizing.
			}

			CollapseSidePanelVisuals();
			//Need to refresh surface's scale so it fits properly in ScrollViewer
			DisplaySurface.SetScale(_displayAttributes.VisualScale);

			if(_restoreFocusTarget != null)
			{
				_restoreFocusTarget.Focus();
			}
		}



		/// <summary>
		/// Responds to notification that the panel contents have changed.
		/// Refreshes the side panel scale to ensure panel contents are scrolled correctly.
		/// </summary>
		public void NotifyPanelContentsChanged()
		{
			sidePanelContents.AdjustContentScale(_displayAttributes.VisualScale);
		}



		/// <summary>
		/// Gets the search component view. This is required by the Presenter to store a reference to the
		/// view and be able to access its functionality.
		/// </summary>
		/// <returns>The appropriate search view reference to use.</returns>
		public ISearchView SearchView()
		{
			return search;
		}



		/// <summary>
		/// Sets the state of the control buttons (i.e. Open Book).
		/// </summary>
		/// <param name="isEnabled">If true, enables the control buttons that relate to the current book.
		/// This does not affect buttons relating to the application itself, e.g. contrast.</param>
		public void SetControlButtonState(bool isEnabled)
		{
			// As this method can be called from another thread (and updates the UI) we need to make 
			// sure it executes on the main (UI) thread.
			Dispatcher.BeginInvoke(delegate { openBookButton.IsEnabled = isEnabled; });
		}



		/// <summary>
		/// Shows the side panel corresponding to the given display mode.
		/// </summary>
		/// <param name="displayMode">The display mode.</param>
		/// <remarks>If the display mode is Hidden, then the side panel will (obviously) be hidden.</remarks>
		public void ToggleSidePanel(SidePanelDisplayMode displayMode)
		{
			TabItem selectedTab = null;

			switch(displayMode)
			{
				case SidePanelDisplayMode.DisplaySettings:
				{
					selectedTab = displaySettingsPanel;
					_restoreFocusTarget = showDisplaySettingsButton;
					break;
				}
				case SidePanelDisplayMode.Navigation:
				{
					selectedTab = navigationPanel;
					_restoreFocusTarget = showNavPanelButton;
					break;
				}
				case SidePanelDisplayMode.Search:
				{
					selectedTab = searchPanel;
					_restoreFocusTarget = showSearchPanelButton;
					break;
				}
				case SidePanelDisplayMode.Help:
				{
					selectedTab = helpPanel;
					_restoreFocusTarget = showHelpButton;
					break;
				}
			}

			if(selectedTab != null)
			{
				sidePanelContainer.SelectedItem = selectedTab;
				ShowSidePanel();
				//Need to refresh surface's scale so it fits properly in ScrollViewer
				DisplaySurface.SetScale(_displayAttributes.VisualScale);	

				if(displayMode == SidePanelDisplayMode.Navigation)
				{
					navigation.RefreshContentsScrollPosition();
				}
			}
			else
			{
				//Switch the tabpanel to an empty panel before hiding
				//This unloads the controls meaning we don't need two sets of code to set focus
				//as controls will always be loaded and call Loaded events

				NoFocus.Focus(); //Stops some of the buggy speech reading and ensures a consistent state
				SelfVoicingSpeakText(this, new SpeechTextEventArgs(Speech.PanelClosedMessage));
				HideSidePanel();
			}

			DisplaySurface.SetFocus();
		}


        void DisplaySurface_DrawFailed(object sender, EventArgs e)
        {
            // For now just make sure the cursor returns to normal
            WaitingOnProgress = false;
        }



		// Private Methods (33) 

		/// <summary>
		/// This event handler initiates the work to be done by the File Open thread.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _fileOpenBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			// Subscribe to any progress changes that the Presenter raises
			Presenter.ProgressChanged += Presenter_ProgressChanged;

			// Let listeners (most likely dependent view) know that a book load process is about
			// to start and to refect this in their UI's - for example the PlayerView will disable its
			// controls.
			if(BookLoadStarted != null)
			{
				BookLoadStarted(this, new EventArgs());
			}

			Dispatcher.BeginInvoke(
				() => { ApplicationVoice.MainInstance.QueueTextToSpeak("Loading book."); });

			// Raise the LoadZipPackage event to let the main Presenter know to do it's thing
			if(LoadZipPackage != null)
			{
				LoadZipPackage(this, new LoadZipPackageEventArgs((FileStream)e.Argument));
			}
		}



		/// <summary>
		/// This event handler is an opportunity to be able to update the main thread's user
		/// interface.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _fileOpenBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			// TODO: Change to flag based on UserState - check against FileOpenState.InProgress/FileExists/Complete
			if(e.ProgressPercentage >= 0)
			{
				SaveProgress = e.ProgressPercentage;
				SaveProgressMessage = e.UserState.ToString();
				if(e.ProgressPercentage % 10 == 0)
				{
					DoSelfVoicingSpeakText(e.ProgressPercentage + " percent");
				}
			}
			else
			{
				// Completed
			}
		}



		/// <summary>
		/// This event handler is called when the File Open process has completed and all the files
		/// are ready to be accessed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _fileOpenBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if(e.Error != null)
			{
				if(BookLoadFailed != null)
				{
					BookLoadFailed(this, new EventArgs());
				}

				DoSelfVoicingSpeakText("The book was not a valid Daisy book. " + e.Error.Message);

				//TODO replace this with an application popup dialog box
				Dispatcher.BeginInvoke(
					() => { MessageBox.Show("The book was not a valid Daisy book.\nError: " + e.Error.Message, "Error loading book", MessageBoxButton.OK); });
			}
		}



		/// <summary>
		/// Default handler raised when a button loses focus. Adjusts the foreground binding.
		/// </summary>
		private void Button_LostFocus(object sender, RoutedEventArgs e)
		{
			if(sender is Button && ((Button)sender).IsMouseOver)
			{
				CommonEventHandlerUtility.HandleMouseOver(sender);
			}
			else
			{
				CommonEventHandlerUtility.HandleMouseOutLoseFocus(sender);
			}
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
		/// Collapses the visual elements of the side panel.
		/// </summary>
		/// <remarks>This should only be called when the panel is already visible.</remarks>
		private void CollapseSidePanelVisuals()
		{
			sidePanelContainer.SelectedItem = emptyPanel;
			sidePanelHost.Visibility = Visibility.Collapsed;
			controlsColumnDefinition.MinWidth = 64.0;
			surfaceContainer.AdjustContentScale(_displayAttributes.VisualScale);
		}



		/// <summary>
		/// Raised when the browser window resizes, so that this control can be resized to fill the window.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e"></param>
		private void Content_Resized(object sender, EventArgs e)
		{
			Width = Application.Current.Host.Content.ActualWidth;
			Height = Application.Current.Host.Content.ActualHeight;
			FrameworkElement popupContents = changeSizePopup.Child as FrameworkElement;
			popupContents.Width = Width;
			popupContents.Height = Height;
		}



		/// <summary>
		/// The PreImageRendered event is raised when an image is being processed before
		/// rendering. It is an opportunity to set it's source programmatically.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DisplaySurface_PreImageRendered(object sender, EventArgs e)
		{
			Image image = sender as Image;
			if(image != null)
			{
				// If the book isn't browseable then we're going to have to set the
				// image source.
				try
				{
					if(!Presenter.GetCurrentBook().IsBrowseable)
					{
						Stream source = Presenter.GetImageStream(((BitmapImage)image.Source).UriSource.ToString());
						(DisplaySurface).SetImageSource(image.Name, source);
					}
					else if(DisplaySurface is XamlSurface)
					{
						image.SizeChanged += ((XamlSurface)DisplaySurface).RefreshLayout;
					}
				}
				catch(Exception ex)
				{
					Logger.Log(ex);
				}
			}
		}



		/// <summary>
		/// Occurs when the surface has completed rendering the contents of the book.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DisplaySurface_RenderComplete(object sender, EventArgs e)
		{
			//Need to refresh surface's scale so it fits properly in ScrollViewer
			DisplaySurface.SetScale(_displayAttributes.VisualScale);

            // TODO: Do we need both these events? They seem the same.
			//       Further - the order of these events being raised is important otherwise
            //       the book doesn't start playing.

            // Notify others that a new book has been loaded.
            if (BookChanged != null)
            {
                BookChanged(this, new EventArgs());
            }

            // Raise an event for the Presenter to respond to.
            if (BookDisplayed != null)
            {
                BookDisplayed(this, new EventArgs());
            }

            // Speak that title of the book
            DoSelfVoicingSpeakText(String.Format(Speech.BookLoaded, Presenter.GetCurrentBook().Title));
		}



		/// <summary>
		/// Raises an event to notify ? to speak a message.
		/// </summary>
		/// <param name="message"></param>
		private void DoSelfVoicingSpeakText(string message)
		{
			if(SelfVoicingSpeakText != null)
			{
				SelfVoicingSpeakText(this, new SpeechTextEventArgs(message));
			}
		}



		private void file_OpenAsyncComplete(object sender, DownloadCompleteEventArgs e)
		{
			// e.UserState should store the url of the requested resource. This can then be used
			// to set the appropriate source
			BitmapImage tempImage = new BitmapImage();
			tempImage.SetSource(e.Result);
		}



		private void hidePanelButton_Click(object sender, RoutedEventArgs e)
		{
			if(SelectPanel != null)
			{
				SelectPanel(this, new PanelDisplayModeEventArgs(SidePanelDisplayMode.Hidden));
			}
		}



		private void HideRescalePopup(object sender, EventArgs e)
		{
			changeSizePopup.IsOpen = false;
		}



		//private void increaseStorage_Click(object sender, RoutedEventArgs e)
		//{
		//    try
		//    {
		//        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
		//        {
		//            if(store.IncreaseQuotaTo(900000000)) //900MB
		//            {
		//                increaseStorage.Visibility = Visibility.Collapsed;
		//            }
		//        }
		//    }
		//    catch
		//    {
		//    }
		//}
		/// <summary>
		/// Handle Player Key Commands
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void KeyCommandHandler_KeyIsPressed(object sender, KeyCommandEventArgs e)
		{
			Shortcut keysPressed = new Shortcut(e.keyEventArgs.PlatformKeyCode, e.keyEventArgs.Key, Keyboard.Modifiers);

			//Universal Application Shortcuts

		    if(e.SubViewHandled == false)
			{
				if(keysPressed.Equals(Shortcuts.OpenBookDialog))
				{
					//Call dispatcher to run this code in the main UI thread, queues the code until the main thread is non-blocking,
					//and prevents multiple keydown events from crashing IE by attempting to open multiple dialogs
					if(openBookButton.Visibility == Visibility.Visible)
					{
						Dispatcher.BeginInvoke(
							() => { openBookButton_Click(new object(), new RoutedEventArgs()); });
					}
				}

				//if (keysPressed.Equals(Shortcuts.AddBookmark))
				//{
				//}
				if(keysPressed.Equals(Shortcuts.HelpPanel))
				{
					showHelpSettingsButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.BookInformationPanel))
				{
					//open/close the book information panel
					if(NavigationViewFocusChanged != null)
					{
						NavigationViewFocusChanged(new object(), new PanelFocusedItemEventArgs((int)NavigationState.Focus.BookInformation));
					}
					showNavPanelButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.ContentsPanel))
				{
					//open/close the table of contents panel
					if(NavigationViewFocusChanged != null)
					{
						NavigationViewFocusChanged(new object(), new PanelFocusedItemEventArgs((int)NavigationState.Focus.Contents));
					}
					showNavPanelButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.BookmarkPanel))
				{
					//open/close the bookmark panel
					if(NavigationViewFocusChanged != null)
					{
						NavigationViewFocusChanged(new object(), new PanelFocusedItemEventArgs((int)NavigationState.Focus.Bookmarks));
					}
					showNavPanelButton_Click(new object(), new RoutedEventArgs());
				}

				if(keysPressed.Equals(Shortcuts.DisplaySettingsPanel))
				{
					if(DisplaySettingsFocusChanged != null)
					{
						DisplaySettingsFocusChanged(new object(), new PanelFocusedItemEventArgs((int)DisplaySettingsState.Focus.ChangeSize));
					}
					//open/close the display settings panel
					showDisplaySettingsButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.Contrast))
				{
					if(DisplaySettingsFocusChanged != null)
					{
						DisplaySettingsFocusChanged(new object(), new PanelFocusedItemEventArgs((int)DisplaySettingsState.Focus.Contrast));
					}
					//open/close the display settings panel
					showDisplaySettingsButton_Click(new object(), new RoutedEventArgs());
				}

				if(keysPressed.Equals(Shortcuts.FindPanelPage))
				{
					if(SearchViewFocusChanged != null)
					{
						SearchViewFocusChanged(new object(), new PanelFocusedItemEventArgs(SearchState.FocusGoToPage));
					}
					showSearchPanelButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.FindPanelSearch))
				{
					if(SearchViewFocusChanged != null)
					{
						SearchViewFocusChanged(new object(), new PanelFocusedItemEventArgs(SearchState.FocusFindText));
					}
					showSearchPanelButton_Click(new object(), new RoutedEventArgs());
				}

				if(keysPressed.Equals(Shortcuts.ClosePanel))
				{
					//close currently open panel
					hidePanelButton_Click(new object(), new RoutedEventArgs());
				}
				if(keysPressed.Equals(Shortcuts.SpeakHelpText))
				{
					e.keyEventArgs.Handled = true; //Prevents text boxes from typing this shortcut
					//DependencyObject focusedElement = FocusManager.GetFocusedElement() as DependencyObject;
					SpeakableElementSelectedHelpText(new object(), new ElementSelectedEventArgs(e.keyEventArgs.OriginalSource as DependencyObject));
				}

				if(keysPressed.Equals(Shortcuts.IncreaseInterfaceSize))
				{
					if(rescaleUIPopupContents != null)
					{
						rescaleUIPopupContents.IncreaseSize();
					}
					if(!changeSizePopup.IsOpen)
					{
						ShowRescalePopup();
					}
				}

				if(keysPressed.Equals(Shortcuts.DecreaseInterfaceSize))
				{
					if(rescaleUIPopupContents != null)
					{
						rescaleUIPopupContents.DecreaseSize();
					}
					if(!changeSizePopup.IsOpen)
					{
						ShowRescalePopup();
					}
				}
			}
		}



		private void Main_KeyDown(object sender, KeyEventArgs e)
		{
		    if (e.Handled == false)
                KeyCommandHandler.KeyPressed(new KeyCommandEventArgs(e));
		}



		void Main_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.Handled == false)
			{
				KeyCommandHandler.KeyReleased(new KeyCommandEventArgs(e));
			}
		}



		/// <summary>
		/// User Control Load event handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Main_Loaded(object sender, RoutedEventArgs e)
		{
			//throw new Exception("YIkes");
			//Set focus to the application
			HtmlPage.Plugin.Focus();

			NoFocus.Focus();

			//Default state is to have all panels closed
			//ToggleSidePanel(SidePanelDisplayMode.Hidden);

			CollapseSidePanelVisuals();
		}



		private void Main_MouseWheelTurned(object sender, HtmlEventArgs args)
		{
			double delta = 0;
			ScriptObject e = args.EventObject;

			//browser specific mouse wheel data
			if(e.GetProperty("wheelDelta") != null) // IE and Opera
			{
				delta = ((double)e.GetProperty("wheelDelta"));
				if(HtmlPage.Window.GetProperty("opera") != null)
				{
					delta = -delta;
				}
			}

			else if(e.GetProperty("detail") != null) // Mozilla and Safari
			{
				delta = -((double)e.GetProperty("detail"));
			}

			double scrollOffset = 0.0;

			//scroll up or down
			if(delta > 0)
			{
				scrollOffset = -100.0;
			}
			else if(delta < 0)
			{
				scrollOffset = 100.0;
			}

			//set the scroll viewer to the new offset
			if(scrollOffset != 0.0)
			{
				scrollOffset += surfaceContainer.VerticalOffset;

				if(scrollOffset < 0.0)
				{
					scrollOffset = 0.0;
				}
				else if(scrollOffset > surfaceContainer.ScrollableHeight)
				{
					scrollOffset = surfaceContainer.ScrollableHeight;
				}

				surfaceContainer.ScrollToVerticalOffset(scrollOffset);
			}

			//prevent the browser from doing what it normally does on mousewheel
			if(delta != 0)
			{
				args.PreventDefault();
				e.SetProperty("returnValue", false);
			}
		}



		/// <summary>
		/// This method is called when the Presenter has completed opening a new book and it
		/// is ready to be rendered.
		/// </summary>
		private void OnBookCreated()
		{
			// Effectively hides progress controls
			SaveProgress = -1;
			SaveProgressMessage = "";
			Book book = Presenter.GetCurrentBook();

			if(book != null)
			{
                DisplaySurface.BookFragment = book.BookXml;
                DisplaySurface.Draw();

                //// Notify others that a new book has been loaded.
                //if(BookChanged != null)
                //{
                //    if(book != null)
                //    {
                //        Logger.Log("Book Changed: " + book.Title);
                //    }
                //    BookChanged(this, new EventArgs());
                //}

                //DoSelfVoicingSpeakText(String.Format(Speech.BookLoaded, book.Title));
			}
		}



		/// <summary>
		/// Response to the user selecting to Open a book
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void openBookButton_Click(object sender, RoutedEventArgs e)
		{
			NoFocus.Focus(); //set focus away from other controls

			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = Strings.FileOpenDialogFilter;
			if(dlg.ShowDialog().Value)
			{
				// Change cursor to an hourglass. Currently, will only be an hourglass when over the main control.
				WaitingOnProgress = true;
				NoFocus.Focus(); //set focus away from other controls

				// Set up a BackGround thread to open the file. This action can take a long time
				// if this is the first time the book is opened as it will be unzipped to Isolated
				// Storage. If also allows us to update the user interface as the open process 
				// progreses.
				_fileOpenBackgroundWorker = new BackgroundWorker();
				_fileOpenBackgroundWorker.ProgressChanged += _fileOpenBackgroundWorker_ProgressChanged;
				_fileOpenBackgroundWorker.DoWork += _fileOpenBackgroundWorker_DoWork;
				_fileOpenBackgroundWorker.WorkerReportsProgress = true;
				_fileOpenBackgroundWorker.WorkerSupportsCancellation = true;
				_fileOpenBackgroundWorker.RunWorkerCompleted += _fileOpenBackgroundWorker_RunWorkerCompleted;
				_fileOpenBackgroundWorker.RunWorkerAsync(dlg.File.OpenRead());
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Presenter_BookChanged(object sender, EventArgs e)
		{
			//Run the OnBookCreated() method on the main UI thread, so that UI element are updated
			//from the main UI. Avoids cross-thread access exceptions.
			mainPlayer.Dispatcher.BeginInvoke(delegate() { OnBookCreated(); });
		}



		/// <summary>
		/// The Presenter will notify the View of progress during the File Open process.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Presenter_ProgressChanged(object sender, ProgressNotificationEventArgs e)
		{
			// Let the worker process know that there has been some progress. This will result
			// in the ProgressChanged event from being raised.
			_fileOpenBackgroundWorker.ReportProgress(Convert.ToInt32(e.PercentageProgress), e.ProgressMessage);
		}



		private void showDisplaySettingsButton_Click(object sender, RoutedEventArgs e)
		{
			if(SelectPanel != null)
			{
				SelectPanel(this, new PanelDisplayModeEventArgs(SidePanelDisplayMode.DisplaySettings));
			}
		}



		private void showHelpSettingsButton_Click(object sender, RoutedEventArgs e)
		{
			if(SelectPanel != null)
			{
				SelectPanel(this, new PanelDisplayModeEventArgs(SidePanelDisplayMode.Help));
			}
		}



		private void showNavPanelButton_Click(object sender, RoutedEventArgs e)
		{
			if(SelectPanel != null)
			{
				SelectPanel(this, new PanelDisplayModeEventArgs(SidePanelDisplayMode.Navigation));
			}
		}



		private void showSearchPanelButton_Click(object sender, RoutedEventArgs e)
		{
			if(SelectPanel != null)
			{
				SelectPanel(this, new PanelDisplayModeEventArgs(SidePanelDisplayMode.Search));
			}
		}



		private void ShowSidePanel()
		{
			bool isHidden = (sidePanelHost.Visibility == Visibility.Collapsed);

			if(!isHidden)
			{
				return; //No need to do any resizing.
			}

			sidePanelHost.Visibility = Visibility.Visible;
			surfaceContainer.AdjustContentScale(_displayAttributes.VisualScale);
			controlsColumnDefinition.MinWidth = 250.0;
		}



		private void sidePanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			FrameworkElement content = sidePanelContents.Content as FrameworkElement;
			if(content == null)
			{
				return;
			}

			content.Width = (sidePanelContents.ViewportWidth - 1.0) / _displayAttributes.VisualScale;

			if(sidePanelContainer.SelectedItem == navigationPanel)
			{
				navigation.RefreshContentsScrollPosition();
			}
		}



		private void sidePanelContents_LayoutUpdated(object sender, EventArgs e)
		{
			//sidePanelContents.LayoutUpdated -= sidePanelContents_LayoutUpdated;

			FrameworkElement content = sidePanelContents.Content as FrameworkElement;
			if(content == null)
			{
				return;
			}

			content.Width = sidePanelContents.ViewportWidth / _displayAttributes.VisualScale;
		}



		private void sidePanelContents_Loaded(object sender, RoutedEventArgs e)
		{
			sidePanelContents.AdjustContentScale(_displayAttributes.VisualScale);
		}



		private void sidePanelContents_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if(sidePanelContainer.SelectedItem == navigationPanel)
			{
				navigation.RefreshContentsScrollPosition();
			}
		}



		/// <summary>
		/// Handles the GotFocus event of a button. This is used to initiate speaking the voice command.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e"></param>
		private void SpeakControlOnFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.HandleGetFocus(sender);

			if(SpeakableElementSelected != null)
			{
				SpeakableElementSelected(this, new ElementSelectedEventArgs(sender as DependencyObject));
			}
		}



		// Internal Methods (1) 

		internal void ShowRescalePopup()
		{
			changeSizePopup.IsOpen = true;
			rescaleUIPopupContents.Focus();
			ApplicationVoice.MainInstance.QueueTextToSpeak("Change Size Dialog");
		}

		#endregion Methods 
	}
}
