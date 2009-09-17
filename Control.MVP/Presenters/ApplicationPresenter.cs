using System;
using System.IO;
using Buttercup.Control.Common;
using Buttercup.Control.Common.IO;
using Buttercup.Control.Common.Net;
using Buttercup.Control.Model;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.Model.Exceptions;
using Control.MVP.Views;
using System.Collections.Generic;


namespace Control.MVP.Presenters
{
	/// <summary>
	/// The ApplicationPresenter is responsible for all the business logic required to populate and
	/// respond to View events. It is instantiated
	/// </summary>
	public class ApplicationPresenter : Presenter, IObservableProgress
	{
		#region Constructors (1) 

		/// <summary>
		/// <summary>
		/// This is the main constructor used by the application. It takes all the application views and
		/// initialises the dependencies between them. Many of the MVP triads are dependent on events
		/// and properties of other Views. The approach we have taken here is treating the triads as
		/// essentially being sub-triads of the main ApplicationView. While this breaks somewhat from the
		/// MVP pattern it simplifies inter-triad communication while still preserving the ability to
		/// test them.
		/// </summary>
		/// <param name="view"></param>
		public ApplicationPresenter(IApplicationView view)
		{
			// Hook in to the View's events.
			View = view;
			View.SelectPanel += View_SelectPanel;
			View.LoadZipPackage += View_LoadZipPackage;
			View.BookChanged += View_BookChanged;
			View.LoadPackage += View_LoadPackage;
			View.BookDisplayed += View_BookDisplayed;
			View.BookLoadFailed += View_BookLoadFailed;
			View.BookLoadStarted += View_BookLoadStarted;
			View.SpeakableElementSelected += AllViews_SpeakableElementSelected;
			View.SelfVoicingSpeakText += AllViews_SelfVoicingSpeakText;
			View.SpeakableElementSelectedHelpText += AllViews_SpeakableElementSelectedHelpText;

			// Configure the application's global state.
			ConfigureApplicationState();

			// It is the responsibility of the ApplicationPresenter to configure all the dependent
			// presenters.
			ConfigurePlayerPresenter();
			ConfigureNavigationPresenter();
			ConfigureDisplaySettingsPresenter();
			ConfigureSearchPresenter();

			// Perform any business logic related initialisation steps
			Init();
		}

		#endregion Constructors 

		#region Properties (5) 

		/// <summary>
		/// Holds a reference to the DisplaySettingsPresenter - the ApplicationPresenter knows about
		/// and instantiates all dependent Presenters.
		/// </summary>
		internal DisplaySettingsPresenter DisplaySettingsPresenter { get; private set; }

		/// <summary>
		/// Holds a reference to the NavigationPresenter - the ApplicationPresenter knows about
		/// and instantiates all dependent Presenters.
		/// </summary>
		internal NavigationPresenter NavigationPresenter { get; set; }

		/// <summary>
		/// Holds a reference to the PlayerPresenter - the ApplicationPresenter knows about
		/// and instantiates all dependent Presenters.
		/// </summary>
		internal PlayerPresenter PlayerPresenter { get; set; }

		/// <summary>
		/// Holds a reference to the SearchPresenter - the ApplicationPresenter knows about
		/// and instantiates all dependent Presenters.
		/// </summary>
		internal SearchPresenter SearchPresenter { get; private set; }

		/// <summary>
		/// A reference to the ApplicationPresenter's primary view.
		/// </summary>
		private IApplicationView View { get; set; }

		#endregion Properties 

		#region Delegates and Events (2) 

		// Events (2) 

		/// <summary>
		/// This event is raised when the presenter has completed loading a new book. This is
		/// used by the presenter's view (Buttercup.Control.Main) to update the user interface when
		/// this happens.
		/// </summary>
		public event EventHandler BookChanged;

		/// <summary>
		/// This event is raised by the Presenter to allow Views to monitor the progress
		/// of long running processes - for example, the call to ProcessPackage.
		/// </summary>
		public event EventHandler<ProgressNotificationEventArgs> ProgressChanged;

		#endregion Delegates and Events 

		#region Methods (28) 

		// Public Methods (2) 

