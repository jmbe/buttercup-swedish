using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Buttercup.Control.UI.ViewModel;
using Control.UI;


namespace Buttercup.Control
{
	public class MediaTextBox : TextBox
	{
		#region Fields (2) 

		public static readonly DependencyProperty FocusBackgroundProperty =
			DependencyProperty.Register("FocusBackground", typeof(Brush), typeof(MediaTextBox),
				new PropertyMetadata(DisplayAttributes.DarkBlueBrush));

		public static readonly DependencyProperty FocusForegroundProperty =
			DependencyProperty.Register("FocusForeground", typeof(Brush), typeof(MediaTextBox),
				new PropertyMetadata(DisplayAttributes.YellowBrush));

		#endregion Fields 



		#region Properties (3) 

		/// <summary>
		/// Gets or sets the Brush to use for the background of a text box in its focussed state.
		/// Default: Dark Blue
		/// </summary>
		/// <value>The focus background brush.</value>
		public Brush FocusBackground
		{
			get { return (Brush)GetValue(FocusBackgroundProperty); }
			set { SetValue(FocusBackgroundProperty, value); }
		}

		/// <summary>
		/// Gets or sets the Brush to use for the foreground of a text box in its focussed state.
		/// Default: Yellow
		/// </summary>
		/// <value>The focus foreground brush.</value>
		public Brush FocusForeground
		{
			get { return (Brush)GetValue(FocusForegroundProperty); }
			set { SetValue(FocusForegroundProperty, value); }
		}

		public char LastCharDeleted { get; private set; }

		#endregion Properties 



		#region Methods (1) 

		// Protected Methods (1) 

		/// <summary>
		/// Overrides the KeyDown event to allow the Backspace key to initiate self-voicing of deleted characters.
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			//We want to use Key.Multiply for contextual self-voice help
			if(e.Key == Shortcuts.SpeakHelpText.ShortcutKey || e.Key == Shortcuts.ClosePanel.ShortcutKey)
			{
				return;
			}
			if(e.Key == Key.Back)
			{
				if(Text.Length > 0)
				{
					LastCharDeleted = Text[Text.Length - 1];
				}
				else
				{
					LastCharDeleted = (char)0;
				}
			}

			base.OnKeyDown(e);

			if(e.Key == Key.Back)
			{
				e.Handled = false;
			}
		}

		#endregion Methods 
	}
}