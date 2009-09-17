using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Buttercup.Control.Resources;
using Buttercup.Control.UI;
using Buttercup.Control.UI.Extensions;
using Control.MVP;
using Control.UI;
using Control.UI.Utility;


namespace Buttercup.Control
{
	public partial class RescaleUIPopupContents : UserControl
	{
		#region Fields (1) 

		private KeyCommandHandler _keyCommandHandler;

		#endregion Fields 



		#region Constructors (1) 

		public RescaleUIPopupContents()
		{
			InitializeComponent();

			KeyDown += RescaleUIPopupContents_KeyDown;
			KeyUp += RescaleUIPopupContents_KeyUp;

			changeSizeSlider.ThumbDragCompleted += changeSizeSlider_ThumbDragCompleted;
			mainFrame.SizeChanged += mainFrame_SizeChanged;
		}

		#endregion Constructors 



		#region Properties (2) 

		internal int CurrentPercentage
		{
			get
			{
				if(changeSizeSlider.Minimum != 0)
				{
					return (int)(changeSizeSlider.Value * 100 / changeSizeSlider.Minimum);
				}
				return 0;
			}
		}

		public KeyCommandHandler KeyCommandHandler
		{
			get { return _keyCommandHandler; }
			set
			{
				_keyCommandHandler = value;
				_keyCommandHandler.KeyDown += _keyCommandHandler_KeyIsPressed;
				_keyCommandHandler.KeyUp += _keyCommandHandler_KeyIsReleased;
			}
		}

		#endregion Properties 



		#region Delegates and Events (5) 

		// Events (5) 

		/// <summary>
		/// Raised when the interface size is changed by the user.
		/// </summary>
		public event EventHandler<InterfaceSizeEventArgs> ChangeInterfaceSize;

		/// <summary>
		/// Raised when the popup is closed by the user.
		/// </summary>
		public event EventHandler ClosePopup;

		public event EventHandler<SpeechTextEventArgs> SelfVoicingSpeakText;

		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelected;

		/// <summary>
		/// Raised when the slider receives focus.
		/// </summary>
		public event EventHandler SpeakPercentage;

		#endregion Delegates and Events 



		#region Methods (23) 

		// Private Methods (20) 

		private void _keyCommandHandler_KeyIsPressed(object sender, KeyCommandEventArgs e)
		{
            Shortcut keysPressed = new Shortcut(e.keyEventArgs.PlatformKeyCode, e.keyEventArgs.Key, Keyboard.Modifiers);

			//Panel Specific Shortcuts

			//Find parent of the control sending this keypress to see if it is part of this views context.
			RescaleUIPopupContents popupContext = UIElementExtensions.GetUIParentOfType<RescaleUIPopupContents>(e.keyEventArgs.OriginalSource as UIElement);

			//If not null, then we are in the search context
			if(popupContext != null && e.SubViewHandled == false)
			{
				if(keysPressed.Equals(Shortcuts.ClosePanel))
				{
					closePopupButton_Click(sender, new RoutedEventArgs());
					e.SubViewHandled = true;
				}
				else if(keysPressed.Equals(Shortcuts.IncreaseInterfaceSize))
				{
					increaseSizeButton.Focus();
				}
				else if(keysPressed.Equals(Shortcuts.DecreaseInterfaceSize))
				{
					decreaseSizeButton.Focus();
				}
				else if(keysPressed.Equals(Shortcuts.ToggleSelfVoicing) || keysPressed.Equals(Shortcuts.StopSelfVoice) || keysPressed.Equals(Shortcuts.SpeakHelpText))
				{
					//let shortcuts can are not application panel related through?
				}
				else
				{
					//Treat this popup as if the application doesn't exist below, so block all other shortcuts
					e.SubViewHandled = true;
				}
			}
		}



		void _keyCommandHandler_KeyIsReleased(object sender, KeyCommandEventArgs e)
		{
            Shortcut keysPressed = new Shortcut(e.keyEventArgs.PlatformKeyCode, e.keyEventArgs.Key, Keyboard.Modifiers);

			//Find parent of the control sending this keypress to see if it is part of this views context.
			RescaleUIPopupContents popupContext = UIElementExtensions.GetUIParentOfType<RescaleUIPopupContents>(e.keyEventArgs.OriginalSource as UIElement);

			//If not null, then we are in the search context
			if(popupContext != null && e.SubViewHandled == false)
			{
				if(keysPressed.Equals(Shortcuts.IncreaseInterfaceSize))
				{
					if(SpeakPercentage != null)
					{
						SpeakPercentage(this, new EventArgs());
					}
					e.SubViewHandled = true;
				}
				if(keysPressed.Equals(Shortcuts.DecreaseInterfaceSize))
				{
					if(SpeakPercentage != null)
					{
						SpeakPercentage(this, new EventArgs());
					}
					e.SubViewHandled = true;
				}
			}
		}



		private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			//Avoid fall-through to close popup if a target element is missed.
			e.Handled = true;
		}



