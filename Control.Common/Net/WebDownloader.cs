using System;
using System.Net;


namespace Buttercup.Control.Common.Net
{
	public class WebDownloader : IDownloader
	{
		#region Fields (1) 

		private WebClient _webClient;

		#endregion Fields 



		#region Constructors (2) 

		public WebDownloader()
		{
            _webClient = new WebClient();
			_webClient.OpenReadCompleted += WebClient_OpenReadCompleted;
			_webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
		}

		#endregion Constructors 



		#region Delegates and Events (2) 

		// Events (2) 

		public event EventHandler<DownloadCompleteEventArgs> DownloadComplete;

		public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

		#endregion Delegates and Events 



		#region Methods (3) 

		// Public Methods (1) 

		public void DownloadAsync(Uri address, object userToken)
		{
			_webClient.OpenReadAsync(address, userToken);
		}



		// Private Methods (2) 

		private void WebClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
		{
			if(DownloadProgressChanged != null)
			{
				DownloadProgressChanged(this, new DownloadProgressChangedEventArgs(e.ProgressPercentage, e.UserState, e.BytesReceived, e.TotalBytesToReceive));
			}
		}



		private void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
		{
			if(DownloadComplete != null)
			{
				DownloadComplete(this, new DownloadCompleteEventArgs(e.Result, e.Error, e.Cancelled, e.UserState));
			}
		}

		#endregion Methods 
	}
}