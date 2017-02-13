using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeocachingToolbox;
using GeocachingToolbox.GeocachingCom;
using GeocachingToolbox.Opencaching;
using Plugin.Connectivity;

namespace GeocachingLib
{
    public interface IGeocachingService
    {
        bool IsConnected { get; }
        string GeocachingComLogin { get; set; }
        string GeocachingComPassword { get; set; }
        string OpencachingToken { get; set; }
        string OpencachingTokenSecret { get; set; }
        bool UseGeocachingCom { get; set; }
        bool UseOpencaching { get; set; }

        Task Login();
        Task<IEnumerable<Geocache>> GetGeocachesFromMap(Location topLeft, Location bottomRight,
            CancellationToken cancellationToken);

        Task<Geocache> GetGeocacheDetailsAsync(
            string cacheCode,
            CancellationToken ct);

        Task GetGeocacheDetailsAsync(
            Geocache geocache,
            CancellationToken ct);

        void Reset();
    }

    public class GeocachingService : IGeocachingService
    {
        private GCClient m_gcClient;
        private OCClient m_ocClient;
        private readonly List<Client> m_clients = new List<Client>();
        private bool m_gcClientConnected;
        private bool m_ocClientConnected;
        private readonly ApiAccessKeysImpl m_apiKeys;

        public string GeocachingComLogin { get; set; }
        public string GeocachingComPassword { get; set; }

        public bool UseGeocachingCom { get; set; }
        public bool UseOpencaching { get; set; }

        public string OpencachingToken { get; set; }
        public string OpencachingTokenSecret { get; set; }
      

        public GeocachingService(string opencachingConsumerKey, string opencachingConsumerSecret)
        {
            m_apiKeys = new ApiAccessKeysImpl(opencachingConsumerKey, opencachingConsumerSecret);
        }

        public void Reset()
        {
            m_ocClientConnected = false;
            m_gcClientConnected = false;
        }

        public bool IsConnected =>
            (m_gcClient != null && m_gcClientConnected && CrossConnectivity.Current.IsConnected) ||
            (m_ocClient != null && m_ocClientConnected && CrossConnectivity.Current.IsConnected);

        public async Task Login()
        {
            Task loginGCTask = LoginGC(GeocachingComLogin, GeocachingComPassword);
            Task loginOCTask = LoginOC();
            await Task.WhenAll(loginGCTask, loginOCTask);
        }

        private async Task LoginGC(string gcLogin, string gcPassword)
        {
            if (m_gcClient != null)
                m_clients.Remove(m_gcClient);
            if (UseGeocachingCom && !m_gcClientConnected && CrossConnectivity.Current.IsConnected)
            {
                m_gcClient = new GCClient();
                await m_gcClient.Login(gcLogin, gcPassword);
                m_gcClientConnected = true;
                m_clients.Add(m_gcClient);
            }
        }

        private async Task LoginOC()
        {
            if (m_ocClient != null)
                m_clients.Remove(m_ocClient);
            if (UseOpencaching && !m_ocClientConnected && CrossConnectivity.Current.IsConnected)
            {
                AccessTokenStore tokens = new AccessTokenStore(OpencachingToken, OpencachingTokenSecret);
                m_ocClient = new OCClient("http://opencaching.pl/okapi/", null, tokens, m_apiKeys);
                await m_ocClient.Connect();
                m_ocClientConnected = true;
                m_clients.Add(m_ocClient);
            }
        }
        
        public async Task<IEnumerable<Geocache>> GetGeocachesFromMap(
            Location topLeft,
            Location bottomRight,
            CancellationToken cancellationToken)
        {
            List<Geocache> result = new List<Geocache>();
            foreach (var client in m_clients)
            {
                result.AddRange(await client.GetGeocachesFromMap<Geocache>(topLeft, bottomRight, cancellationToken));
            }
            return result;
        }

        public async Task GetGeocacheDetailsAsync(Geocache geocache,
            CancellationToken ct = default(CancellationToken))
        {
            await m_clients[0].GetGeocacheDetailsAsync(geocache, ct);
        }

        private Client GetClientFromCacheCode(string cacheCode)
        {
            cacheCode = cacheCode.ToUpper();
            if (cacheCode.StartsWith("GC"))
                return m_gcClient;
            if (cacheCode.StartsWith("OP"))
                return m_ocClient;
            return null;
        }

        public async Task<Geocache> GetGeocacheDetailsAsync(string cacheCode, CancellationToken ct = default(CancellationToken))
        {
            Client client = GetClientFromCacheCode(cacheCode);
            var geocache = await client.GetGeocacheDetailsAsync(cacheCode, ct);
            return geocache;
        }
    }
}
