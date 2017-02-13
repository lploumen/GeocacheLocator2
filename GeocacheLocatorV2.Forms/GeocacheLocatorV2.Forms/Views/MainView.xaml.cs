using Caliburn.Micro;
using GeocacheLocatorV2.PCL.Services;
using GeocacheLocatorV2.PCL.ViewModels;
using Xamarin.Forms;

namespace GeocacheLocatorV2.PCL.Views
{
    public partial class MainView : MasterDetailPage
    {
        public MainView()
        {
            InitializeComponent();
            IoC.Get<IMasterNavigationService>().MasterDetailPage = this;
            Detail = IoC.Get<NavigationPage>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as MainViewModel).NavigateToInitialView();
        }
    }
}
