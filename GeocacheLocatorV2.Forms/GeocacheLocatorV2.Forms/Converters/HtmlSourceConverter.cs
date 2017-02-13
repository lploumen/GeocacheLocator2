using System;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;

namespace GeocacheLocatorV2.PCL.Converters
{
    public class HtmlSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var html = new HtmlWebViewSource
            {
                Html = value != null ? value.ToString() : "Empty"
            };
            Debug.WriteLine("HtmlSourceConverter");
            return html;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
