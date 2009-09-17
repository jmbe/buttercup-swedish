using System;
using Buttercup.Control.Model;
using Buttercup.Control.UI;

namespace Control.MVP.Views
{
    public interface IDisplaySettingsView
    {
		#region Data Members (2) 

        int FocusedItem { get; set; }
        IApplicationView ApplicationView { get; set; }

        /// <summary>
        /// Handler for shortcut key events
        /// </summary>
        KeyCommandHandler KeyCommandHandler { get; set; }

		#endregion Data Members 

		#region Delegates and Events (5) 

		// Events (5) 

        /// <summary>
        /// Raised when the contrast level is changed by the user.
        /// </summary>
        event EventHandler<ContrastSchemeEventArgs> ChangeContrastScheme;

        /// <summary>
        /// Raised when the interface size is changed by the user.
        /// </summary>
        event EventHandler<InterfaceSizeEventArgs> ChangeInterfaceSize;

        /// <summary>
        /// Raised when some contextual speech text is to be spoken.
        /// </summary>
        event EventHandler<SpeechTextEventArgs> SelfVoicingSpeakText;

        /// <summary>
        /// Raised when a UI element is required to be spoken.
        /// </summary>
        event EventHandler<ElementSelectedEventArgs> SpeakableElementSelected;

        /// <summary>
        /// Raised when a UI element help text is required to be spoken.
        /// </summary>
        event EventHandler<ElementSelectedEventArgs> SpeakableElementSelectedHelpText;

		#endregion Delegates and Events 

		#region Operations (2) 

        /// <summary>
        /// Updates the reported contrast scheme in the display settings.
        /// </summary>
        /// <param name="contrastScheme">The contrast scheme.</param>
        void UpdateContrastScheme(ContrastLevel contrastScheme);

        /// <summary>
        /// Updates the reported interface size in the display settings.
        /// </summary>
        /// <param name="value">The interface size value.</param>
        void UpdateInterfaceSize(int value);

		#endregion Operations 
    }
}