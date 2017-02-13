using System;
using System.Diagnostics;
using Acr.UserDialogs;
using Caliburn.Micro;
using GeocacheLocatorV2.PCL.Resources;
using GeocachingLib;
using GeocachingToolbox.GeocachingCom;
using GeocachingToolbox.Opencaching;
using Xamarin.Forms;

namespace GeocacheLocatorV2.PCL.ViewModels
{
    public class LoginViewModel : Screen
    {
        private readonly SettingsService m_settings;
        private readonly IGeocachingService m_geocachingService;
        private string username;
        private string password;
        private string m_geocachingLoginState;
        private string m_OpencachingPLLoginState;
        private bool m_locationEnabled;

        private OCClient ocClient;
        private bool m_useGeocachingCom;
        private bool m_useGeocachingPl;

        public LoginViewModel(SettingsService settings,IGeocachingService geocachingService)
        {
            m_settings = settings;
            m_geocachingService = geocachingService;
        }

        public bool CanLogin => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

        public string OpencachingPLLoginState
        {
            get { return m_OpencachingPLLoginState; }
            set
            {
                if (value == m_OpencachingPLLoginState) return;
                m_OpencachingPLLoginState = value;
                NotifyOfPropertyChange();
            }
        }

        public string GeocachingLoginState
        {
            get { return m_geocachingLoginState; }
            set
            {
                if (value == m_geocachingLoginState) return;
                m_geocachingLoginState = value;
                NotifyOfPropertyChange();
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            LocationEnabled = m_settings.UserLocationEnabled;
            GeocachingLoginState = m_settings.GeocachingLoginValidated ? string.Format(AppResource.LoggedInAs, m_settings.GeocachingUsername) : AppResource.Login;
            OpencachingPLLoginState = m_settings.OpencachingPlLoginValidated ? AppResource.LoggedIn : AppResource.Login;
            UseGeocachingCom = m_settings.UseGeocachingService;
            UseGeocachingPl = m_settings.UseOpencachingPlService;
        }

        public bool UseGeocachingCom
        {
            get { return m_useGeocachingCom; }
            set
            {
                if (value == m_useGeocachingCom) return;
                m_useGeocachingCom = value;
                NotifyOfPropertyChange();
            }
        }

        public bool UseGeocachingPl
        {
            get { return m_useGeocachingPl; }
            set
            {
                if (value == m_useGeocachingPl) return;
                m_useGeocachingPl = value;
                NotifyOfPropertyChange();
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            m_geocachingService.Reset();
            if (m_settings.GeocachingLoginValidated)
            {
                m_geocachingService.GeocachingComLogin = m_settings.GeocachingUsername;
                m_geocachingService.GeocachingComPassword = m_settings.GeocachingPassword;
            }
            if (m_settings.OpencachingPlLoginValidated)
            {
                m_geocachingService.OpencachingToken = m_settings.OpencachingToken;
                m_geocachingService.OpencachingTokenSecret = m_settings.OpencachingTokenSecret;
            }
            m_settings.UseGeocachingService = UseGeocachingCom;
            m_settings.UseOpencachingPlService = UseGeocachingPl;
            m_settings.UserLocationEnabled = LocationEnabled;
            m_settings.Save();
        }

        public async void LoginOpencachingPl()
        {
            if (m_settings.OpencachingPlLoginValidated)
            {
                bool res = await UserDialogs.Instance.ConfirmAsync("Realy logout?", "Log out", "Yes", "No");
                if (res)
                {
                    m_settings.OpencachingToken = "";
                    m_settings.OpencachingPlPassword = "";
                }
            }

            ApiAccessKeysImpl apiKeys = new ApiAccessKeysImpl(ApiKeys.OpencachingConsumerKey, ApiKeys.OpencachingConsumerSecret);
            AccessTokenStore accessTokenStore = new AccessTokenStore(m_settings.OpencachingToken, m_settings.OpencachingTokenSecret);
            ocClient = new OCClient("http://opencaching.pl/okapi/", null, accessTokenStore, apiKeys);
            if (ocClient.NeedsAuthorization)
            {
                string authUrl = await ocClient.GetAuthorizationUrl();
                Device.OpenUri(new Uri(authUrl));
                var promptResult = await UserDialogs.Instance.PromptAsync("Enter Pin");
                if (promptResult.Ok)
                {
                    try
                    {
                        await ocClient.EnterAuthorizationPin(promptResult.Text);
                        m_settings.OpencachingToken = accessTokenStore.Token;
                        m_settings.OpencachingTokenSecret = accessTokenStore.TokenSecret;

                        OpencachingPLLoginState = AppResource.LoggedIn;
                        m_settings.OpencachingPlLoginValidated = true;
                        await UserDialogs.Instance.AlertAsync("OK");
                    }
                    catch
                    {
                        OpencachingPLLoginState = AppResource.Login;
                        m_settings.OpencachingPlLoginValidated = false;
                        await UserDialogs.Instance.AlertAsync("KO");
                    }
                }
            }
            m_settings.Save();
            Debug.WriteLine(apiKeys.ConsumerKey);
        }

        public async void LoginGeocachingCom()
        {
            var lc = new LoginConfig
            {
                Message = AppResource.GeocachingComEnterCredentials,
                Title = AppResource.GeocachingCom,
                LoginValue = m_settings.GeocachingUsername,
            };

            var loginResult = await UserDialogs.Instance.LoginAsync(lc);
            if (loginResult.Ok && !string.IsNullOrEmpty(loginResult.LoginText) && !string.IsNullOrEmpty(loginResult.Password))
            {
                UserDialogs.Instance.ShowLoading(AppResource.Login);
                GCClient client = new GCClient();
                try
                {
                    await client.Login(loginResult.LoginText, loginResult.Password);
                    await UserDialogs.Instance.AlertAsync("OK");
                    GeocachingLoginState = string.Format(AppResource.LoggedInAs, loginResult.LoginText);
                    m_settings.GeocachingLoginValidated = true;
                }
                catch
                {
                    await UserDialogs.Instance.AlertAsync("KO");
                    GeocachingLoginState = AppResource.Login;
                    m_settings.GeocachingLoginValidated = false;
                }
                finally
                {
                    m_settings.GeocachingUsername = loginResult.LoginText;
                    m_settings.GeocachingPassword = loginResult.Password;
                    m_settings.Save();
                    UserDialogs.Instance.HideLoading();
                }
            }
        }

        public bool LocationEnabled
        {
            get { return m_locationEnabled; }
            set
            {
                if (value == m_locationEnabled) return;
                m_locationEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                NotifyOfPropertyChange(() => Username);
                NotifyOfPropertyChange(() => CanLogin);
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                NotifyOfPropertyChange(() => Password);
                NotifyOfPropertyChange(() => CanLogin);
            }
        }
    }
}