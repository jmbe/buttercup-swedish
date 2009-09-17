using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Buttercup.Control
{
	/// <summary>
	/// Adapted from the ViewBox implementation for Silverlight 2
	/// accessed 14/01/2009 from http://cid-123ec1ed6c72a14a.skydrive.live.com/self.aspx/Public/SL2Samples.zip
	/// </summary>
	public class TopCentredViewBox : Panel
	{
		#region Fields (2) 

		private Size _desiredSize;
		private ScaleTransform _scale;

		#endregion Fields 



		#region Methods (3) 

		/// <summary>
		/// Arranges all child elements in the "Arrange" pass of Silverlight layout.
		/// </summary>
		/// <param name="finalSize">The final area within the parent that this object should use
		/// to arrange itself and its children.</param>
		/// <returns>The actual size used.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			Size actualSize = new Size();

			//Calculate actual size of coverage of total coverage of all overlapping children
			actualSize.Width = _desiredSize.Width;
			actualSize.Height = _desiredSize.Height;

			Point origin = new Point((finalSize.Width - actualSize.Width) / 2.0, 0.0);

			foreach(FrameworkElement child in Children)
			{
				child.RenderTransform = _scale;
				Size childSize = child.DesiredSize;

				child.Arrange(new Rect(origin.X, origin.Y, childSize.Width, childSize.Height));
			}

			return finalSize;
		}



		/// <summary>
		/// Gets the child scale transform based on the maximum size of the given child. 
		/// Used by the Measure pass in Silverlight's layout process.
		/// </summary>
		/// <param name="availableSize">The available size.</param>
		/// <param name="childSize">The child's desired size.</param>
		/// <returns></returns>
		protected ScaleTransform GetChildScaleTransform(Size availableSize, Size childSize)
		{
			double scaleX = (childSize.Width == 0.0)? 0.0 : availableSize.Width / childSize.Width;
			double scaleY = (childSize.Height == 0.0)? 0.0 : availableSize.Height / childSize.Height;

			scaleX = Math.Max(1.0, Math.Min(scaleX, scaleY));

			ScaleTransform transform = new ScaleTransform();
			transform.ScaleX = scaleX;
			transform.ScaleY = scaleX;
			return transform;
		}



		/// <summary>
		/// Measures all child elements in the "Measure" pass of Silverlight's layout process.
		/// This aggregates all children and assumes the maximum size in each dimension.
		/// </summary>
		/// <param name="availableSize">The available size for all children to fit into.</param>
		/// <returns>
		/// The desired size of this panel and its contents, based on the union of its children's desired sizes.
		/// </returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			_desiredSize = new Size();
			Size childSizeCoverage = new Size();

			foreach(FrameworkElement child in Children)
			{
				child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
				childSizeCoverage.Width = Math.Max(child.DesiredSize.Width, childSizeCoverage.Width);
				childSizeCoverage.Height = Math.Max(child.DesiredSize.Height, childSizeCoverage.Height);
			}
			_scale = GetChildScaleTransform(availableSize, childSizeCoverage);

			_desiredSize.Width = _scale.ScaleX * childSizeCoverage.Width;
			_desiredSize.Height = _scale.ScaleY * childSizeCoverage.Height;

			return _desiredSize;
		}

		#endregion Methods 
	}
}