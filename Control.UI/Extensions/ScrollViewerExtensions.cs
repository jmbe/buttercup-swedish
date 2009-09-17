using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using System.Windows.Media;


namespace Buttercup.Control.UI.Extensions
{
	public static class ScrollViewerExtensions
	{
		#region Methods (3) 


		/// <summary>
		/// Adjusts the scale of the ScrollViewer's contents.
		/// This requires the horizontal scrollbar to be disabled, i.e. the content has a limited width.
		/// The content should not have any other transform (including TransformGroup) apart from
		/// a ScaleTransform.
		/// </summary>
		/// <param name="scrollViewer">The ScrollViewer whose content should be scaled.</param>
		/// <param name="scale">The scaling factor, which must be greater than zero.</param>
		public static void AdjustContentScale(this ScrollViewer scrollViewer, double scale)
		{
			FrameworkElement content = scrollViewer.Content as FrameworkElement;
			if(content == null)
				return;

			// For the root.RenderSize to show the correct values after the
			// transformation (for some reason!) we need to reset the content's dimensions
			content.Width = 0;
			content.Height = Double.NaN;

			// Transform the content
			ScaleTransform currentScaleTransform = content.RenderTransform as ScaleTransform;
			if(currentScaleTransform == null)
			{
				currentScaleTransform = new ScaleTransform();
				content.RenderTransform = currentScaleTransform;
			}
			currentScaleTransform.ScaleY = scale;
			currentScaleTransform.ScaleX = scale;

			// Before updating the scroller layout set the width of the
			// content to the scaled scroller width
			content.Width = (scale > 0)? scrollViewer.ViewportWidth / scale : 0;

			// By updating the scroller layout the content's RenderSize
			// will reflect the new (unscaled) height.
			scrollViewer.UpdateLayout();
			content.Height = content.RenderSize.Height * scale;
		}

		#endregion Methods 

	
	}
}