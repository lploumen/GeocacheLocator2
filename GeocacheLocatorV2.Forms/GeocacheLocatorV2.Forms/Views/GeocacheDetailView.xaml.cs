using Xamarin.Forms;

namespace GeocacheLocatorV2.PCL.Views
{
    public partial class GeocacheDetailView : TabbedPage
    {
        public GeocacheDetailView()
        {
            //InitializeComponent();
        }

        public void SetDescription(string description)
        {
            HtmlWebViewSource source = new HtmlWebViewSource { Html = description };
            webView.Source = source;
        }
    }
}
