using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Caliburn.Micro;
using GeocacheLocatorV2.PCL.Messages;
using GeocacheLocatorV2.PCL.Resources;
using GeocacheLocatorV2.PCL.Services;
using GeocachingLib;
using GeocachingToolbox;
using Plugin.Connectivity;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Position = Xamarin.Forms.Maps.Position;

namespace GeocacheLocatorV2.PCL.ViewModels
{
    public class MapViewModel : Screen, IHandleWithTask<MapViewChangedMessage>
    {
        private readonly ICacheRepository m_cacheRepository;
        private readonly IEventAggregator m_eventAggregator;
        private readonly IMasterNavigationService m_NavigationService;
        private string m_busyText;
        private readonly IGeocachingService m_geocachingService;
        private readonly ISettingsService m_settingsService;
        private CancellationTokenSource m_cts;
        private string m_error;
        private bool m_isBusy;
        private MtObservableCollection<ILocationViewModel> m_items;
        private bool m_isUpdatingLocation;
        public ICommand UpdateLocationCommand { get; set; }
        public ICommand DownloadAllVisibleCachesCommand { get; set; }

        public MapViewModel(IEventAggregator eventAggregator, IMasterNavigationService navigationService,
            ICacheRepository repository, IGeocachingService geocachingService, ISettingsService settingsService)
        {
            m_eventAggregator = eventAggregator;
            m_eventAggregator?.Subscribe(this);
            m_NavigationService = navigationService;
            m_cacheRepository = repository;
            UpdateLocationCommand = new Command(UpdateLocation);
            DownloadAllVisibleCachesCommand = new Command(async () => await DownloadAllVisibleCaches());
            m_geocachingService = geocachingService;
            m_settingsService = settingsService;
            Items = new MtObservableCollection<ILocationViewModel>();
        }

        public string Error
        {
            get { return m_error; }
            set
            {
                if (value == m_error) return;
                m_error = value;
                NotifyOfPropertyChange();
            }
        }

