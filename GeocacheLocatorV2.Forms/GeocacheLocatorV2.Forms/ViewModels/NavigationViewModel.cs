using Caliburn.Micro;
using GeocacheLocatorV2.PCL.Resources;
using GeocacheLocatorV2.PCL.Services;

namespace GeocacheLocatorV2.PCL.ViewModels
{
    public class NavigationViewModel : Screen
    {
        public MasterNavigationItem SelectedNavigationItem { get; set; }
        public BindableCollection<MasterNavigationItem> Navigation { get; set; } = new BindableCollection<MasterNavigationItem>();
        private readonly IMasterNavigationService navigationService;
        private readonly SettingsService m_Settings;
        private bool m_isEnabled;

        public NavigationViewModel(SettingsService settings, IMasterNavigationService navigationService)
        {
            this.navigationService = navigationService;
            m_Settings = settings;
            Navigation.Add(new MasterNavigationItem { Title = AppResource.Settings, ViewModelType = typeof(LoginViewModel) });
            Navigation.Add(new MasterNavigationItem { Title = AppResource.Map, ViewModelType = typeof(MapViewModel) });
        }

        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set
            {
                if (value == m_isEnabled) return;
                m_isEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public void UpdateMenu()
        {
            IsEnabled = m_Settings.LoginValidated;
        }

        public void SelectNavItem()
        {
            navigationService.MasterDetailPage.IsPresented = false;
            navigationService.ShowDetailPageFor(SelectedNavigationItem.ViewModelType);
        }
    }
}