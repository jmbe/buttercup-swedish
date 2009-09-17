using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Buttercup.Control.UI.ViewModel;


namespace Buttercup.Control
{
	public class MediaButton : Button
	{
		#region Fields (4) 

		public static readonly DependencyProperty SelectionBackgroundProperty =
			DependencyProperty.Register("SelectionBackground", typeof(Brush), typeof(MediaButton),
				new PropertyMetadata(DisplayAttributes.YellowBrush));

		public static readonly DependencyProperty SelectionForegroundProperty =
			DependencyProperty.Register("SelectionForeground", typeof(Brush), typeof(MediaButton),
				new PropertyMetadata(DisplayAttributes.BlackBrush));

		public static readonly DependencyProperty FocusBackgroundProperty =
			DependencyProperty.Register("FocusBackground", typeof(Brush), typeof(MediaButton),
				new PropertyMetadata(DisplayAttributes.YellowBrush));

		public static readonly DependencyProperty FocusForegroundProperty =
			DependencyProperty.Register("FocusForeground", typeof(Brush), typeof(MediaButton),
				new PropertyMetadata(DisplayAttributes.BlackBrush));

		#endregion Fields 



		#region Properties (4) 

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

		#endregion Properties 



		#region Methods (1) 

		// Protected Methods (1) 

		/// <summary>
		/// Overrides the KeyDown event to allow keyboard events to bubble up, while causing Enter to activate
		/// the button.
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			//Default ButtonBase behaviour is to call the ButtonClicked method and set KeyEventArgs->Handled to true
			//Override this so it instead does not respond to key events

			//However, we are allowing 'Enter' to activate buttons instead of 'Space'
			//Since we cannot invoke the base class event directly, use a ButtonAutomationPeer
			//to perform the 'Click'.
			if(e.Key == Key.Enter)
			{
				// Create a ButtonAutomationPeer using the ClickButton
				ButtonAutomationPeer buttonAutoPeer = new ButtonAutomationPeer(this);
				// Create an InvokeProvider
				IInvokeProvider invokeProvider =
					buttonAutoPeer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
				// Invoke the default event, which is click for the button
				invokeProvider.Invoke();
			
            }
		}

		#endregion Methods 
	}
}