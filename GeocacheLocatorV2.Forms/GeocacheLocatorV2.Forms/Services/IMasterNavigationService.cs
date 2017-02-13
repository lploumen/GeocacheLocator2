using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Caliburn.Micro.Xamarin.Forms;
using Xamarin.Forms;

namespace GeocacheLocatorV2.PCL.Services
{
    public interface IMasterNavigationService : INavigationService
    {
        Conductor<Screen> MainViewModel { get; set; }
        MasterDetailPage MasterDetailPage { get; set; }
        void ShowDetailPageFor(Type viewModelType);
        void ActivateViewModel(Screen viewModel);
        Task NavigateModalToViewModelAsync<TViewModel>(object parameter = null, bool animated = true);
        Task NavigateModalToViewModelAsync(Type viewModelType, object parameter = null, bool animated = true);
    }
}