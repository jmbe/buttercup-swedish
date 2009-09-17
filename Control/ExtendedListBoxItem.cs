using System.Windows.Input;
using Buttercup.Control.UI.ViewModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;


namespace Buttercup.Control
{
	/// <summary>
	/// Extends the ListBoxItem to include properties for selected item brushes.
	/// </summary>
	public class ExtendedListBoxItem : ListBoxItem
	{
		#region Fields (2) 

		public static readonly DependencyProperty SelectionBackgroundProperty =
			DependencyProperty.Register("SelectionBackground", typeof(Brush), typeof(ExtendedListBoxItem),
				new PropertyMetadata(DisplayAttributes.BlackBrush));

		public static readonly DependencyProperty SelectionForegroundProperty =
			DependencyProperty.Register("SelectionForeground", typeof(Brush), typeof(ExtendedListBoxItem),
				new PropertyMetadata(DisplayAttributes.WhiteBrush));

		#endregion Fields 



		#region Properties (2) 

		/// <summary>
		/// Gets or sets the Brush to use for the background of the selected item in the list.
		/// Default: Black
		/// </summary>
		/// <value>The selection background brush.</value>
		public Brush SelectionBackground
		{
			get {return (Brush)GetValue(SelectionBackgroundProperty);}
			set {SetValue(SelectionBackgroundProperty, value);}
		}

		/// <summary>
		/// Gets or sets the Brush to use for the foreground of the selected item in the list.
		/// Default: White
		/// </summary>
		/// <value>The selection foreground brush.</value>
		public Brush SelectionForeground
		{
			get {return (Brush)GetValue(SelectionForegroundProperty);}
			set {SetValue(SelectionForegroundProperty, value);}
		}

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                base.OnKeyDown(e);
            }
            else
            {
                base.OnKeyDown(e);
            }
        }


		#endregion Properties 
	}
}