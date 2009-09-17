using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;


namespace Buttercup.Control.UI.Extensions
{
	public static class FrameworkElementExtensions
	{
		#region Methods (3) 

		// Public Methods (3) 

		/// <summary>
		/// Changes the foreground binding of the given element to the specified binding.
		/// This is used to set the foreground of the button content when the button state changes
		/// so that contrast is maximised.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="targetBrushBinding">The target brush binding.</param>
		public static void ChangeForegroundBinding(this FrameworkElement element, Binding targetBrushBinding)
		{
			if(element is Path)
			{
				(element).SetBinding(Shape.FillProperty, targetBrushBinding);
			}
			else if(element is TextBlock)
			{
				(element).SetBinding(TextBlock.ForegroundProperty, targetBrushBinding);
			}
		}



		/// <summary>
		/// Determines whether the given FrameworkElement is fully rendered in the parent viewport.
		/// This will return true only if the entire element is rendered (at lower sizes), or if the
		/// element occupies the full viewport area.
		/// </summary>
		/// <param name="element">The element to check.</param>
		/// <param name="parent">The parent ScrollViewer containing viewport information.</param>
		/// <returns>
		/// True if the given FrameworkElement is rendered in the parent viewport, otherwise false.
		/// </returns>
		/// <remarks>The main consideration is the relative vertical offsets and heights,
		/// since that is the sole dimension that is positioned.</remarks>
		public static bool IsInView(this FrameworkElement element, ScrollViewer parent, double scale)
		{
			if(parent == null)
				return false;

			//Silverlight should have taken the ScrollViewer padding (and hence ScrollContentPresenter
			// margin) into account when calculating the ViewportHeight.
			double topPadding = parent.Padding.Top;
			double bottomPadding = parent.Padding.Bottom;

			Point topLeft = element.TopLeft(parent);
			//The element's allocated area within the ScrollViewer, in absolute co-ordinates
			Rect elementRect = new Rect(topLeft.X, topLeft.Y + parent.VerticalOffset,
					element.ActualWidth, element.ActualHeight * scale);
			//The coverage area of the current viewport in the ScrollViewer, in absolute co-ordinates.
			Rect parentRect = new Rect(0, parent.VerticalOffset + topPadding, parent.ViewportWidth,
				Math.Max(parent.ViewportHeight - topPadding - bottomPadding, 0.0));

			if(elementRect.Height > parentRect.Height)
				//Superfull coverage - element cannot fit entirely in viewport.
				return (parentRect.Top <= elementRect.Top && parentRect.Bottom >= elementRect.Bottom);

			//Element can fit entirely in viewport
			return (elementRect.Top >= parentRect.Top && elementRect.Bottom <= parentRect.Bottom);
		}



		public static void ScrollIntoView(this FrameworkElement element, ScrollViewer scrollViewerObj, double scale)
		{
			//Don't need to worry about scrolling if the ScrollViewer is null or vertically unscrollable.
			if(scrollViewerObj == null || scrollViewerObj.ComputedVerticalScrollBarVisibility != Visibility.Visible)
				return;

			FrameworkElement containedObject = element;
			bool isInView = false;
			//Silverlight should have taken the ScrollViewer padding (and hence ScrollContentPresenter
			// margin) into account when calculating the ViewportHeight.
			double verticalPadding = scrollViewerObj.Padding.Top + scrollViewerObj.Padding.Bottom;

			try
			{
				isInView = containedObject.IsInView(scrollViewerObj, scale);
			}
			catch(Exception)
			{
				//Don't do any transform - element is not a child of the scrollviewer / element is invisible.
				return;
			}

			if(!isInView) 
			{
				//Initially-calculated offset is simply the top of the element relative to the entire scrollable area.
				double calculatedOffset = (containedObject.TopLeft(scrollViewerObj).Y + scrollViewerObj.VerticalOffset);

				if(element.ActualHeight * scale <= scrollViewerObj.ViewportHeight - verticalPadding)
					calculatedOffset -= verticalPadding;

				//Scrollable height is simply the difference between extent and viewport.
				double maximumOffset = scrollViewerObj.ScrollableHeight;

				calculatedOffset = Math.Max(0, Math.Min(calculatedOffset, maximumOffset));
				scrollViewerObj.ScrollToVerticalOffset(calculatedOffset);
			}
		}

		#endregion Methods 
	}
}