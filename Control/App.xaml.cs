using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Browser;
using Buttercup.Control.Common;
using Buttercup.Control.Common.IO;


namespace Buttercup.Control
{
	/// <summary>
	/// Represents the main entry point of the application.
	/// </summary>
	public partial class App : Application
	{
        
        private string _loggingEnabledReference;

		#region Constructors (1) 

		/// <summary>
		/// Default constructor
		/// </summary>
		public App()
		{
			Startup += Application_Startup;
			UnhandledException += Application_UnhandledException;

            if (LoggingEnabledReference != null && LoggingEnabledReference == "true")
            {
                Logger.LoggingEnabled = true;
            }
            else
            {
                Logger.LoggingEnabled = false;
            }

			// Initialise a Logger instance
			if (Logger.LoggingEnabled)
			{
			    Logger.Instance = new Logger(IsolatedStorageFileSystem.GetForApplication());
			}

			InitializeComponent();
		}

		#endregion Constructors 



		#region Methods (3) 

        public string LoggingEnabledReference
        {
            get
            {
                if (_loggingEnabledReference == null)
                {
                    IDictionary<string, string> queryString = HtmlPage.Document.QueryString;
                    if (queryString.ContainsKey("log"))
                    {
                        _loggingEnabledReference = queryString["log"];
                    }
                }
                return _loggingEnabledReference;
            }
            set { throw new NotImplementedException(); }
        }

		/// <summary>
		/// Handles the Startup event to initialise any application level parameters.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			RootVisual = new Main();
		}



		/// <summary>
		/// Handles any unhandled exceptions raised in the application.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			// Always log the exception.
			Logger.Log(e.ExceptionObject);

			//If the app is running outside of the debugger then report the exception using
			//the browser's exception mechanism. On IE this will display it a yellow alert 
			//icon in the status bar and Firefox will display a script error.
			if(!System.Diagnostics.Debugger.IsAttached)
			{
				// NOTE: This will allow the application to continue running after an exception has been thrown
				// but not handled. 
				// For production applications this error handling should be replaced with something that will 
				// report the error to the website and stop the application.
				e.Handled = true;
				Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
			}
			else
			{
				
			}
		}



		/// <summary>
		/// Write the error message to the browser. Used in Debug mode when exceptions have been raised.
		/// </summary>
		/// <param name="e"></param>
		private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
		{
			try
			{
				string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
				errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

				System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight 2 Application " + errorMsg + "\");");
			}
			catch(Exception)
			{
			}
		}

		#endregion Methods 
	}
}