		/// <summary>
		/// Default handler raised when a button loses focus. Adjusts the foreground binding.
		/// </summary>
		private void Button_LostFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.HandleMouseOutLoseFocus(sender);
		}



		/// <summary>
		/// Default handler raised when the mouse enters a button's region. Adjusts the foreground binding.
		/// </summary>
		private void Button_MouseEnter(object sender, MouseEventArgs e)
		{
			CommonEventHandlerUtility.HandleMouseOver(sender);
		}



		/// <summary>
		/// Default handler raised when the mouse leaves a button's region. Adjusts the foreground binding.
		/// </summary>
		private void Button_MouseLeave(object sender, MouseEventArgs e)
		{
			CommonEventHandlerUtility.HandleMouseOutLoseFocus(sender);
		}



		private void changeSizeSlider_GotFocus(object sender, RoutedEventArgs e)
		{
			if(SelfVoicingSpeakText != null)
			{
				SelfVoicingSpeakText(this, new SpeechTextEventArgs(String.Format(Speech.InterfaceSizeSliderMessage, CurrentPercentage)));
			}
		}



		void changeSizeSlider_ThumbDragCompleted(object sender, EventArgs e)
		{
			if(SpeakPercentage != null)
			{
				SpeakPercentage(this, new EventArgs());
			}
		}



		/// <summary>
		/// Raised when the interface-size slider's value is changed. This delegates to the view that has
		/// subscribed to this event (i.e. the DisplaySettings view).
		/// </summary>
		/// <remarks>This entire control is intended to be hosted in the main view, in order to
		/// occupy the entire application area.</remarks>
		private void changeSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(changeSizeSlider != null)
			{
				int value = (int)e.NewValue;

				if(ChangeInterfaceSize != null)
				{
					ChangeInterfaceSize(this, new InterfaceSizeEventArgs(value));
				}
			}
		}



		private void closePopupButton_Click(object sender, RoutedEventArgs e)
		{
			PopupNoFocus.Focus();
			InitiateClosePopup();
		}



		//Trap the 'Tab' key on this control to trap tab-ordering in the popup
		private void closePopupButton_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.None)
			{
				decreaseSizeButton.Focus();
				e.Handled = true;
			}
		}



		/// <summary>
		/// Raised when the Decrease Size button is clicked. This will change the slider's value, which
		/// automatically raises the ValueChanged event and therefore invokes that event handler.
		/// </summary>
		private void decreaseSizeButton_Click(object sender, RoutedEventArgs e)
		{
			DecreaseSize();
		}



		//Trap the 'Shift+Tab' key on this control to trap tab-ordering in the popup
		private void decreaseSizeButton_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.Shift)
			{
				closePopupButton.Focus();
				e.Handled = true;
			}
		}



		/// <summary>
		/// Raised when the Increase Size button is clicked. This will change the slider's value, which
		/// automatically raises the ValueChanged event and therefore invokes that event handler.
		/// </summary>
		private void increaseSizeButton_Click(object sender, RoutedEventArgs e)
		{
			IncreaseSize();
		}



		private void InitiateClosePopup()
		{
			if(ClosePopup != null)
			{
				ClosePopup(this, new EventArgs());
			}
		}



		void mainFrame_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ScaleTransform mainTransform = mainFrame.RenderTransform as ScaleTransform;

			mainTransform.CenterX = mainFrame.ActualWidth / 2;
			mainTransform.CenterY = mainFrame.ActualHeight / 2;
		}



		private void OuterPopup_Click(object sender, MouseButtonEventArgs e)
		{
			InitiateClosePopup();
		}



		void RescaleUIPopupContents_KeyDown(object sender, KeyEventArgs e)
		{
			//might need to raise it's own key press events
			if(e.Handled == false)
			{
				KeyCommandHandler.KeyPressed(new KeyCommandEventArgs(e));
			}
		}



		void RescaleUIPopupContents_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.Handled == false)
			{
				KeyCommandHandler.KeyReleased(new KeyCommandEventArgs(e));
			}
		}



		/// <summary>
		/// Raised when the interface-size slider receives focus, in order to activate self-voicing.
		/// </summary>
		private void SpeakElementOnFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.HandleGetFocus(sender);

			if(SpeakableElementSelected != null)
			{
				SpeakableElementSelected(this, new ElementSelectedEventArgs(e.OriginalSource as DependencyObject));
			}
		}



		// Internal Methods (3) 

		/// <summary>
		/// Decreases the size of the UI elements by a small step.
		/// </summary>
		internal void DecreaseSize()
		{
			double newValue = changeSizeSlider.Value - changeSizeSlider.SmallChange;
			changeSizeSlider.Value = Math.Max(changeSizeSlider.Minimum, newValue);
		}



		/// <summary>
		/// Increases the size of the UI elements by a small step.
		/// </summary>
		internal void IncreaseSize()
		{
			double newValue = changeSizeSlider.Value + changeSizeSlider.SmallChange;
			changeSizeSlider.Value = Math.Min(changeSizeSlider.Maximum, newValue);
		}



		/// <summary>
		/// Updates the reported interface size in the display.
		/// </summary>
		/// <param name="value">The interface size value.</param>
		internal void UpdateInterfaceSize(int value)
		{
			//Don't want the event to fire to re-update this in the Model.
			changeSizeSlider.ValueChanged -= changeSizeSlider_ValueChanged;
			changeSizeSlider.Value = value;
			changeSizeSlider.ValueChanged += changeSizeSlider_ValueChanged;
		}

		#endregion Methods 
	}
}
