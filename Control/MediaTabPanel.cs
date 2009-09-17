using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;


namespace Buttercup.Control
{
	public class MediaTabControl : TabControl
	{
		#region Methods (1) 

		/// <summary>
		/// Overrides the KeyDown event to prevent the TabControl from intercepting key events.
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			//Default Slider behaviour is to respond to Up, Down, Left, Right for changing value
			//Override this so it instead does not respond to key events
		}

		#endregion Methods 
	}
}