using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Buttercup.Control.UI.ViewModel;


namespace Buttercup.Control
{
	public class MediaSlider : Slider
	{
		#region Fields (8) 

		public static readonly DependencyProperty FocusBackgroundProperty =
			DependencyProperty.Register("FocusBackground", typeof(Brush), typeof(MediaSlider),
				new PropertyMetadata(DisplayAttributes.YellowBrush));
		public static readonly DependencyProperty FocusForegroundProperty =
			DependencyProperty.Register("FocusForeground", typeof(Brush), typeof(MediaSlider),
				new PropertyMetadata(DisplayAttributes.BlackBrush));
		public static readonly DependencyProperty SelectionBackgroundProperty =
			DependencyProperty.Register("SelectionBackground", typeof(Brush), typeof(MediaSlider),
				new PropertyMetadata(DisplayAttributes.YellowBrush));
		public static readonly DependencyProperty SelectionForegroundProperty =
			DependencyProperty.Register("SelectionForeground", typeof(Brush), typeof(MediaSlider),
				new PropertyMetadata(DisplayAttributes.BlackBrush));
		public static readonly DependencyProperty ThumbFocusBackgroundProperty =
			DependencyProperty.Register("FocusBackground", typeof(Brush), typeof(Thumb),
				new PropertyMetadata(DisplayAttributes.YellowBrush));
		public static readonly DependencyProperty ThumbFocusForegroundProperty =
			DependencyProperty.Register("FocusForeground", typeof(Brush), typeof(Thumb),
				new PropertyMetadata(DisplayAttributes.BlackBrush));
		public static readonly DependencyProperty ThumbSelectionBackgroundProperty =
			DependencyProperty.Register("SelectionBackground", typeof(Brush), typeof(Thumb),
				new PropertyMetadata(DisplayAttributes.YellowBrush));
		public static readonly DependencyProperty ThumbSelectionForegroundProperty =
			DependencyProperty.Register("SelectionForeground", typeof(Brush), typeof(Thumb),
				new PropertyMetadata(DisplayAttributes.BlackBrush));

		#endregion Fields 

		#region Properties (4) 

		/// <summary>
		/// Gets or sets the Brush to use for the background of a button in its focussed state.
		/// Default: Yellow
		/// </summary>
		/// <value>The focus background brush.</value>
		public Brush FocusBackground
		{
			get { return (Brush)GetValue(FocusBackgroundProperty); }
			set { SetValue(FocusBackgroundProperty, value); }
		}

		/// <summary>
		/// Gets or sets the Brush to use for the foreground of a button in its focussed state.
		/// Default: Black
		/// </summary>
		/// <value>The focus foreground brush.</value>
		public Brush FocusForeground
		{
			get { return (Brush)GetValue(FocusForegroundProperty); }
			set { SetValue(FocusForegroundProperty, value); }
		}

		/// <summary>
		/// Gets or sets the Brush to use for the background of a button in its pressed or MouseOver state.
		/// Default: Yellow
		/// </summary>
		/// <value>The selection background brush.</value>
		public Brush SelectionBackground
		{
			get { return (Brush)GetValue(SelectionBackgroundProperty); }
			set { SetValue(SelectionBackgroundProperty, value); }
		}

		/// <summary>
		/// Gets or sets the Brush to use for the foreground of a button in its pressed or MouseOver state.
		/// Default: Black
		/// </summary>
		/// <value>The selection foreground brush.</value>
		public Brush SelectionForeground
		{
			get { return (Brush)GetValue(SelectionForegroundProperty); }
			set { SetValue(SelectionForegroundProperty, value); }
		}

		#endregion Properties 

		#region Delegates and Events (2) 

		// Events (2) 

        /// <summary>
        /// Events thrown when slider thumb has been eleased
        /// </summary>
	    public event EventHandler<EventArgs> ThumbDragCompleted;

        /// <summary>
        /// Events thrown when slider thumb has been clicked, dragged
        /// </summary>
	    public event EventHandler<EventArgs> ThumbDragStarted;

		#endregion Delegates and Events 

		#region Methods (6) 

		// Public Methods (2) 

        public override void OnApplyTemplate()
        {
           base.OnApplyTemplate();

            Thumb thumb = this.GetTemplateChild("HorizontalThumb") as Thumb;
            if (thumb != null)
            {
                thumb.DragStarted += new DragStartedEventHandler(thumb_DragStarted);
                thumb.DragCompleted += new DragCompletedEventHandler(thumb_DragCompleted);
            }
        }

        public virtual void OnThumbDragStarted(object sender, EventArgs e)
        {
            if (ThumbDragStarted != null)
                ThumbDragStarted(sender, e);
        }
		// Protected Methods (2) 

		/// <summary>
		/// Overrides the KeyDown event to prevent the slider from intercepting key events.
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if(e.Key == Key.Home || e.Key == Key.End || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
			{
				return;
			}

			//Default Slider behaviour is to respond to Up, Down, Left, Right for changing value
			//Override this so it instead does not respond to key events

			base.OnKeyDown(e);
		}

        protected virtual void OnThumbDragCompleted(object sender, EventArgs e)
        {
            if (ThumbDragCompleted != null)
                ThumbDragCompleted(sender, e);
        }
		// Private Methods (2) 

        void thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            OnThumbDragCompleted(this, new EventArgs());
        }

        void thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            OnThumbDragStarted(this, new EventArgs());
        }

		#endregion Methods 
	}
}