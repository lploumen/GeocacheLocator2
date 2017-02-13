using System.Globalization;
using GeocacheLocatorV2.PCL.Extensions;
using GeocacheLocatorV2.UWP;
using Xamarin.Forms;

[assembly: Dependency(typeof(Localize))]
namespace GeocacheLocatorV2.UWP
{
    public class Localize : ILocalize
    {
        public CultureInfo GetCurrentCultureInfo()
        {
            return  CultureInfo.CurrentUICulture;
        }
    }
}
