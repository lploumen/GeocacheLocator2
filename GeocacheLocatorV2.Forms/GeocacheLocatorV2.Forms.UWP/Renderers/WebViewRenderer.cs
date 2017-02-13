using GeocacheLocatorV2.UWP.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(WebView), typeof(WebViewRenderer2))]
namespace GeocacheLocatorV2.UWP.Renderers
{
    public class WebViewRenderer2 : ViewRenderer<WebView, Windows.UI.Xaml.Controls.WebView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var webView = new Windows.UI.Xaml.Controls.WebView();
                    SetNativeControl(webView);

                    var html = Element.Source as HtmlWebViewSource;
                    if (html != null)
                        webView.NavigateToString(html.Html);
                }
            }
        }
    }
}