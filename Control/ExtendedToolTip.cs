using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Buttercup.Control.UI.ViewModel;

namespace Buttercup.Control
{
    /// <summary>
    /// This extended Tooltip is used to make sure the tooltip scales with the rest of the 
    /// application. By default it doesn't inherit the scale of its associated control.
    /// </summary>
    public class ExtendedToolTip : ToolTip
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            // Make sure the scale on the tooltip is the same as the rest of the Application
            ScaleTransform transform = this.RenderTransform as ScaleTransform;
            if (transform == null)
            {
                transform = new ScaleTransform();
            }
            if (transform.ScaleX != DisplayAttributes.CommonInstance.VisualScale)
            {
                transform.ScaleX = DisplayAttributes.CommonInstance.VisualScale;
                transform.ScaleY = DisplayAttributes.CommonInstance.VisualScale;
                this.RenderTransform = transform;
            }

            // Call and return the value of the base method (in case it matters)
            return base.MeasureOverride(availableSize);
        }
    }
}
