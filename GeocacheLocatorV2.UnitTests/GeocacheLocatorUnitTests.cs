using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Caliburn.Micro;
using GeocacheLocatorV2.PCL;
using GeocacheLocatorV2.PCL.Resources;
using GeocacheLocatorV2.PCL.Services;
using GeocacheLocatorV2.PCL.ViewModels;
using GeocachingLib;
using GeocachingLib.DB;
using GeocachingToolbox;
using GeocachingToolbox.GeocachingCom;
//using GeocachingToolbox.UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace GeocacheLocatorV2.UnitTests
{
    [TestClass]
    public class GeocacheLocatorUnitTests
    {

        //CacheRepository CreateAndInitCacheRepository()
        //{
        //    var connectionFactory = new Func<SQLiteConnectionWithLock>(() => new SQLiteConnectionWithLock(new SQLitePlatformGeneric(), new SQLiteConnectionString("geo.db", true)));
        //    var db = new SQLiteAsyncConnection(connectionFactory);
        //    CacheRepository repository = new CacheRepository(db);
        //    repository.Init().GetAwaiter().GetResult();
        //    return repository;
        //}

        [TestMethod]
        public void TestExceptionInDownloadCachesFromMap()
        {
            var cacheRepositoryStub = MockRepository.GenerateMock<ICacheRepository>();
            cacheRepositoryStub.Stub(x => x.Insert(Arg<Geocache>.Is.TypeOf)).Return(Task.FromResult(0));

            var stubbedConnector = MockRepository.GenerateMock<IGCConnector>();
            var gcClient = new GCClient(stubbedConnector)
            {
                _dateFormat = "dd/MM/yyyy"
            };
            stubbedConnector.Expect(x => x.GetPage("geocache/GC5V392")).Throw(new Exception("coucou"));
            stubbedConnector.Expect(x => x.GetPage("geocache/GC5V393")).Throw(new Exception("coucou"));

            GeocachingService geocachingService = new GeocachingService("123","456");

            MapViewModel mapVM = new MapViewModel(null, null, cacheRepositoryStub, geocachingService, null)
            {
                Items = new MtObservableCollection<ILocationViewModel>()
            };

            var pin1 = new GeocachePinViewModel
            {
                Latitude = 1,
                Longitude = 2,
                Description = "description",
                Code = "GC5V392",
                CacheType = GeocacheType.Traditional,
                Command = null
            };
            var pin2 = new GeocachePinViewModel
            {
                Latitude = 1,
                Longitude = 2,
                Description = "description",
                Code = "GC5V393",
                CacheType = GeocacheType.Traditional,
                Command = null
            };
            mapVM.Items.Add(pin1);
            mapVM.Items.Add(pin2);
            mapVM.DownloadAllVisibleCaches().GetAwaiter().GetResult();

            cacheRepositoryStub.AssertWasNotCalled(x => x.Insert(Arg<Geocache>.Matches(c => c.Code == "GC5V392")));
            stubbedConnector.AssertWasCalled(x => x.GetPage("geocache/GC5V392"));
            Assert.AreEqual(mapVM.Error, string.Format(AppResource.DownloadCacheError, "GC5V392") +
                Environment.NewLine +
                string.Format(AppResource.DownloadCacheError, "GC5V393"));
        }

        //[TestMethod]
        //public void TestMapChanged()
        //{
        //    var cacheRepositoryStub = MockRepository.GenerateMock<ICacheRepository>();


        //    var stubbedConnector = MockRepository.GenerateMock<IGCConnector>();
        //    var stubberGeocachingService = MockRepository.GenerateMock<IGeocachingService>();

        //    stubberGeocachingService.Expect(x => x.IsConnected).Return(true);
        //    var stubbedSetingsService = MockRepository.GenerateMock<ISettingsService>();
        //    stubbedSetingsService.Expect(x => x.GeocachingLoginValidated).Return(false);

        //    List<Geocache> cachesInMap = new List<Geocache>
        //    {
        //        new GCGeocache
        //        {
        //            Code = "GC2H81Y",
        //            Name = "Respect 2",
        //            Found = false,
        //            Waypoint = new Location(50, 39.588M, 5, 54.751M),
        //            Type = GeocacheType.Traditional
        //        },
        //        new GCGeocache
        //        {
        //            Code = "GC62EY0",
        //            Name = "Hoof",
        //            Found = false,
        //            Waypoint = new Location(50, 39.287M, 5, 55.293M),
        //            Type = GeocacheType.Traditional,
        //        }
        //    };


        //    List<Geocache> cachesInDB = new List<Geocache>
        //    {
        //         new GCGeocache
        //        {
        //            Code = "GC2H81Y",
        //            Name = "Respect 2",
        //            IsDetailed = true
        //        },
        //    };

        //    cacheRepositoryStub.Stub(
        //       x =>
        //           x.GetAsync(Arg<Expression<Func<DBGeocache, bool>>>.Is.Anything,
        //               Arg<Expression<Func<DBGeocache, Geocache>>>.Is.Null))
        //       .Return(Task.FromResult(cachesInDB.AsEnumerable()));


        //    stubberGeocachingService.Expect(
        //        x =>
        //            x.GetGeocachesFromMap(Arg<Location>.Is.Anything, Arg<Location>.Is.Anything,
        //                Arg<CancellationToken>.Is.Anything))
        //        .Return(Task.FromResult<IEnumerable<Geocache>>(cachesInMap));


        //    stubbedConnector.Expect(x => x.GetPage(Arg<string>.Is.Anything))
        //      .Do((Func<string, Task<string>>)(url =>
        //      {
        //          if (url == GCConstants.URL_LIVE_MAP)
        //          {
        //              var page = MspecExtensionMethods.ReadContent(@"GeocachingCom\WebpageContents\LiveMap.html");
        //              return Task.FromResult(page);
        //          }
        //          var x = HttpUtility.ParseQueryString(url).Get(0);
        //          var y = HttpUtility.ParseQueryString(url).Get("y");
        //          var z = HttpUtility.ParseQueryString(url).Get("z");
        //          var data = MspecExtensionMethods.ReadContent($@"GeocachingCom\WebpageContents\Tiles\Set2\map_{x}_{y}_{z}.json");
        //          return Task.FromResult(data);
        //      }));

        //    stubbedConnector.Expect(x => x.GetContent(Arg<string>.Is.Anything, Arg<IDictionary<string, string>>.Is.Null))
        //       .Do((Func<string, IDictionary<string, string>, Task<HttpContent>>)((url, data) =>
        //       {
        //           var x = HttpUtility.ParseQueryString(url).Get(0);
        //           var y = HttpUtility.ParseQueryString(url).Get("y");
        //           var z = HttpUtility.ParseQueryString(url).Get("z");
        //           var bytes = MspecExtensionMethods.ReadContentasByteArray($@"GeocachingCom\WebpageContents\Tiles\Set2\map_{x}_{y}_{z}.png");
        //           return Task.FromResult<HttpContent>(new ByteArrayContent(bytes));
        //       }));

        //    stubbedConnector.Expect(x => x.GetPage("geocache/GC5V392")).ReturnContentOf(@"GeocachingCom\WebpageContents\GeocacheDetails.html").Repeat.Once();
        //    EventAggregator eventAggregator = new EventAggregator();
        //    MapViewModel mapVM = new MapViewModel(eventAggregator, null, cacheRepositoryStub, stubberGeocachingService, stubbedSetingsService)
        //    {
        //        Items = new MtObservableCollection<ILocationViewModel>()
        //    };

        //    mapVM.MapViewChanged(
        //        new Rectangle(5.8996969694271684, 50.674132034182549, 0.056392522528767586, -0.051786918193101883),
        //        new Position(50.648238575086, 5.9278932306915522)).GetAwaiter().GetResult();

        //    // This item is in the DB, it should be returned.
        //    var firstItem = mapVM.Items.ToList().Find(x => x.Code == cachesInDB[0].Code);
        //    Assert.IsNotNull(firstItem);
        //    Assert.IsTrue(firstItem.IsDetailed);

        //    // Not in DB, return the object.
        //    var otherItem = mapVM.Items.ToList().Find(x => x.Code != cachesInDB[0].Code);
        //    Assert.IsNotNull(otherItem);
        //    Assert.IsFalse(otherItem.IsDetailed);

        //    Assert.AreEqual(mapVM.Items.Count, cachesInMap.Count);
        //}


        [TestMethod]
        public void TestMapChangedOffline()
        {
            var cacheRepositoryStub = MockRepository.GenerateMock<ICacheRepository>();
            var stubberGeocachingService = MockRepository.GenerateMock<IGeocachingService>();

            stubberGeocachingService.Expect(x => x.IsConnected).Return(false);
            var stubbedSetingsService = MockRepository.GenerateMock<ISettingsService>();
            stubbedSetingsService.Expect(x => x.GeocachingLoginValidated).Return(false);

            List<Geocache> cachesInDB = new List<Geocache>
            {
                 new GCGeocache
                {
                    Code = "GC2H81Y",
                    Name = "Respect 2",
                    IsDetailed = true,
                    Waypoint = new Location(10f,20f)
                },
                   new GCGeocache
                {
                    Code = "123",
                    Name = "222 2",
                    IsDetailed = true,
                    Waypoint = new Location(15f,20f)
                },
            };

            cacheRepositoryStub.Stub(
                x =>
                    x.GetAsync(Arg<Expression<Func<DBGeocache, bool>>>.Is.Anything,
                        Arg<Expression<Func<DBGeocache, Geocache>>>.Is.Null))
                .Return(Task.FromResult(cachesInDB.AsEnumerable()));

            EventAggregator eventAggregator = new EventAggregator();
            MapViewModel mapVM = new MapViewModel(eventAggregator, null, cacheRepositoryStub, stubberGeocachingService, stubbedSetingsService)
            {
                Items = new MtObservableCollection<ILocationViewModel>()
            };

            mapVM.MapViewChanged(
                new Rectangle(5.8996969694271684, 50.674132034182549, 0.056392522528767586, -0.051786918193101883),
                new Position(50.648238575086, 5.9278932306915522)).GetAwaiter().GetResult();

            Assert.AreEqual(mapVM.Items.Count, 2);
        }


        //[TestMethod]
        //public void TestDownloadCachesFromMap()
        //{
        //    var cacheRepositoryStub = MockRepository.GenerateMock<ICacheRepository>();
        //    cacheRepositoryStub.Stub(x => x.Insert(Arg<Geocache>.Is.TypeOf)).Return(Task.FromResult(0));

        //    var stubbedConnector = MockRepository.GenerateMock<IGCConnector>();
        //    var gcClient = new GCClient(stubbedConnector)
        //    {
        //        _dateFormat = "dd/MM/yyyy"
        //    };
        //    stubbedConnector.Expect(x => x.GetPage("geocache/GC5V392")).ReturnContentOf(@"GeocachingCom\WebpageContents\GeocacheDetails.html").Repeat.Once();
        //    GeocachingService geocachingService = new GeocachingService("123","456"/*gcClient*/);
        //    MapViewModel mapVM = new MapViewModel(new EventAggregator(), null, cacheRepositoryStub, geocachingService, null)
        //    {
        //        Items = new MtObservableCollection<ILocationViewModel>()
        //    };

        //    var pin1 = new GeocachePinViewModel
        //    {
        //        Latitude = 1,
        //        Longitude = 2,
        //        Description = "description",
        //        Code = "GC5V392",
        //        CacheType = GeocacheType.Traditional,
        //        Command = null
        //    };
        //    var pin2 = new GeocachePinViewModel
        //    {
        //        Latitude = 1,
        //        Longitude = 2,
        //        Description = "description",
        //        IsDetailed = true,
        //        Code = "DetailedCache",
        //        CacheType = GeocacheType.Traditional,
        //        Command = null
        //    };
        //    mapVM.Items.Add(pin1);
        //    mapVM.Items.Add(pin2);
        //    mapVM.DownloadAllVisibleCaches().GetAwaiter().GetResult();

        //    cacheRepositoryStub.AssertWasCalled(x => x.Insert(Arg<Geocache>.Matches(c => c.Code == "GC5V392")));
        //    cacheRepositoryStub.AssertWasNotCalled(x => x.Insert(Arg<Geocache>.Matches(c => c.Code == "DetailedCache")));
        //    stubbedConnector.AssertWasNotCalled(x => x.GetPage("geocache/DetailedCache"));
        //    stubbedConnector.AssertWasCalled(x => x.GetPage("geocache/GC5V392"));
        //}


    }
}