        public string BusyText
        {
            get { return m_busyText; }
            set
            {
                if (value == m_busyText) return;
                m_busyText = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsBusy
        {
            get { return m_isBusy; }
            set
            {
                if (value == m_isBusy) return;
                m_isBusy = value;
                Debug.WriteLine($"IsBusy = {value}");
                NotifyOfPropertyChange();
            }
        }

        public MtObservableCollection<ILocationViewModel> Items
        {
            get { return m_items; }
            set
            {
                m_items = value;
                NotifyOfPropertyChange();
            }
        }

        private bool IsCacheInBound(ILocationViewModel vm, Rectangle bounds)
        {
            return IsCacheInBound(vm.Longitude, vm.Latitude, bounds);
        }

        private bool IsCacheInBound(double longitude, double latitude, Rectangle bounds)
        {
            return longitude >= bounds.Left &&
                  longitude <= bounds.Right &&
                  latitude <= bounds.Top &&
                  latitude >= bounds.Bottom;
        }


        public async Task MapViewChanged(Rectangle bounds, Position center)
        {
            m_cts?.Cancel();
            m_cts = new CancellationTokenSource();

            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                BusyText = AppResource.FetchingCaches;

                // Remove all caches that are out of viewport
                var cachesOutOfViewport = Items.Where(x => !IsCacheInBound(x, bounds)).ToList();
                Items.RemoveRange(cachesOutOfViewport);

                Debug.WriteLine(
                    $"Bounds {bounds.Top} {bounds.Left} {bounds.Bottom} {bounds.Right} center {center.Latitude} {center.Longitude}");

                //JSFiddle
                Debug.WriteLine(
                    $"new google.maps.LatLng({bounds.Top}, {bounds.Left}),new google.maps.LatLng({bounds.Bottom},  {bounds.Right}))");
                //cache position 	long -119.68733215332 lat 35.4441833496094
                double left = Math.Min(bounds.Left, bounds.Right);
                double right = Math.Max(bounds.Left, bounds.Right);
                double top = Math.Max(bounds.Top, bounds.Bottom);
                double bottom = Math.Min(bounds.Top, bounds.Bottom);
                // get caches from db
                var dbCaches =
                     (await
                         m_cacheRepository.GetAsync<Geocache>(
                             x => x.WayPointLong >= left &&
                                    x.WayPointLong <= right &&
                                    x.WayPointLat <= top &&
                                    x.WayPointLat >= bottom
                      )).ToList();


                //var dbCaches =
                //       (await
                //           m_cacheRepository.GetAsync<Geocache>(
                //               x => x.WayPointLong >= bounds.Left &&
                //                      x.WayPointLong <= bounds.Right &&
                //                      x.WayPointLat <= bounds.Top &&
                //                      x.WayPointLat >= bounds.Bottom
                //        )).ToList();

                var cachesToAdd = dbCaches.Take(100).Select(CreateGeocachePinViewModel);
                cachesToAdd = cachesToAdd.Except(Items, new LocationViewModelComparer());
                // Add them to the collection
                Items.AddRange(cachesToAdd);

                await ConnectToGeocachingServices();

                List<Geocache> cachesList;
                if (m_geocachingService.IsConnected)
                {
                    Debug.WriteLine("Fetching caches");
                    // Get caches from Geocaching.com
                    var caches =
                        await
                            m_geocachingService.GetGeocachesFromMap(new Location(bounds.Top, bounds.Left),
                                new Location(bounds.Bottom, bounds.Right), m_cts.Token);

                    cachesList = caches.ToList();
                    if (cachesList.Count > 0)
                    {
                        // si + de 1000 caches, erreur lors du .Get
                        // split en plusieurs morceaux.
                        for (int i = 0; i < cachesList.Count; i += 200)
                        {
                            // Remove all caches that are downloaded from Geocaching.com and that are 
                            // in the db because they have priority
                            cachesList.RemoveAll(x => dbCaches.Find(y => y.Code == x.Code) != null);
                            cachesList.AddRange(dbCaches);
                        }
                    }
                }
                else
                {
                    cachesList = dbCaches;
                }

                if (cachesList.Count > 100)
                    Debug.WriteLine($"Too many caches on screen ({cachesList.Count})");

                var orderedCaches =
                    cachesList.OrderBy(
                        x =>
                            Math.Abs(
                                x.Waypoint.DistanceInMetersTo(new Location(center.Latitude,
                                    center.Longitude)))).Take(100).Select(CreateGeocachePinViewModel);

                // order them and add only those that are not displayed
                cachesToAdd = orderedCaches.Except(Items, new LocationViewModelComparer());

                foreach (var pin in cachesToAdd)
                {
                    Items.Add(pin);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("MapFetch cancelled");
            }
            finally
            {
                Debug.WriteLine("Fetching caches done");
                IsBusy = false;
            }
        }

        private ILocationViewModel CreateGeocachePinViewModel(Geocache cache)
        {

            return new GeocachePinViewModel
            {
                Latitude = (double)cache.Waypoint.Latitude,
                Longitude = (double)cache.Waypoint.Longitude,
                Description = cache.Name,
                Code = cache.Code,
                CacheType = cache.Type,
                IsDetailed = cache.IsDetailed,
                CacheStatus = cache.Status,
                Found = cache.Found,
                Own = cache.Own,
                Command = new Command(x => PinSelected((GeocachePinViewModel)x))
            };
        }

        public async Task Handle(MapViewChangedMessage message)
        {
            if (IsInitialized)
            {
                m_settingsService.SetLastLocationCenter(message.Center);
                m_settingsService.SetLastLocationRectangle(message.Bounds);
                await MapViewChanged(message.Bounds, message.Center);
            }
        }

        protected override async void OnActivate()
        {
            base.OnActivate();
            var requestMapSpan = MapSpan.FromCenterAndRadius(m_settingsService.GetLastLocationCenter(), Distance.FromMiles(1));
            await m_eventAggregator.PublishOnUIThreadAsync(new MapChangeViewMessage(requestMapSpan));//PublishOnUIThread(new MapChangeViewMessage(requestMapSpan));
            await MapViewChanged(m_settingsService.GetLastLocationRectangle(), m_settingsService.GetLastLocationCenter());
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            Items?.Clear();
            m_cts?.Cancel();
            m_settingsService?.Save();
        }

        private async Task ConnectToGeocachingServices()
        {
            if (m_settingsService.LoginValidated &&
                CrossConnectivity.Current.IsConnected &&
                !m_geocachingService.IsConnected)
            {
                try
                {
                    //IsBusy = true;
                    BusyText = AppResource.ConnectingToGeocachingService;
                    m_geocachingService.UseGeocachingCom = m_settingsService.UseGeocachingService && m_settingsService.GeocachingLoginValidated;
                    m_geocachingService.GeocachingComLogin = m_settingsService.GeocachingUsername;
                    m_geocachingService.GeocachingComPassword = m_settingsService.GeocachingPassword;

                    m_geocachingService.UseOpencaching = m_settingsService.UseOpencachingPlService && m_settingsService.OpencachingPlLoginValidated;
                    m_geocachingService.OpencachingToken = m_settingsService.OpencachingToken;
                    m_geocachingService.OpencachingTokenSecret = m_settingsService.OpencachingTokenSecret;

                    await m_geocachingService.Login();
                }
                catch (Exception ex)
                {
                    await UserDialogs.Instance.AlertAsync(string.Format(AppResource.CannotConnect, ex.Message));
                }
            }
        }

        public bool IsUpdatingLocation
        {
            get { return m_isUpdatingLocation; }
            set
            {
                if (value == m_isUpdatingLocation) return;
                m_isUpdatingLocation = value;
                NotifyOfPropertyChange();
            }
        }

        public async void UpdateLocation()
        {
            try
            {
                IsUpdatingLocation = true;
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 50;

                var position = await locator.GetPositionAsync(20000);
                Debug.WriteLine($"Position Status: {position.Timestamp}");
                Debug.WriteLine($"Position Latitude: {position.Latitude}");
                Debug.WriteLine($"Position Longitude: {position.Longitude}");

                var requestMapSpan = MapSpan.FromCenterAndRadius(new Position(position.Latitude, position.Longitude),
                    Distance.FromMiles(1));
                m_eventAggregator.PublishOnUIThread(new MapChangeViewMessage(requestMapSpan));
            }
            catch (GeolocationException ex)
            {
                if (ex.Error == GeolocationError.Unauthorized)
                {
                    await UserDialogs.Instance.AlertAsync(AppResource.GeolocationNotAuthorized);
                }
            }
            catch
            {

            }
            finally
            {
                IsUpdatingLocation = false;
            }
        }

        public async Task DownloadAllVisibleCaches()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                var itemsToDownload = Items.Where(x => !x.IsDetailed).ToList();
                int downloadCount = 1;
                foreach (var locationVM in itemsToDownload)
                {
                    try
                    {
                        BusyText = string.Format(AppResource.DownloadingCacheXofY, downloadCount++, itemsToDownload.Count);
                        var geocache = await DownloadAndSaveCache(locationVM.Code);
                    }
                    catch (Exception)
                    {
                        if (!string.IsNullOrEmpty(Error))
                            Error += Environment.NewLine;
                        Error += string.Format(AppResource.DownloadCacheError, locationVM.Code);
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<Geocache> DownloadAndSaveCache(string cacheCode)
        {
            m_cts?.Cancel();
            m_cts = new CancellationTokenSource();
            var geocache = await m_geocachingService.GetGeocacheDetailsAsync(cacheCode, m_cts.Token);
            await m_cacheRepository.Insert(geocache);
            m_eventAggregator.PublishOnUIThread(new GeocacheDownloadedMessage(CreateGeocachePinViewModel(geocache)));
            return geocache;
        }

        public async void PinSelected(ILocationViewModel vm)
        {
            if (IsBusy)
                return;
            m_cts?.Cancel();
            IsBusy = true;
            try
            {

                BusyText = AppResource.DownloadingCache;
                var cache = await m_cacheRepository.Get(vm.Code);
                if (cache == null)
                {
                    Debug.WriteLine("Downloading caches");
                    cache = await DownloadAndSaveCache(vm.Code);
                }
                Debug.WriteLine("Downloading caches finished");
                await m_NavigationService.NavigateToViewModelAsync(typeof(GeocacheDetailViewModel), cache);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}