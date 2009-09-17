using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Buttercup.Control
{
	public class MinSpacePreservingPanel : Panel
	{
		#region Fields (5) 

		private List<Size> _containerSize;
		private List<ScaleTransform> _scale;
		private double _normalisedUnitWidth;

		public static readonly DependencyProperty BaseUnitWidthProperty =
			DependencyProperty.Register("BaseUnitWidth", typeof(double), typeof(HorizontalScalingStackPanel),
				new PropertyMetadata(50.0));

		public static readonly DependencyProperty ScaleMultiplierProperty =
			DependencyProperty.Register("ScaleMultiplier", typeof(double), typeof(HorizontalScalingStackPanel),
				new PropertyMetadata(1.0));

		public static readonly DependencyProperty ShouldScaleProperty =
			DependencyProperty.RegisterAttached("ShouldScale", typeof(bool), typeof(HorizontalScalingStackPanel),
				new PropertyMetadata(true));

		public static readonly DependencyProperty WidthRatioProperty =
			DependencyProperty.RegisterAttached("WidthRatio", typeof(double), typeof(HorizontalScalingStackPanel),
				new PropertyMetadata(1.0));

		#endregion Fields 



		#region Properties (1) 

		/// <summary>
		/// Gets or sets the base unit width of this panel.
		/// </summary>
		/// <value>The base unit width.</value>
		public double BaseUnitWidth
		{
			get { return (double)GetValue(BaseUnitWidthProperty); }
			set
			{
				SetValue(BaseUnitWidthProperty, value);
				InvalidateMeasure();
			}
		}

		/// <summary>
		/// Gets or sets the scaling multiplier for all child items in the panel.
		/// </summary>
		/// <value>The scaling multiplier.</value>
		public double ScaleMultiplier
		{
			get { return (double)GetValue(ScaleMultiplierProperty); }
			set
			{
				SetValue(ScaleMultiplierProperty, value);
				InvalidateMeasure();
			}
		}

		#endregion Properties 



		#region Methods (7) 

		// Public Methods (4) 

		public static bool GetShouldScale(UIElement element)
		{
			return (bool)element.GetValue(ShouldScaleProperty);
		}



		public static double GetWidthRatio(UIElement element)
		{
			return (double)element.GetValue(WidthRatioProperty);
		}



		public static void SetShouldScale(UIElement element, bool value)
		{
			element.SetValue(ShouldScaleProperty, value);
		}



		public static void SetWidthRatio(UIElement element, double value)
		{
			element.SetValue(WidthRatioProperty, value);
		}



		// Protected Methods (3) 

		/// <summary>
		/// Arranges all child elements in the "Arrange" pass of Silverlight layout.
		/// </summary>
		/// <param name="finalSize">The final area within the parent that this object should use
		/// to arrange itself and its children.</param>
		/// <returns>The actual size used.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			Size actualSize = new Size(0.0, finalSize.Height);
			Point origin = new Point();

			int currentScaleIndex = 0;
			int currentChildIndex = 0;
			foreach(FrameworkElement child in Children)
			{
				if(child.Visibility == Visibility.Collapsed)
					continue;

				bool shouldChildScale = GetShouldScale(child);
				Size childSize = _containerSize[currentChildIndex];
				double allocatedWidth = childSize.Width;
				currentChildIndex++;
				if(shouldChildScale)
				{
					//Get & apply next scale transform
					ScaleTransform currentTransform = _scale[currentScaleIndex];
					child.RenderTransform = currentTransform;
					currentScaleIndex++;
					childSize.Width = _normalisedUnitWidth * GetWidthRatio(child);
					childSize.Height /= currentTransform.ScaleY;
				}

				child.Arrange(new Rect(origin.X, origin.Y, childSize.Width, childSize.Height));
				origin.X += allocatedWidth;
				actualSize.Width += allocatedWidth;
			}

			return actualSize;
		}



		/// <summary>
		/// Gets the child scale transform based on the maximum size of the given child. 
		/// Used by the Measure pass in Silverlight's layout process.
		/// This will allow the child to resize to the maximum size to allow it to fit in the container,
		/// but avoid gaps to the left and right.
		/// </summary>
		/// <param name="availableSize">The available size.</param>
		/// <param name="childSize">The child's desired size.</param>
		/// <returns></returns>
		protected ScaleTransform GetChildScaleTransform(Size availableSize, Size childSize)
		{
			double scaleX = (childSize.Width == 0.0)? 0.0 : availableSize.Width / childSize.Width;
			double scaleY = (childSize.Height == 0.0)? 0.0 : availableSize.Height / childSize.Height;

			scaleX = Math.Min(scaleX, scaleY); //Limit scaling by vertical scale
			if(Math.Abs(scaleX) < 0.001)
				scaleX = 1.0;

			ScaleTransform transform = new ScaleTransform();
			transform.ScaleX = scaleX;
			transform.ScaleY = scaleX;
			return transform;
		}


		/// <summary>
		/// Gets the maximum unit width that can fit within the maximum width.
		/// This allows the allocated space to be adjusted to fit within the available size.
		/// </summary>
		/// <returns></returns>
		protected double GetUnitWidthLimit(double maximumWidth)
		{
			double cumulativeWidthRatio = 0.0;

			foreach(FrameworkElement child in Children)
			{
				if(child.Visibility != Visibility.Collapsed)
					cumulativeWidthRatio += GetWidthRatio(child);
			}

			return (cumulativeWidthRatio > 0)? maximumWidth / cumulativeWidthRatio : BaseUnitWidth;
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
			_containerSize = new List<Size>();
			_scale = new List<ScaleTransform>();

			Size cumulativeSize = new Size();
			_normalisedUnitWidth = Math.Min(BaseUnitWidth, GetUnitWidthLimit(availableSize.Width));

			foreach(FrameworkElement child in Children)
			{
				if(child.Visibility == Visibility.Collapsed)
					continue;

				bool shouldChildScale = GetShouldScale(child);
				double calculatedWidth = _normalisedUnitWidth * GetWidthRatio(child);
				Size allocatedSize = (shouldChildScale) ?
						//Scaled children should be allocated a fixed width but open-ended height - hence no ScaleMultiplier.
						new Size(calculatedWidth, Double.PositiveInfinity)
						//Unscaled children should be allocated the full scaled width and available height.
						: new Size(calculatedWidth * ScaleMultiplier, availableSize.Height);
				child.Measure(allocatedSize);

				Size containerSize;
				if(shouldChildScale)
				{
					containerSize = new Size(calculatedWidth * ScaleMultiplier, availableSize.Height);
					ScaleTransform currentTransform = GetChildScaleTransform(containerSize, child.DesiredSize);
					_scale.Add(currentTransform);

					//Amend size of container to fit scaled child tightly
					containerSize.Width = child.DesiredSize.Width * currentTransform.ScaleX;
					containerSize.Height = child.DesiredSize.Height * currentTransform.ScaleY;
				}
				else
					containerSize = new Size(allocatedSize.Width, child.DesiredSize.Height);

				// Merge allocated space into cumulative size (desired size of the panel)
				cumulativeSize.Width += containerSize.Width;
				cumulativeSize.Height = Math.Max(cumulativeSize.Height, containerSize.Height);

				if(!shouldChildScale)
					//Correct height of container for unscaled items - to use the full height.
					containerSize.Height = availableSize.Height;

				_containerSize.Add(containerSize);
			}

			return cumulativeSize;
		}

		#endregion Methods 
	}
}