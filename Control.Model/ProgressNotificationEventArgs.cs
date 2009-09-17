using System;

namespace Buttercup.Control.Model
{
    public class ProgressNotificationEventArgs : EventArgs
    {
		#region Constructors (1) 

        /// <summary>
        /// Used to notify the application presenter of progress made when loading a book, includes progress percentage
        /// and a message to notify user of what's happening
        /// </summary>
        /// <param name="percentageProgress"></param>
        /// <param name="progressMessage"></param>
        public ProgressNotificationEventArgs(double percentageProgress, string progressMessage)
        {
            PercentageProgress = percentageProgress;
            ProgressMessage = progressMessage;
        }

		#endregion Constructors 

		#region Properties (2) 

        public double PercentageProgress { get; set; }

        public string ProgressMessage { get; set; }

		#endregion Properties 
    }
}