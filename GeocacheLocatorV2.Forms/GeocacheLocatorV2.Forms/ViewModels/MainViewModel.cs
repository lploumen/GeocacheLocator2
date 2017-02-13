using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Caliburn.Micro;
using GeocacheLocatorV2.PCL.Resources;
using GeocacheLocatorV2.PCL.Services;

namespace GeocacheLocatorV2.PCL.ViewModels
{
    public class MainViewModel : Conductor<Screen>
    {
        public Screen ActiveScreen { get; set; }
        private readonly SettingsService m_Settings;
       
        public NavigationViewModel Navigation { get; set; } = IoC.Get<NavigationViewModel>();

        private readonly IMasterNavigationService navigationService;


        public MainViewModel(SettingsService settings, IMasterNavigationService navigationService)
        {
            this.navigationService = navigationService;
            m_Settings = settings;
            navigationService.MainViewModel = this;
        }

        protected override async void OnActivate()
        {
            base.OnActivate();
            if (!m_Settings.UserLocationAsked)
            {
                bool allowLocation =
                    await
                        UserDialogs.Instance.ConfirmAsync(AppResource.PrivacyStatementText,
                            AppResource.PrivacyStatementHeader);
                m_Settings.UserLocationEnabled = allowLocation;
                m_Settings.UserLocationAsked = true;
                m_Settings.Save();
            }
            LocationManager.Instance.UserLocationAllowed = m_Settings.UserLocationEnabled;
        }

        public void NavigateToInitialView()
        {
            Type vmType = typeof(LoginViewModel);
            if (m_Settings?.LoginValidated ?? false)
                vmType = typeof(MapViewModel);
            navigationService.ShowDetailPageFor(vmType);
        }

        public void UpdateMenu()
        {
            Navigation.UpdateMenu();
        }
    }
}