		/// <summary>
		/// Gets the current book.
		/// </summary>
		/// <returns>The current book being displayed.</returns>
		public Book GetCurrentBook()
		{
			return base.MainState.CurrentBook;
		}

		/// <summary>
		/// This method is called by the presenter's view to update the DisplaySurface with an image
		/// stream.
		/// </summary>
		public Stream GetImageStream(string imagePath)
		{
			Stream returnStream = null;

			IFile file = MainState.BookFileSystem.GetFile(MainState.BookFileSystem.CombinePath(MainState.CurrentBook.FolderPath.FullName, imagePath));
			if(file.Exists)
			{
				returnStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			return returnStream;
		}
		// Private Methods (25) 

		/// <summary>
		/// All requests to speak text, from all Views, end up here. Currently we pass the request
		/// on to the Player since it is the one that deals with Audio and has to pause the book
		/// voice in order to speak.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void AllViews_SelfVoicingSpeakText(object sender, SpeechTextEventArgs e)
		{
			PlayerPresenter.PlayTextSpeech(e);
		}

		/// <summary>
		/// All requests to speak text, from all Views, end up here. Currently we pass the request
		/// on to the Player since it is the one that deals with Audio and has to pause the book
		/// voice in order to speak.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void AllViews_SpeakableElementSelected(object sender, ElementSelectedEventArgs e)
		{
			PlayerPresenter.PlaySpeakableElement(e);
		}

		/// <summary>
		/// All requests to speak text, from all Views, end up here. Currently we pass the request
		/// on to the Player since it is the one that deals with Audio and has to pause the book
		/// voice in order to speak.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void AllViews_SpeakableElementSelectedHelpText(object sender, ElementSelectedEventArgs e)
		{
			PlayerPresenter.PlaySpeakableElementHelpText(e);
		}

		/// <summary>
		/// Applies the given contrast scheme to the UI.
		/// </summary>
		/// <param name="contrastScheme">The contrast scheme to be applied.</param>
		private void ApplyContrastScheme(ContrastLevel contrastScheme)
		{
			View.ApplyContrastSetting(contrastScheme);
		}

		/// <summary>
		/// Applies the given interface size to the UI.
		/// </summary>
		/// <param name="interfaceSize">The new interface size.</param>
		private void ApplyInterfaceSize(int interfaceSize)
		{
			View.ApplyInterfaceSize(interfaceSize);
		}

		/// <summary>
		/// Configure the ApplicationState - this is used to store application wide state information
		/// that can be used by all Presenters.
		/// </summary>
		private void ConfigureApplicationState()
		{
			//Initialise the ApplicationState
			MainState = new ApplicationState();

			if(View.ServerBookReference != null)
			{
				MainState.BookFileSystem = FileSystemFactory.RemoteFileSystem;
				MainState.IsServerHostedMode = true;
			}
			else
			{
				// For local books we actaully have a choice of FileSystems, either 
				// IsolatedStorage (persistent but slow to load) or InMemory (transient but fast to load)
				MainState.BookFileSystem = FileSystemFactory.LocalFileSystem;
				MainState.IsServerHostedMode = false;
			}

			MainState.DisplaySettingsState.ApplyInterfaceSize += ApplyInterfaceSize;
			MainState.DisplaySettingsState.ApplyContrastScheme += ApplyContrastScheme;
		}

		/// <summary>
		/// The ApplicationPresenter is resposnible for configuring all the dependent Presenters.
		/// </summary>
		private void ConfigureDisplaySettingsPresenter()
		{
            View.DisplaySettingsView.ApplicationView = View;

			// The ApplicationPresenter will handle all speak requests 
			View.DisplaySettingsView.SpeakableElementSelected += AllViews_SpeakableElementSelected;
			View.DisplaySettingsView.SpeakableElementSelectedHelpText += AllViews_SpeakableElementSelectedHelpText;
			View.DisplaySettingsView.SelfVoicingSpeakText += AllViews_SelfVoicingSpeakText;

			DisplaySettingsPresenter = new DisplaySettingsPresenter(View.DisplaySettingsView, this, MainState.DisplaySettingsState);
		}

		/// <summary>
		/// The ApplicationPresenter is resposnible for configuring all the dependent Presenters.
		/// </summary>
		private void ConfigureNavigationPresenter()
		{
			// In order for the table of contents view to have access and subscribe to application
			// and player view properties and events we need to set a reference to them.
			View.NavigationView.ApplicationView = View;
			View.NavigationView.PlayerView = View.PlayerView;

			// The ApplicationPresenter will handle all speak requests 
			View.NavigationView.SpeakableElementSelected += AllViews_SpeakableElementSelected;
			View.NavigationView.SpeakableElementSelectedHelpText += AllViews_SpeakableElementSelectedHelpText;
			View.NavigationView.SelfVoicingSpeakText += AllViews_SelfVoicingSpeakText;

			NavigationPresenter = new NavigationPresenter(View.NavigationView, this, base.MainState);
		}

		/// <summary>
		/// The ApplicationPresenter is resposnible for configuring all the dependent Presenters.
		/// </summary>
		private void ConfigurePlayerPresenter()
		{
			View.PlayerView.ApplicationView = View;
			View.PlayerView.NavigationView = View.NavigationView;
			View.PlayerView.SearchView = View.SearchView;

			// The ApplicationPresenter will handle all speak requests 
			View.PlayerView.SpeakableElementSelected += AllViews_SpeakableElementSelected;
			View.PlayerView.SpeakableElementSelectedHelpText += AllViews_SpeakableElementSelectedHelpText;
			View.PlayerView.SelfVoicingSpeakText += AllViews_SelfVoicingSpeakText;

			PlayerPresenter = new PlayerPresenter(View.PlayerView, this, base.MainState);
		}

		/// <summary>
		/// The ApplicationPresenter is resposnible for configuring all the dependent Presenters.
		/// </summary>
		private void ConfigureSearchPresenter()
		{
			View.SearchView.ApplicationView = View;
			View.SearchView.SpeakableElementSelected += AllViews_SpeakableElementSelected;
			View.SearchView.SpeakableElementSelectedHelpText += AllViews_SpeakableElementSelectedHelpText;
			View.SearchView.SelfVoicingSpeakText += AllViews_SelfVoicingSpeakText;

			SearchPresenter = new SearchPresenter(View.SearchView, this, MainState.SearchState);
		}

		/// <summary>
		/// This method is called whenever progress has been made in loading a file (notified by
		/// progress changes made in the datacontext store and the package inspector). 
		/// The ApplicationPresenter can then weight the changes from the two sources and
		/// update the main view for notifying the user of the progress.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The EventArgs containing the progress progress made so far.</param>
		private void DoProgressChanged(object sender, ProgressNotificationEventArgs e)
		{
			double progress;

			if(sender.GetType() == typeof(PackageInspector))
			{
				// For now assume the inspector is doing 10% of the (initial) total work
				progress = e.PercentageProgress * 0.1;
			}
			else
			{
				// Assume that if the store is reporting progress then the Inspector has finished
				// and we are on the final 90%
				progress = 10 + e.PercentageProgress * 0.9;
			}

			// Still a couple of minor issues with calculating exact percentage
			if(progress > 100)
			{
				progress = 100;
			}

			// Pass on overall progress to any subscribers of the presenter's progress 
			// (i.e. the View)
			if(ProgressChanged != null)
			{
				string message = "Opening Book... " + progress + "%";
				ProgressChanged(this, new ProgressNotificationEventArgs(progress, message));
			}
		}

		/// <summary>
		/// This method is called when a book opf file has been asynchronously opened. Then we can begin to create a Package for
		/// the loaded book.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The EventArgs containing the book opf and the parent directory path.</param>
		private void file_OpenAsyncComplete(object sender, DownloadCompleteEventArgs e)
		{
            //Remove the event
            //This prevents a file from getting opened twice causing an exception when trying to read
            //the stream of the document since the file has already been opened and is currently stored 
            //in memory.
            ((IFile)sender).OpenAsyncComplete -= file_OpenAsyncComplete;

			// TODO: Proper regex or other types of path handling to get the parent directory
			// Cheat, currently just grab the url given in the query string till the last '/' and use that
			// as the parent directory
			string opfParentDirectory = e.UserState.ToString().Substring(0, e.UserState.ToString().LastIndexOf('/'));
            
			IPackage package = new Package300(e.Result);
			package.BookFolder = opfParentDirectory;
			SetCurrentBook(package);
		}

		/// <summary>
		/// Initialisation code for the Presenter.
		/// </summary>
		private void Init()
		{
			//Apply the initial contrast scheme.
			View.ApplyContrastSetting(MainState.DisplaySettingsState.ContrastScheme);
			View.ApplyInterfaceSize(MainState.DisplaySettingsState.InterfaceSize);

			if(!MainState.IsServerHostedMode)
			{
				//TODO: change string literals to reference speech resources
				PlayerPresenter.PlayTextSpeech(new SpeechTextEventArgs("Welcome to Buttercup. Press O to open a Daisy zip file."));
			}
		}

		/// <summary>
		/// This event handler responds to any progress updates from the package inspector as a book is being loaded.
		/// It then calls a method to update the view, notifying the user of the progress.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The EventArgs containing the progress progress made so far.</param>
		private void inspector_ProgressChanged(object sender, ProgressNotificationEventArgs e)
		{
			DoProgressChanged(sender, e);
		}

		/// <summary>
		/// Initiates the processing of loading a book (from a zip stream) into the FileSystem, by loading and creating
		/// the book package and passing that off to the datacontext to load the rest of the book.
		/// </summary>
		/// <param name="zipStream">FileStream of the zip file to load the book from.</param>
		private void LoadBook(FileStream zipStream)
		{
			// The DataContext is instatiated with a IFileSystem implementation that will determine
			// where the books are stored to and retrieved from. For example, IsolatedStorage/ServerStorage
			DataContext store = new DataContext(base.MainState.BookFileSystem);

			// Listen to the progress the store DataContext makes 
			store.ProgressChanged += store_ProgressChanged;

			IPackageInspector inspector;

			// To ensure the FileStream is property disposed wrap it in
			// a using statement.
			using (FileStream fileStream = zipStream)
			{
                inspector = new PackageInspector();

				// The inspector will pass on any progress updates to this event. We subscribe to
				// it from the View to be able to update the display.
				inspector.ProgressChanged += inspector_ProgressChanged;

				// Process the package - this will perform an initial parse over the zip file
				// to derive some basic information like its version, the bookId and Filecount. 
                // This method will raise some ProgressChanged events for us to update the UI.
				inspector.ProcessPackage(fileStream);

				// Check the file was OK.
				if(!inspector.IsValid)
				{
					// TODO: Could be enhanced to return all the errors.
					// While there could actally be more than one error in ValidationErrors
					// we are only going to notify of the first.
					throw new InvalidPackageException(inspector.ValidationErrors[0]);
				}
				else
				{
					// Save the book files to the store - don't bother replacing if the
					// book is already there. Again this will take some time and will raise
					// ProgressChanged events for us to update the UI with progress.
                    store.LoadZipPackage(inspector, false);
				}
			}

			// Set the CurrentBook on View
			SetCurrentBook(inspector.Package);
		}

		/// <summary>
		/// This method is called when a book package has been constructed, we then tell
		/// the datacontext to complete loading the book contents (xml, ncx etc.)
		/// </summary>
		/// <param name="package">Book package to load the contents of.</param>
		private void SetCurrentBook(IPackage package)
		{
			DataContext store = new DataContext(base.MainState.BookFileSystem);
			store.BookCreated += store_BookCreated;
			store.GetBookAsync(package);
		}

		/// <summary>
		/// This event handler responds to an event in the DataContext that is raised when
		/// a Book has been completely loaded. We then raise the BookChanged event which
		/// the main view responds to for updating UI elements for the new book.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The EventArgs containing the book that has been created.</param>
		private void store_BookCreated(object sender, BookCreateEventArgs e)
		{
			base.MainState.CurrentBook = e.Book;
			if(BookChanged != null)
			{
				BookChanged(this, new EventArgs());
			}
		}

		/// <summary>
		/// This method processes the ProgressChanged event of the Store. This is raised by the 
		/// store on method calls that can take a long time to process, for example, SaveBookFiles.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The EventArgs containing the progress progress made so far.</param>
		private void store_ProgressChanged(object sender, ProgressNotificationEventArgs e)
		{
			DoProgressChanged(sender, e);
		}

		/// <summary>
		/// Handles event when a new book has been loaded. This event is also handled by the
		/// other dependent views (e.g. Navigator and Player to respond to this event).
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e"></param>
		private void View_BookChanged(object sender, EventArgs e)
		{
			SearchPresenter.ClearSearchResults(); //Don't allow previous search results to show.

			View.SetControlButtonState(true);
		}

		/// <summary>
		/// This handler is called when the View has finished rendering the book within the 
		/// surface.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void View_BookDisplayed(object sender, EventArgs e)
		{
			View.WaitingOnProgress = false;
		}

		/// <summary>
		/// This method is called when the ApplicationView notifies us that a book has failed to 
		/// load.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void View_BookLoadFailed(object sender, EventArgs e)
		{
			// Allow the Open button to be selected again (local books only).
			View.SetControlButtonState(true);

			// Tell the view that we are no longer waiting (this will set the cursor back to an arrow)
			View.WaitingOnProgress = false;

			//if the currently loaded book is on the server (the one _before_ the fail), restore to server file system
			if(MainState.PreviousServerHostedMode)
			{
				//revert back to server mode
				MainState.BookFileSystem = FileSystemFactory.RemoteFileSystem;
				MainState.IsServerHostedMode = true;
			}
		}

		/// <summary>
		/// Listens to the event that a local book is being loaded, and sets the working filesystem to the
		/// local file system.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void View_BookLoadStarted(object sender, EventArgs e)
		{
			//If we are switching from server to local books
			if(MainState.IsServerHostedMode)
			{
				//Set the appropriate states
				MainState.PreviousServerHostedMode = MainState.IsServerHostedMode;
				MainState.BookFileSystem = FileSystemFactory.LocalFileSystem;
				MainState.IsServerHostedMode = false;
			}
		}

		/// <summary>
		/// This event is raised by the View when a book needs to be opened from a reference to 
		/// an opf file. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void View_LoadPackage(object sender, LoadPackageEventArgs e)
		{
			// Retrieve the package from the BookFileSystem 
			IFile file = base.MainState.BookFileSystem.GetFile(e.PackageFullPath);
			file.OpenAsyncComplete += file_OpenAsyncComplete;
			file.OpenAsync(e.PackageFullPath);
		}

		/// <summary>
		/// Handles the event raised when a Zip File is available to be loaded. The View gets a stream
		/// from a user opening a file open dialog.
		/// </summary>
		/// <remarks>
		/// IMPORTANT: This method is called asynchronously from the main view's UI thread. Any interaction
		/// with the view that could effect the user interface must be done on the UI thread. This means
		/// that the view methods called during processing must be wrapped in "this.Dispatcher.BeginInvoke" calls.
		/// </remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void View_LoadZipPackage(object sender, LoadZipPackageEventArgs e)
		{
			LoadBook(e.ZipStream);
		}

		/// <summary>
		/// Handles the event raised when the user selects a particular panel, or hides the
		/// currently displayed panel.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The EventArgs containing the selected panel display mode.</param>
		private void View_SelectPanel(object sender, PanelDisplayModeEventArgs e)
		{
			SidePanelDisplayMode currentDisplayMode = MainState.SidePanelMode;
			SidePanelDisplayMode newDisplayMode = e.PanelDisplayMode;

			if(currentDisplayMode == newDisplayMode)
			{
				if(currentDisplayMode != SidePanelDisplayMode.Hidden)
				{
					//Hide the panel if the user selected a panel that is already visible.
					MainState.SidePanelMode = SidePanelDisplayMode.Hidden;
					View.ToggleSidePanel(MainState.SidePanelMode);
				}
			}
			else
			{
				MainState.SidePanelMode = newDisplayMode;
				View.ToggleSidePanel(newDisplayMode);
				View.NotifyPanelContentsChanged();
			}
		}
		// Internal Methods (1) 

		/// <summary>
		/// Notifies the view that its panel contents have changed.
		/// </summary>
		internal void NotifyPanelContentsChanged()
		{
			View.NotifyPanelContentsChanged();
		}

		#endregion Methods 
	}
}