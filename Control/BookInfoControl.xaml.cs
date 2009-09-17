using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Control.UI.Utility;


namespace Buttercup.Control
{
	public partial class BookInfoControl : UserControl
	{
		#region Constructors (1) 

		public BookInfoControl()
		{
			InitializeComponent();
		}

		#endregion Constructors 



		#region Methods (3) 

		// Protected Methods (1) 

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new BookInfoAutomationPeer(this);
		}



		// Private Methods (2) 

		/// <summary>
		/// Raised when this control receives focus. This is to allow the focus state
		/// to be visually emphasised.
		/// </summary>
		private void ControlGotFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.ChangeFocusBorder(mainFrame, true);
		}



		/// <summary>
		/// Raised when this control loses focus. This is to allow the previously focussed
		/// Listbox to be rendered in its default state.
		/// </summary>
		private void ControlLostFocus(object sender, RoutedEventArgs e)
		{
			CommonEventHandlerUtility.ChangeFocusBorder(mainFrame, false);
		}

		#endregion Methods 
	}
}