using System;
using Control.MVP;

namespace Control.UI
{
    public interface CommandHandler
    {
        #region Delegates and Events (1)

        // Events (1) 

        event EventHandler<KeyCommandEventArgs> KeyDown;
        event EventHandler<KeyCommandEventArgs> KeyUp;

        #endregion Delegates and Events

        #region Operations (1)

        /// <summary>
        /// Raise the event for a command based on the key pressed
        /// </summary>
        /// <param name="e"></param>
        void KeyPressed(KeyCommandEventArgs e);

        /// <summary>
        /// Raise the event for a command based on the key being released
        /// </summary>
        /// <param name="e"></param>
        void KeyReleased(KeyCommandEventArgs e);

        #endregion Operations
    }
}