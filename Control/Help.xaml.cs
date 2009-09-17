using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using Buttercup.Control.Resources;
using Buttercup.Control.UI;
using Buttercup.Control.UI.Extensions;
using Control.MVP;
using Control.UI;

namespace Buttercup.Control
{
    public partial class Help : UserControl
    {
        private KeyCommandHandler _keyCommandHandler;

        #region Constructors (1) 

        public Help()
        {
            InitializeComponent();
            KeyDown += new System.Windows.Input.KeyEventHandler(Help_KeyDown);
            KeyUp += new System.Windows.Input.KeyEventHandler(Help_KeyUp);

            Loaded += new System.Windows.RoutedEventHandler(Help_Loaded);
        }

        void Help_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationVoice.MainInstance.QueueTextToSpeak(Speech.HelpPanelMessage);
            this.Focus();
        }

        public KeyCommandHandler KeyCommandHandler
        {
            get { return _keyCommandHandler; }
            set
            {
                _keyCommandHandler = value;
                _keyCommandHandler.KeyDown += _keyCommandHandler_KeyIsPressed;
            }
        }

        public string ContentsAsString()
        {
            string contents = "Shortcut Keys. O. Open Dialog. C. Show Content. I. Show Book Info. Shift+Question Mark. Show Help. Spacebar. Play or Pause. Home. Go to First Heading. End.";
            string contents2 = " Go to Last Heading. Page Up. Go to Previous Page. Page Down. Go to Next Page. Up Arrow. Go to Previous Header. Down Arrow. Go to the Next Header.";
            string contents3 = " Left Arrow. Go to Previous Phrase. Right Arrow. Go to Next Phrase. K. Increase Volume. J. Decrease Volume. M. Mute or Un-Mute.";
            string contents4 = " F. Find Text. P. Go To Page. Shift+K. Increase Content Size. Shift+J. Decrease Content Size. A. Contrast. Escape. Close Panel.";
            string contents5 = " Tab. Tab Forward. Shift+Tab. Tab Backwards. Enter. Start Action. Backspace. Clear Search Textbox. Ctrl+Spacebar. Toggle Self Voicing. Numpad Asterisk. Speak Help Text. Numpad Minus. Stop Self Voice.";

            return (contents + contents2 + contents3 + contents4 + contents5);
        }

        private void _keyCommandHandler_KeyIsPressed(object sender, KeyCommandEventArgs e)
        {
            
        }

        /// <summary>
        /// Capture KeyUp events, needed in each subview as the Scrollviewer control
        /// that the panels are situated in will not let certain keypresses through
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Help_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ScrollViewer panelScrollViewer = UIElementExtensions.GetUIParentOfType<ScrollViewer>(e.OriginalSource as UIElement);

			//If we are not in a scrollviewer, or the scrollviewer has a vertical scrollbar visible, then arrows won't be let through
            if (panelScrollViewer == null || panelScrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            {
                if (e.Handled == false &&
                    (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.PageUp || e.Key == Key.PageDown || e.Key == Key.Home ||
                     e.Key == Key.End))
                    KeyCommandHandler.KeyReleased(new KeyCommandEventArgs(e));
            }
        }

        /// <summary>
        /// Capture KeyDown events, needed in each subview as the Scrollviewer control
        /// that the panels are situated in will not let certain keypresses through
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Help_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ScrollViewer panelScrollViewer = UIElementExtensions.GetUIParentOfType<ScrollViewer>(e.OriginalSource as UIElement);

			//If we are not in a scrollviewer, or the scrollviewer has a vertical scrollbar visible, then arrows won't be let through
            if (panelScrollViewer == null || panelScrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            {
                if (e.Handled == false &&
                    (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.PageUp || e.Key == Key.PageDown || e.Key == Key.Home ||
                     e.Key == Key.End))
                    KeyCommandHandler.KeyPressed(new KeyCommandEventArgs(e));
            }
        }

		#endregion Constructors 

		#region Methods (1) 

		// Protected Methods (1) 

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new HelpAutomationPeer(this);
        }

		#endregion Methods 
    }
}