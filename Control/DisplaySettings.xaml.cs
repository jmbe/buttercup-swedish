using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Buttercup.Control.Model;
using Buttercup.Control.Resources;
using Buttercup.Control.UI;
using Buttercup.Control.UI.Extensions;
using Control.MVP;
using Control.MVP.Views;
using Control.UI;
using Control.UI.Utility;
using System.Windows.Data;


namespace Buttercup.Control
{
	public partial class DisplaySettings : UserControl, IDisplaySettingsView
	{
		#region Fields (3) 

		private KeyCommandHandler _keyCommandHandler;
		private Main _mainViewReference;
		private RescaleUIPopupContents _rescaleUIPopupContentsReference;

		#endregion Fields 



		#region Constructors (1) 

		public DisplaySettings()
		{
			InitializeComponent();
			Loaded += DisplaySettings_Loaded;

            KeyDown += DisplaySettings_KeyDown;
            KeyUp += DisplaySettings_KeyUp;

		}

		#endregion Constructors 



		#region Properties (2) 

		public KeyCommandHandler KeyCommandHandler
		{
			get { return _keyCommandHandler; }
			set
			{
				_keyCommandHandler = value;
				_keyCommandHandler.KeyDown += _keyCommandHandler_KeyIsPressed;
			}
		}

		/// <summary>
		/// Gets or sets the reference to the containing main view.
		/// </summary>
		/// <value>The reference to the containing main view.</value>
		internal Main MainViewReference
		{
			get { return _mainViewReference; }
			set
			{
				//Disconnect handlers from old reference
				if(_rescaleUIPopupContentsReference != null)
				{
					_rescaleUIPopupContentsReference.ChangeInterfaceSize -= InterfaceSizeValueChangedInUI;
					_rescaleUIPopupContentsReference.SpeakPercentage -= SpeakPercentage;
				}

				_mainViewReference = value;

				//Connect handlers for new reference
				if(_mainViewReference != null)
				{
					_rescaleUIPopupContentsReference = _mainViewReference.RescaleUIPopupContents;
					if(_rescaleUIPopupContentsReference != null)
					{
						_rescaleUIPopupContentsReference.ChangeInterfaceSize += InterfaceSizeValueChangedInUI;
						_rescaleUIPopupContentsReference.SpeakableElementSelected += _rescaleUIPopupContentsReference_SpeakableElementSelected;
						_rescaleUIPopupContentsReference.SelfVoicingSpeakText += _rescaleUIPopupContentsReference_SelfVoicingSpeakText;
						_rescaleUIPopupContentsReference.SpeakPercentage += SpeakPercentage;
					}
				}
			}
		}

        public int FocusedItem { get; set; }

        public IApplicationView ApplicationView { get; set; }

		#endregion Properties 



		#region Delegates and Events (5) 

		// Events (5) 

		/// <summary>
		/// Raised when the contrast level is changed by the user.
		/// </summary>
		public event EventHandler<ContrastSchemeEventArgs> ChangeContrastScheme;

		/// <summary>
		/// Raised when the interface size is changed by the user.
		/// </summary>
		public event EventHandler<InterfaceSizeEventArgs> ChangeInterfaceSize;

		public event EventHandler<SpeechTextEventArgs> SelfVoicingSpeakText;

		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelected;

		/// <summary>
		/// Raised when a UI element's help text is required to be spoken.
		/// </summary>
		public event EventHandler<ElementSelectedEventArgs> SpeakableElementSelectedHelpText;

		#endregion Delegates and Events 



		#region Methods (21) 

		// Public Methods (2) 

		/// <summary>
		/// Updates the reported contrast scheme in the display settings. Called by the Presenter.
		/// </summary>
		/// <param name="contrastScheme">The contrast scheme.</param>
		public void UpdateContrastScheme(ContrastLevel contrastScheme)
		{
			ListBoxItem selectedItem = (ListBoxItem)contrastSchemeSelector.Items.FirstOrDefault(
				currentItem => ((ListBoxItem)currentItem).Tag.ToString() == contrastScheme.ToString());

			//Don't want the event to fire to re-update this in the Model.
			contrastSchemeSelector.SelectionChanged -= contrastSchemeSelectionChanged;
			if(selectedItem != null)
			{
				contrastSchemeSelector.SelectedItem = selectedItem;
			}
			contrastSchemeSelector.SelectionChanged += contrastSchemeSelectionChanged;
		}



		/// <summary>
		/// Updates the reported interface size in the display. Called by the Presenter.
		/// </summary>
		/// <param name="value">The interface size value.</param>
		public void UpdateInterfaceSize(int value)
		{
			if(MainViewReference != null)
			{
				MainViewReference.RescaleUIPopupContents.UpdateInterfaceSize(value);
			}
		}



		// Private Methods (19) 

