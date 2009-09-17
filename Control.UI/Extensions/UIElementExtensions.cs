using System.Windows;
using System.Windows.Media;

namespace Buttercup.Control.UI.Extensions
{
    public static class UIElementExtensions
    {
        #region Methods (2)

        // Public Methods (2) 

        /// <summary>
        /// Search the ancestry of a given FrameworkElement for an Element of given type
        /// Used to determine the context of a UI element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T GetParentOfType<T>(this FrameworkElement element) where T : FrameworkElement
        {
            T returnElement = null;

            if (element == null) return returnElement;

            if (element.Parent is T)
            {
                returnElement = element.Parent as T;
            }
            else
            {
                returnElement = ((FrameworkElement)element.Parent).GetParentOfType<T>();
            }
            return returnElement;
        }

        /// <summary>
        /// Search the ancestry of a given UIElement for an Element of given type
        /// Used to determine the context of a UI element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T GetUIParentOfType<T>(UIElement element) where T : UIElement
        {
            UIElement p = VisualTreeHelper.GetParent(element) as UIElement;
            if (p != null)
            {
                if (p is T)
                    return p as T;

                return GetUIParentOfType<T>(p);
            }
            return null;
        }

        public static Point TopLeft(this UIElement element, UIElement parent)
        {
            GeneralTransform gt = element.TransformToVisual(parent);
            return gt.Transform(new Point(0, 0));
        }

        #endregion Methods
    }
}