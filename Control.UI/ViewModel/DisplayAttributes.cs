using System.ComponentModel;
using System.Windows.Media;
using System;

namespace Buttercup.Control.UI.ViewModel
{
    public class DisplayAttributes : INotifyPropertyChanged
    {
        #region Fields (19)

        private Brush _controlSectionBackground;
        private Brush _defaultBackground;
        private Brush _defaultForeground;
        private Brush _highlightBackground;
        private Brush _highlightForeground;
        private Brush _selectionBackground;
		private Brush _selectionBorderBrush;
		private Brush _selectionForeground;
    	private double _visualScale;

        public static Brush BlackBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        public static Brush BlueGreyBrush = new SolidColorBrush(Color.FromArgb(255, 95, 95, 127));
        public static Brush DarkBlueBrush = new SolidColorBrush(Color.FromArgb(255, 0, 15, 79));
        public static Brush DarkGreyBrush = new SolidColorBrush(Color.FromArgb(255, 95, 95, 95));
        public static Brush DarkYellowBrush = new SolidColorBrush(Color.FromArgb(255, 47, 43, 0));
        public static Brush LightGreyBrush = new SolidColorBrush(Color.FromArgb(255, 191, 191, 191));
        public static Brush LightWhiteBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        public static Brush LightYellowBrush = new SolidColorBrush(Color.FromArgb(255, 255, 247, 127));
        public static Brush MidBlueBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 127));
        public static Brush MidGreyBrush = new SolidColorBrush(Color.FromArgb(255, 159, 159, 159));
        public static Brush WhiteBrush = new SolidColorBrush(Color.FromArgb(255, 223, 223, 223));
        public static Brush YellowBrush = new SolidColorBrush(Color.FromArgb(255, 255, 191, 0));

        #endregion Fields

        #region Constructors (1)

        public DisplayAttributes()
        {
            DefaultBackground = DarkBlueBrush;
            DefaultForeground = LightWhiteBrush;
            ControlSectionBackground = DarkBlueBrush;
            HighlightBackground = YellowBrush;
            HighlightForeground = BlackBrush;
            SelectionBackground = YellowBrush;
        	SelectionBorderBrush = YellowBrush;
            SelectionForeground = BlackBrush;
        }

        #endregion Constructors

        #region Properties (14)

        /// <summary>
        /// Gets or sets the common instance of the display attributes used across the entire application.
        /// </summary>
        /// <value>The common instance.</value>
        public static DisplayAttributes CommonInstance { get; set; }

        /// <summary>
        /// Gets or sets the background brush for the controls section.
        /// </summary>
        /// <value>The default background brush for the controls section.</value>
        public Brush ControlSectionBackground
        {
            get { return _controlSectionBackground; }
            set
            {
                _controlSectionBackground = value;
                RaisePropertyChanged("ControlSectionBackground");
            }
        }

        /// <summary>
        /// Gets or sets the default background brush - i.e. the brush used most of the time
        /// to render the background.
        /// </summary>
        /// <value>The default background brush.</value>
        public Brush DefaultBackground
        {
            get { return _defaultBackground; }
            set
            {
                _defaultBackground = value;
                RaisePropertyChanged("DefaultBackground");
            }
        }


        /// <summary>
        /// Gets or sets the default foreground brush - i.e. the brush used most of the time
        /// to render the foreground.
        /// </summary>
        /// <value>The default foreground brush.</value>
        public Brush DefaultForeground
        {
            get { return _defaultForeground; }
            set
            {
                _defaultForeground = value;
                RaisePropertyChanged("DefaultForeground");
            }
        }


        /// <summary>
        /// Gets or sets the background brush used for highlighted content.
        /// </summary>
        /// <value>The background brush used for highlighted content.</value>
        public Brush HighlightBackground
        {
            get { return _highlightBackground; }
            set
            {
                _highlightBackground = value;
                RaisePropertyChanged("HighlightBackground");
            }
        }


        /// <summary>
        /// Gets or sets the foreground brush used for highlighted content.
        /// </summary>
        /// <value>The foreground brush used for highlighted content.</value>
        public Brush HighlightForeground
        {
            get { return _highlightForeground; }
            set
            {
                _highlightForeground = value;
                RaisePropertyChanged("HighlightForeground");
            }
        }


        /// <summary>
        /// Gets or sets the background brush used for selected content.
        /// </summary>
        /// <value>The background brush used for selected content.</value>
        public Brush SelectionBackground
        {
            get { return _selectionBackground; }
            set
            {
                _selectionBackground = value;
                RaisePropertyChanged("SelectionBackground");
            }
        }


        /// <summary>
        /// Gets or sets the border brush used for selected control borders.
        /// </summary>
		/// <value>The border brush used for selected control borders.</value>
        public Brush SelectionBorderBrush
        {
            get { return _selectionBorderBrush; }
            set
            {
				_selectionBorderBrush = value;
				RaisePropertyChanged("SelectionBorderBrush");
            }
        }


		/// <summary>
		/// Gets or sets the foreground brush used for selected content.
		/// </summary>
		/// <value>The foreground brush used for selected content.</value>
		public Brush SelectionForeground
		{
			get { return _selectionForeground; }
			set
			{
				_selectionForeground = value;
				RaisePropertyChanged("SelectionForeground");
			}
		}


		/// <summary>
		/// Gets or sets the visual scaling factor of the entire interface.
		/// </summary>
		/// <value>The visual scaling factor of the entire interface.</value>
		public double VisualScale
		{
			get { return _visualScale; }
			set
			{
				_visualScale = Math.Max(0.1, value);
				RaisePropertyChanged("VisualScale");
			}
		}

        #endregion Properties

        #region Delegates and Events (1)

        // Events (1) 

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Delegates and Events

        #region Methods (1)

        // Private Methods (1) 

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}