		/// <summary>
		/// Raised when a key is pressed while this panel (or one of its child controls) has focus.
		/// </summary>
		void _keyCommandHandler_KeyIsPressed(object sender, KeyCommandEventArgs e)
		{
            Shortcut keysPressed = new Shortcut(e.keyEventArgs.PlatformKeyCode, e.keyEventArgs.Key, Keyboard.Modifiers);

			//Panel Specific Shortcuts

			//Find parent of the control sending this keypress to see if it is part of this views context.
			DisplaySettings displayContext = UIElementExtensions.GetUIParentOfType<DisplaySettings>(e.keyEventArgs.OriginalSource as UIElement);

			//If not null, then we are in the display settings context
			if(displayContext != null && e.SubViewHandled == false)
			{
                //Block up, down, left, right when inside the listboxes on this view
                if (e.keyEventArgs.OriginalSource.ToString() == KeyCommandHandler.ListBoxItem)
                {
                    if (keysPressed.Equals(Shortcuts.PreviousHeader) || keysPressed.Equals(Shortcuts.NextHeader) || keysPressed.Equals(Shortcuts.PreviousSection) ||
                        keysPressed.Equals(Shortcuts.NextSection))
                    {
                        e.SubViewHandled = true;
                    }
                }

				//Tell main not to navigate on Up,Down,Left,Right for the slider control
				if(e.keyEventArgs.OriginalSource.ToString() == KeyCommandHandler.SliderControl)
				{
					if(keysPressed.Equals(Shortcuts.PreviousHeader) || keysPressed.Equals(Shortcuts.NextHeader) || keysPressed.Equals(Shortcuts.PreviousSection) ||
						keysPressed.Equals(Shortcuts.NextSection))
					{
						e.SubViewHandled = true;
					}
				}

                if (keysPressed.Equals(Shortcuts.DisplaySettingsPanel))
                {
                    if (FocusManager.GetFocusedElement() != changeSizeButton)
                    {
                        changeSizeButton.Focus();
                        e.SubViewHandled = true;
                    }
                }
                if (keysPressed.Equals(Shortcuts.Contrast))
                {
                    if (UIElementExtensions.GetUIParentOfType<MediaListBox>(FocusManager.GetFocusedElement() as UIElement) != contrastSchemeSelector)
                    {
                        contrastSchemeSelector.Focus();
                        e.SubViewHandled = true;
                    }
                }
			}
		}



		/// <summary>
		/// Redirects speaking of elements through the display settings
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _rescaleUIPopupContentsReference_SelfVoicingSpeakText(object sender, SpeechTextEventArgs e)
		{
			SelfVoicingSpeakText(sender, e);
		}



		/// <summary>
		/// Redirects speaking of elements through the display settings
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _rescaleUIPopupContentsReference_SpeakableElementSelected(object sender, ElementSelectedEventArgs e)
		{
			SpeakableElementSelected(sender, e);
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



		private void changeSizeButton_Click(object sender, RoutedEventArgs e)
		{
			if(MainViewReference != null)
			{
				MainViewReference.ShowRescalePopup();
			}

			//Wire up the closed event to return focus
			if(_rescaleUIPopupContentsReference != null)
			{
				_rescaleUIPopupContentsReference.ClosePopup += RescaleUIPopupClosed;
			}
		}



		/// <summary>
		/// Raised when the selected contrast scheme (in the ListBox) is changed.
		/// </summary>
		private void contrastSchemeSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(contrastSchemeSelector != null)	//Need to focus the parent listbox
				contrastSchemeSelector.Focus();

			ContrastLevel contrastScheme = ContrastLevel.WhiteTextOnBlue;
			ListBoxItem selectedItem = ((ListBox)sender).SelectedItem as ListBoxItem;

			try
			{
				if(selectedItem != null)
				{
					contrastScheme = (ContrastLevel)Enum.Parse(typeof(ContrastLevel),
						selectedItem.Tag.ToString(), true);
				}
			}
			catch(Exception)
			{
				//Contrast scheme will remain the default.
			}

			if(ChangeContrastScheme != null)
			{
				ChangeContrastScheme(this, new ContrastSchemeEventArgs(contrastScheme));
			}
		}


