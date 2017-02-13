using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Caliburn.Micro;
using Caliburn.Micro.Xamarin.Forms;
using GeocacheLocatorV2.PCL.Resources;
using GeocachingToolbox;
using GeocachingToolbox.GeocachingCom;

namespace GeocacheLocatorV2.PCL.ViewModels
{
    public class ShellViewModel : Screen
    {
        private readonly INavigationService m_NavigationService;
        private readonly SettingsService m_settingsService;


        public ShellViewModel(INavigationService navigationService, SettingsService settingsService)
        {
            m_NavigationService = navigationService;
            m_settingsService = settingsService;
        }

        protected override async void OnActivate()
        {
            base.OnActivate();
            NotifyOfPropertyChange(nameof(CanShowMap));
            if (!m_settingsService.UserLocationAsked)
            {
                bool allowLocation =
                    await
                        UserDialogs.Instance.ConfirmAsync(AppResource.PrivacyStatementText,
                            AppResource.PrivacyStatementHeader);
                m_settingsService.UserLocationEnabled = allowLocation;
                m_settingsService.UserLocationAsked = true;
                m_settingsService.Save();
            }
            LocationManager.Instance.UserLocationAllowed = m_settingsService.UserLocationEnabled;
        }

        public bool CanShowMap => m_settingsService?.LoginValidated ?? false;

        public void ShowMap()
        {
            m_NavigationService.For<MapViewModel>().Navigate();
        }

        public async void ShowCompass()
        {
            GCGeocache gc = new GCGeocache();
            gc.Waypoint = new Location(50.6475, 5.9166);
            await m_NavigationService.NavigateToViewModelAsync(typeof(CompassViewModel), gc);
        }

        public void ShowSettings()
        {
            m_NavigationService.For<LoginViewModel>().Navigate();
        }
    }
}
