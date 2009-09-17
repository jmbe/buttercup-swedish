using System;
using System.Windows.Input;

namespace Control.UI
{
    /// <summary>
    /// Contains all the definitions for the shortcut keys
    /// </summary>
    public class Shortcuts
    {
        #region Fields (29)

        /// <summary>
        /// Shortcut Definitions
        /// </summary>
        //public static Shortcut AddBookmark = new Shortcut(Key.B, ModifierKeys.Shift);//Shift + 'B'
        public static Shortcut BookInformationPanel = new Shortcut(Key.I, ModifierKeys.None);//'I'
        public static Shortcut BookmarkPanel = new Shortcut(Key.B, ModifierKeys.None);//'B'
        public static Shortcut ClosePanel = new Shortcut(Key.Escape, ModifierKeys.None);//'Escape'
        public static Shortcut Contrast = new Shortcut(Key.A, ModifierKeys.None);//'A'
        public static Shortcut DecreaseInterfaceSize = new Shortcut(Key.J, ModifierKeys.Shift);//Shift + 'J'
        public static Shortcut DecreaseVolume = new Shortcut(Key.J, ModifierKeys.None);//'J'
        public static Shortcut DeleteBookmark = new Shortcut(Key.Delete, ModifierKeys.None);//'Delete'
        public static Shortcut DisplaySettingsPanel = new Shortcut(Key.D, ModifierKeys.None);//'D'
        public static Shortcut FindPanelPage = new Shortcut(Key.P, ModifierKeys.None);//'P'
        public static Shortcut FindPanelSearch = new Shortcut(Key.F, ModifierKeys.None);//'F'
        public static Shortcut FirstHeading = new Shortcut(Key.Home, ModifierKeys.None);//'Home'
        public static Shortcut FontType = new Shortcut(Key.T, ModifierKeys.None);//'T'
        public static Shortcut HelpPanel = new Shortcut(191, ModifierKeys.Shift);//'?'
        public static Shortcut IncreaseInterfaceSize = new Shortcut(Key.K, ModifierKeys.Shift);//Shift + 'K'
        public static Shortcut IncreaseVolume = new Shortcut(Key.K, ModifierKeys.None);//'K'
        public static Shortcut LastHeading = new Shortcut(Key.End, ModifierKeys.None);//'End'
        public static Shortcut MuteUnmute = new Shortcut(Key.M, ModifierKeys.None);//'M'
        public static Shortcut ContentsPanel = new Shortcut(Key.C, ModifierKeys.None);//'C'
        public static Shortcut NextHeader = new Shortcut(Key.Down, ModifierKeys.None);//'Down'
        public static Shortcut NextPage = new Shortcut(Key.PageDown, ModifierKeys.None);//'PageDown'
        public static Shortcut NextSection = new Shortcut(Key.Right, ModifierKeys.None);//'Right'
        public static Shortcut OpenBookDialog = new Shortcut(Key.O, ModifierKeys.None);//'O'
        public static Shortcut PlayPause = new Shortcut(Key.Space, ModifierKeys.None);//'Space'
        public static Shortcut PreviousHeader = new Shortcut(Key.Up, ModifierKeys.None);//'Up'
        public static Shortcut PreviousPage = new Shortcut(Key.PageUp, ModifierKeys.None);//'PageUp'
        public static Shortcut PreviousSection = new Shortcut(Key.Left, ModifierKeys.None);//'Left'
        public static Shortcut SpeakHelpText = new Shortcut(Key.Multiply, ModifierKeys.None);//'NumPad *' Should be question mark
        public static Shortcut ToggleSelfVoicing = new Shortcut(Key.Space, ModifierKeys.Control);//Control + 'M'
        //public static Shortcut ZoomIn = new Shortcut(Key.K, ModifierKeys.Control);//Control + 'K'
        //public static Shortcut ZoomOut = new Shortcut(Key.J, ModifierKeys.Control);//Control + 'J'

        public static Shortcut StopSelfVoice = new Shortcut(Key.Subtract, ModifierKeys.None);//'-'
        //public static Shortcut DecreaseRate = new Shortcut(Key.NumPad1, ModifierKeys.None);//'NumPad1'
        //public static Shortcut IncreaseRate = new Shortcut(Key.NumPad2, ModifierKeys.None);//'NumPad2'

        #endregion Fields

    }

    /// <summary>
    /// Shortcut definition, basically a Key, ModifierKeys Pair
    /// </summary>
    public class Shortcut
    {
        public static int None = -1;

        #region Constructors (1)

        public Shortcut(Key key, ModifierKeys modifiers)
        {
            PlatformKey = None;
            ShortcutKey = key;
            Modifiers = modifiers;
        }

        public Shortcut(int key, ModifierKeys modifiers)
        {
            PlatformKey = key;
            Modifiers = modifiers;
        }

        public Shortcut(int platKey, Key key, ModifierKeys modifiers)
        {
            PlatformKey = platKey;
            ShortcutKey = key;
            Modifiers = modifiers;
        }

        #endregion Constructors

        #region Properties (2)

        public ModifierKeys Modifiers { get; private set; }

        public Key ShortcutKey { get; private set; }

        public int PlatformKey { get; private set;}

        #endregion Properties

        #region Methods (1)

        // Public Methods (1) 

        public Boolean Equals(Shortcut shortcut)
        {
            if (PlatformKey != None && shortcut.PlatformKey != None)
            {
                return (shortcut.PlatformKey == PlatformKey && shortcut.Modifiers.Equals(Modifiers));
            }
            return (shortcut.ShortcutKey.Equals(ShortcutKey) && shortcut.Modifiers.Equals(Modifiers));
        }

        #endregion Methods
    }
}