        /// <summary>
        /// Capture KeyDown events, needed in each subview as the Scrollviewer control
        /// that the panels are situated in will not let certain keypresses through
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		void DisplaySettings_KeyDown(object sender, KeyEventArgs e)
		{
            ScrollViewer panelScrollViewer = UIElementExtensions.GetUIParentOfType<ScrollViewer>(e.OriginalSource as UIElement);

			//If we are not in a scrollviewer, or the scrollviewer has a vertical scrollbar visible, then arrows won't be let through
            if (panelScrollViewer == null || panelScrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            {
                if (e.Handled == false &&
                    (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.PageUp || e.Key == Key.PageDown || e.Key == Key.Home ||
                     e.Key == Key.End))
                {
                    KeyCommandHandler.KeyPressed(new KeyCommandEventArgs(e));
                }
            }
		}


        /// <summary>
        /// Capture KeyUp events, needed in each subview as the Scrollviewer control
        /// that the panels are situated in will not let certain keypresses through
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		void DisplaySettings_KeyUp(object sender, KeyEventArgs e)
		{
            ScrollViewer panelScrollViewer = UIElementExtensions.GetUIParentOfType<ScrollViewer>(e.OriginalSource as UIElement);

            //If we are not in a scrollviewer, or the scrollviewer has a vertical scrollbar visible, then arrows won't be let through
            if (panelScrollViewer == null || panelScrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            {
                if (e.Handled == false &&
                    (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.PageUp || e.Key == Key.PageDown || e.Key == Key.Home ||
                     e.Key == Key.End))
                {
                    KeyCommandHandler.KeyReleased(new KeyCommandEventArgs(e));
                }
            }
		}



		void DisplaySettings_Loaded(object sender, RoutedEventArgs e)
		{
            //Select the right tab depending on the set focus
            if (FocusedItem == (int)DisplaySettingsState.Focus.ChangeSize)
            {
                changeSizeButton.Focus();
            }
            if (FocusedItem == (int)DisplaySettingsState.Focus.Contrast)
            {
                contrastSchemeSelector.Focus();
            }

			ApplicationVoice.MainInstance.QueueTextToSpeak(Speech.DisplaySettingsPanelMessage);
		}



		private void DoSelfVoicingSpeakText(string message)
		{
			if(SelfVoicingSpeakText != null)
			{
				SelfVoicingSpeakText(this, new SpeechTextEventArgs(message));
			}
		}



		/// <summary>
		/// Raised when the interface-size slider's value is changed, to initiate system-wide interface-scaling
		/// via the Presenter.
		/// </summary>
		private void InterfaceSizeValueChangedInUI(object sender, InterfaceSizeEventArgs e)
		{
			if(ChangeInterfaceSize != null)
			{
				ChangeInterfaceSize(this, new InterfaceSizeEventArgs(e.InterfaceSize));
			}
		}



		/// <summary>
		/// Raised when the source ListBox receives focus. This is to allow the focus state
		/// to be visually emphasised.
		/// </summary>
		private void ListBoxGotFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.ChangeFocusBorder(sender, true);
		}



		/// <summary>
		/// Raised when the source ListBox loses focus. This is to allow the previously focussed
		/// Listbox to be rendered in its default state.
		/// </summary>
		private void ListBoxLostFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.ChangeFocusBorder(sender, false);
		}



		/// <summary>
		/// Raised when the popup is closed. The popup contents reference is guaranteed to be non-null.
		/// This will only be called when the popup is shown via the Change Size button, to restore focus.
		/// </summary>
		private void RescaleUIPopupClosed(object sender, EventArgs e)
		{
			//Unwire the closed event now that we are finished with it.
			_rescaleUIPopupContentsReference.ClosePopup -= RescaleUIPopupClosed;
			changeSizeButton.Focus();
		}



		private void SpeakContrastSchemesOnFocus(object sender, RoutedEventArgs e)
		{
			//Since having a TextBlock inside a Border as the content of a ListBoxItem totally kills
			//being able to set the AutomationProperties.Name of that ListBoxItem... we'll do things the silly
			//manual way

			string contrastSchemeText = "Contrast: White Text on Blue Background";

			if((e.OriginalSource as ExtendedListBoxItem).Tag as string == "BlackTextOnWhite")
			{
				contrastSchemeText = "Contrast: Black Text on White Background";
			}

			if((e.OriginalSource as ExtendedListBoxItem).Tag as string == "YellowTextOnBlack")
			{
				contrastSchemeText = "Contrast: Yellow Text on Black Background";
			}

			if(SelfVoicingSpeakText != null)
			{
				SelfVoicingSpeakText(this, new SpeechTextEventArgs(contrastSchemeText));
			}
		}



		/// <summary>
		/// Handles the GotFocus event of a button. This is used to initiate speaking the voice command.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e"></param>
		private void SpeakControlOnFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.HandleGetFocus(sender);

			if(SpeakableElementSelected != null)
			{
				SpeakableElementSelected(this, new ElementSelectedEventArgs(sender as DependencyObject));
			}
		}



		/// <summary>
		/// Speaks the percentage of the change size slider
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SpeakPercentage(object sender, EventArgs e)
		{
			int percentage = 0;
			if(_rescaleUIPopupContentsReference != null)
			{
				percentage = _rescaleUIPopupContentsReference.CurrentPercentage;
			}

			DoSelfVoicingSpeakText(String.Format(Speech.InterfaceSizeSliderPercentage, percentage));
		}

		#endregion Methods 
	}
}
