using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Buttercup.Control.UI.Extensions;
using SysControls = System.Windows.Controls;


namespace Control.UI.Utility
{
	public class CommonEventHandlerUtility
	{
		#region Fields (4) 

		private static Binding _defaultForegroundBinding;
		private static Binding _focusBorderBrushBinding;
		private static Binding _focusForegroundBinding;
		private static Binding _selectionForegroundBinding;

		#endregion Fields 



		#region Constructors (1) 

		static CommonEventHandlerUtility()
		{
			_focusForegroundBinding = new Binding("SelectionForeground");
			_focusBorderBrushBinding = new Binding("SelectionBorderBrush");
			_selectionForegroundBinding = new Binding("HighlightForeground");
			_defaultForegroundBinding = new Binding("DefaultForeground");
		}

		#endregion Constructors 



		#region Methods (6) 

		// Public Methods (5) 

		/// <summary>
		/// Changes the given control's border based on whether it has focus.
		/// </summary>
		/// <param name="sender">The control cast as an object.</param>
		/// <param name="isFocussed">Indicates whether or not the control has focus.</param>
		public static void ChangeFocusBorder(object sender, bool isFocussed)
		{
			System.Windows.Controls.Control control = sender as System.Windows.Controls.Control;
			if(control == null)
			{
				return;
			}

			if(isFocussed)
			{
				control.SetBinding(System.Windows.Controls.Control.BorderBrushProperty, _focusBorderBrushBinding);
			}
			else
			{
				control.SetBinding(System.Windows.Controls.Control.BorderBrushProperty, _defaultForegroundBinding);
			}
		}



		/// <summary>
		/// Changes the given control's border based on whether it has focus.
		/// </summary>
		/// <param name="sender">The target border.</param>
		/// <param name="isFocussed">Indicates whether or not the control has focus.</param>
		public static void ChangeFocusBorder(Border target, bool isFocussed)
		{
			if(target == null)
			{
				return;
			}

			if(isFocussed)
			{
				target.SetBinding(Border.BorderBrushProperty, _focusBorderBrushBinding);
			}
			else
			{
				target.SetBinding(Border.BorderBrushProperty, _defaultForegroundBinding);
			}
		}



		/// <summary>
		/// Raised when a particular content control gets focus.
		/// This changes the foreground binding of its content accordingly.
		/// </summary>
		public static void HandleGetFocus(object sender)
		{
			FrameworkElement content = GetChildElement(sender);
			if(content != null)
			{
				content.ChangeForegroundBinding(_focusForegroundBinding);
			}
		}



		/// <summary>
		/// Raised when a particular content control loses focus or experiences a MouseOut event.
		/// This changes the foreground binding of its content accordingly.
		/// </summary>
		public static void HandleMouseOutLoseFocus(object sender)
		{
			FrameworkElement content = GetChildElement(sender);
			if(content != null)
			{
				content.ChangeForegroundBinding(_defaultForegroundBinding);
			}
		}



		/// <summary>
		/// Raised when a particular content control experiences a MouseOver event.
		/// This changes the foreground binding of its content accordingly.
		/// </summary>
		public static void HandleMouseOver(object sender)
		{
			FrameworkElement content = GetChildElement(sender);
			if(content != null)
			{
				content.ChangeForegroundBinding(_selectionForegroundBinding);
			}
		}



		// Private Methods (1) 

		/// <summary>
		/// Gets the child element as a FrameworkElement from the given (uncast) source control.
		/// </summary>
		/// <param name="sourceObject">The source control as an object.</param>
		/// <returns>The source control's content as a FrameworkElement.</returns>
		private static FrameworkElement GetChildElement(object sourceObject)
		{
			ContentControl source = sourceObject as ContentControl;
			if(source == null)
			{
				return null;
			}

			return source.Content as FrameworkElement;
		}

		#endregion Methods 
	}
}