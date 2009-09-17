using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Buttercup.Control
{
	public class MediaListBox : ListBox
	{
        public FrameworkElement NextElement { get; set; }

		#region Methods (1) 

		/// <summary>
		/// Overrides the KeyDown event to bypass the default handling of the following keys:
		/// Space (select), Page Up and Page Down (page up and down within listbox)
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if(e.Key == Key.Space || e.Key == Key.PageUp || e.Key == Key.PageDown
					|| e.Key == Key.Home || e.Key == Key.End)
			{
				return;
			}

            if (e.Key == Key.Enter)
            {
                //TODO: Is there a more elegant way of reselecting the same selecteditem without having to set it to something else first?
                int selectedIndex = SelectedIndex;
                SelectedIndex = -1;
                SelectedIndex = selectedIndex;
            }

			base.OnKeyDown(e);
		}

        /// <summary>
        /// The list boxes we use will auto-apply/activate any action selected in the list box
        /// So there should be no distinction between a Selected Item and a Focused Item.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(System.Windows.RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if ((SelectedItem as ListBoxItem) != null)
                (SelectedItem as ListBoxItem).Focus();
        }
		#endregion Methods 
	}
}