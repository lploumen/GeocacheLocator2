using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Caliburn.Micro;
using Caliburn.Micro.Xamarin.Forms;
using GeocacheLocatorV2.PCL.Extensions;
using GeocacheLocatorV2.PCL.Resources;
using GeocacheLocatorV2.PCL.Services;
using GeocacheLocatorV2.PCL.ViewModels;
using GeocachingLib;
using SQLite.Net;
using SQLite.Net.Async;
using Xamarin.Forms;

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace GeocacheLocatorV2.PCL
{
    public partial class App : FormsApplication
    {
        private readonly SimpleContainer container;
        private readonly NavigationPage navigationPage = new NavigationPage();
        
        static App()
        {
            LogManager.GetLog = type => new DebugLogger(type);
            if (Device.OS != TargetPlatform.WinPhone)
            {
                AppResource.Culture = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
            }
        }
        SettingsService m_Settings = new SettingsService();
        public App(SimpleContainer container)
        {
            InitializeComponent();
            this.container = container;


            container.Instance(m_Settings);
            m_Settings.Load();

            var adapter = new MasterNavigationService(navigationPage);
           
            container
                .Singleton<LoginViewModel>()
                .Singleton<ShellViewModel>()
                .Singleton<MapViewModel>()
                .Singleton<GeocacheDetailViewModel>()
                .Singleton<CompassViewModel>()
                .Instance<ISettingsService>(m_Settings)
                .Instance<IGeocachingService>(CreateGeocachingService())
                .Singleton<MainViewModel>()
                .Singleton<NavigationViewModel>()
                .Instance(navigationPage)
                .Instance<IMasterNavigationService>(adapter);

            var sqliteConnectionWithLock = DependencyService.Get<ISQLite>();
            var connectionFactory = new Func<SQLiteConnectionWithLock>(() => sqliteConnectionWithLock.GetConnection());
            var db = new SQLiteAsyncConnection(connectionFactory);
            CacheRepository repository = new CacheRepository(db);
            repository.Init(); // TODO autre part
            container.Instance<ICacheRepository>(repository);

            
            Initialize();
            DisplayRootViewFor<MainViewModel>();
        }

        private IGeocachingService CreateGeocachingService()
        {
            var service = new GeocachingService(ApiKeys.OpencachingConsumerKey,ApiKeys.OpencachingConsumerKey)
            {
                UseGeocachingCom = m_Settings.UseGeocachingService && m_Settings.GeocachingLoginValidated,
                UseOpencaching = m_Settings.UseOpencachingPlService && m_Settings.OpencachingPlLoginValidated
            };

            if (m_Settings.GeocachingLoginValidated)
            {
                service.GeocachingComLogin = m_Settings.GeocachingUsername;
                service.GeocachingComPassword = m_Settings.GeocachingPassword;
            }
            if (m_Settings.OpencachingPlLoginValidated)
            {
                service.OpencachingToken = m_Settings.OpencachingToken;
                service.OpencachingTokenSecret = m_Settings.OpencachingTokenSecret;
            }
            return service;
        }

        protected override void PrepareViewFirst(NavigationPage navigationPage)
        {
            if (container.HasHandler(typeof(INavigationService), null))
                container.UnregisterHandler(typeof(INavigationService), null);
            container.Instance<INavigationService>(new NavigationPageAdapter(navigationPage));
        }
    }
}
