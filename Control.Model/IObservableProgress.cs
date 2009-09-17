using System;

namespace Buttercup.Control.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IObservableProgress
    {
		#region Delegates and Events (1) 

		// Events (1) 

        event EventHandler<ProgressNotificationEventArgs> ProgressChanged;

		#endregion Delegates and Events 
    }
}