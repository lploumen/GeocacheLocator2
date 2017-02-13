using System.Reflection;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GeocacheLocatorV2.UWP
{
    internal static class Helper
    {
        public static Brush ToBrush(this Xamarin.Forms.Color color)
        {
            return new SolidColorBrush(color.ToMediaColor());
        }

        public static Color ToMediaColor(this Xamarin.Forms.Color color)
        {
            return Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
        }

        public static Thickness ToWinPhone(this Xamarin.Forms.Thickness t)
        {
            return new Thickness(t.Left, t.Top, t.Right, t.Bottom);
        }

        public static T FindChildControl<T>(this DependencyObject control, string ctrlName) where T : UIElement
        {
            int childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (int i = 0; i < childNumber; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, i);
                FrameworkElement fe = child as FrameworkElement;
                // Not a framework element or is null
                if (fe == null) return default(T);


                if (child is T && fe.Name == ctrlName)
                {
                    // Found the control so return
                    return child as T;
                }
                else
                {
                    // Not found it - search children
                    T nextLevel = FindChildControl<T>(child, ctrlName);
                    if (nextLevel != null)
                        return nextLevel;
                }
            }
            return default(T);
        }

        public static FrameworkElement GetRootPage(this FrameworkElement control)
        {
            var p = control.Parent;
            while (p != null)
            {
                if (typeof(Page).IsInstanceOfType(p))
                {
                    break;
                }
                else
                    p = (p as FrameworkElement).Parent;
            }

            return p as FrameworkElement;
        }
    }
}
