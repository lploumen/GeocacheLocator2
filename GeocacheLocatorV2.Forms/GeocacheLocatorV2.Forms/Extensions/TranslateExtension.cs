using System;
using System.Globalization;
using GeocacheLocatorV2.PCL.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GeocacheLocatorV2.PCL.Extensions
{
    public interface ILocalize
    {
        CultureInfo GetCurrentCultureInfo();
    }

    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        readonly CultureInfo ci;
        const string ResourceId = "ARToolboxV2.Forms.Resources.AppResource";

        public TranslateExtension()
        {
            ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
        }

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return "";
            var translation = AppResource.ResourceManager.GetString(Text, ci);
            if (translation == null)
            {
#if DEBUG
                throw new ArgumentException(
                    $"Key '{Text}' was not found in resources '{ResourceId}' for culture '{ci.Name}'.",
                    "Text");
#else
				translation = Text; // HACK: returns the key, which GETS DISPLAYED TO THE USER
#endif
            }
            return translation;
        }
    }
}
