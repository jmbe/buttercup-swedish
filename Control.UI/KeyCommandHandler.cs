using System;
using System.Windows.Input;
using Control.MVP;
using Control.UI;

namespace Buttercup.Control.UI
{
    /// <summary>
    /// The KeyCommandHandler listens to the Main KeyDown event and reroutes the key press
    /// to the subscribing views.
    /// TODO: Is this needed? Possibly views can simply subscribe directly to the Main.KeyDown event itself.
    /// </summary>
    public class KeyCommandHandler : CommandHandler
    {
        private KeyCommandEventArgs _keyCommandEventArgs;

        #region Fields (3)

        /// <summary>
        /// The names of UI controls that need special treatment when handling key commands
        /// (i.e. having their own behaviour when arrow keys are pressed)
        /// </summary>
        public const string ListBoxItem = "Buttercup.Control.ExtendedListBoxItem";
        public const string SliderControl = "System.Windows.Controls.Primitives.Thumb";
        public const string TextBox = "Buttercup.Control.MediaTextBox";

        #endregion Fields

        #region Delegates and Events (1)

        // Events (1) 

        /// <summary>
        /// This event is raised when the KeyDown event is fired on the Main View, notifying the other
        /// views which subscribe to this KeyCommandHandler that a key has been pressed.
        /// </summary>
        public event EventHandler<KeyCommandEventArgs> KeyDown;
        public event EventHandler<KeyCommandEventArgs> KeyUp;

        #endregion Delegates and Events

        #region Methods (1)

        // Public Methods (1) 

        /// <summary>
        /// Raise the event for a command based on the key pressed
        /// </summary>
        /// <param name="e"></param>
        public void KeyPressed(KeyCommandEventArgs e)
        {
            _keyCommandEventArgs = e;
            KeyDown(this, e);
        }

        /// <summary>
        /// Raise the event for a command based on the key being released
        /// </summary>
        /// <param name="e"></param>
        public void KeyReleased(KeyCommandEventArgs e)
        {
            //KeyUp(this, new KeyCommandEventArgs(_keyEventArgs));
            if (_keyCommandEventArgs != null)
                KeyUp(this, _keyCommandEventArgs);
        }

        #endregion Methods
    }
}