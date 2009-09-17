using System;
using System.Windows;

namespace Buttercup.Control.Model
{
    public class DisplaySettingsState
    {
		#region Fields (2) 

        public enum Focus
        {
            ChangeSize,
            Contrast
        } ;

        private ContrastLevel _contrastScheme;
        private int _interfaceSize;

		#endregion Fields 

		#region Constructors (1) 

        public DisplaySettingsState()
        {
            InterfaceSize = 12;
            ContrastScheme = ContrastLevel.WhiteTextOnBlue;

            //On PCs that run Windows, the HighContrast property is true when the user has enabled High Contrast in Control Panel. 
            //On Macintosh computers, HighContrast is true when the user has selected the White on Blackoption in Universal Access.
            //If set to true set the contrast level to black text on white.
            if (SystemParameters.HighContrast)
                ContrastScheme = ContrastLevel.BlackTextOnWhite;
        }

		#endregion Constructors 

		#region Properties (2) 

        /// <summary>
        /// Gets or sets the contrast scheme to display the interface in.
        /// </summary>
        /// <value>The contrast scheme.</value>
        public ContrastLevel ContrastScheme
        {
            get { return _contrastScheme; }
            set
            {
                _contrastScheme = value;

                if (ApplyContrastScheme != null)
                    ApplyContrastScheme(_contrastScheme);
            }
        }

        /// <summary>
        /// Gets or sets the interface size multiplier.
        /// </summary>
        /// <value>The interface size multiplier.</value>
        /// <remarks>The interface size is scaled so that a value of 8 corresponds to 100%.
        /// Values may be in the range of 4 to 32 inclusive (50% to 400%).</remarks>
        public int InterfaceSize
        {
            get { return _interfaceSize; }
            set
            {
                _interfaceSize = Math.Max(10, Math.Min(30, value));

                if (ApplyInterfaceSize != null)
                    ApplyInterfaceSize(_interfaceSize);
            }
        }

		#endregion Properties 

		#region Delegates and Events (4) 

		// Delegates (2) 

        public delegate void SetContrastSchemeMethod(ContrastLevel contrastScheme);
        public delegate void SetInterfaceSizeMethod(int interfaceSize);
		// Events (2) 

        /// <summary>
        /// Raised when the contrast scheme changes and needs to be updated in the View.
        /// </summary>
        public event SetContrastSchemeMethod ApplyContrastScheme;

        /// <summary>
        /// Raised when the interface size changes and needs to be updated in the View.
        /// </summary>
        public event SetInterfaceSizeMethod ApplyInterfaceSize;

		#endregion Delegates and Events 
    }
}