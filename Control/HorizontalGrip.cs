using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Buttercup.Control
{
	public class HorizontalGrip : System.Windows.Controls.Control
	{
		public static readonly DependencyProperty MinValueProperty;
		public static readonly DependencyProperty MaxValueProperty;
		public static readonly DependencyProperty IsRelativeToControlProperty;

		public static readonly DependencyProperty ValueProperty;

		public event EventHandler ValueChanged;


		private Point? _startLocation;
		private bool _isMouseCaptured = false;
		private int _startValue;
		private int _range;
		private UIElement _offsetSourceElement;
	

		static HorizontalGrip()
		{
			//Dynamic Properties
			MinValueProperty = DependencyProperty.Register("MinValue", typeof(int), typeof(HorizontalGrip),
				new PropertyMetadata(0));
			MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(HorizontalGrip),
				new PropertyMetadata(100));
			IsRelativeToControlProperty = DependencyProperty.Register("IsRelativeToControl", typeof(bool),
				typeof(HorizontalGrip), new PropertyMetadata(true));

			ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(HorizontalGrip),
				new PropertyMetadata(0));

		}



		public HorizontalGrip()
		{
			MouseLeftButtonDown += new MouseButtonEventHandler(HorizontalGrip_MouseLeftButtonDown);
			MouseMove += new MouseEventHandler(HorizontalGrip_MouseMove);
			MouseLeftButtonUp += new MouseButtonEventHandler(HorizontalGrip_MouseLeftButtonUp);
		}


	
		void HorizontalGrip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			FrameworkElement sourceElement = sender as FrameworkElement;
			if (!_isMouseCaptured)
			{
				sourceElement.CaptureMouse();
				_isMouseCaptured = true;

				_offsetSourceElement = IsRelativeToControl ? this : null;
				_startLocation = e.GetPosition(_offsetSourceElement);
				_startValue = Value;

				_range = MaxValue - MinValue + 1;
			}
		}



		void HorizontalGrip_MouseMove(object sender, MouseEventArgs e)
		{
			//Only process dragging from this control
			if (_isMouseCaptured)
			{
				if (_startLocation == null) return;

				Point currentPoint = e.GetPosition(_offsetSourceElement);

				int delta = (int)(currentPoint.X - _startLocation.Value.X);
				int newValue = _startValue + delta;

				newValue = Math.Max(MinValue, Math.Min(MaxValue, newValue));

				if (Value != newValue)
				{
					Value = newValue;

					if(ValueChanged != null)
						ValueChanged(this, new EventArgs());
				}
			}
		}



		void HorizontalGrip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			//Indicate that the user is no longer dragging from this control.
			if (_isMouseCaptured)
			{
				_startLocation = null;
				ReleaseMouseCapture();
				_isMouseCaptured = false;
			}
		}



		#region Properties

		/// <summary>
		/// Gets or sets the minimum possible value.
		/// </summary>
		/// <value>The minimum possible value.</value>
		public int MinValue
		{
			get { return (int)GetValue(MinValueProperty); }
			set { SetValue(MinValueProperty, value); }
		}


		/// <summary>
		/// Gets or sets the maximum possible value.
		/// </summary>
		/// <value>The maximum possible value.</value>
		public int MaxValue
		{
			get { return (int)GetValue(MaxValueProperty); }
			set { SetValue(MaxValueProperty, value); }
		}


		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The underlying value.</value>
		public int Value
		{
			get { return (int)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}


		/// <summary>
		/// Gets or sets a value indicating whether the pointer co-ordinates are relative to this control
		/// (as opposed to the entire window).
		/// </summary>
		/// <value>True if the pointer co-ordinates are relative to this control, otherwise false.</value>
		public bool IsRelativeToControl
		{
			get { return (bool)GetValue(IsRelativeToControlProperty); }
			set { SetValue(IsRelativeToControlProperty, value); }
		}

		#endregion


	}
}
