using Gossip.Model;

namespace Gossip.Services.Abstractions
{
    public abstract class BaseBackgroundService: BackgroundService
    {
        protected readonly IConfiguration _config;
        protected readonly HttpClient _client;
        protected readonly PowerIterationState _state;
        protected readonly Random _random = new();

        protected readonly string _nodeId;
        protected readonly List<Peer> _peers = [];

        protected readonly double _delta; 


        protected BaseBackgroundService(IHttpClientFactory httpClientFactory,
            IConfiguration config,
            PowerIterationState state,
            IPeerConfigurationLoader peerConfigurationLoader)
        {
            _config = config;
            _client = httpClientFactory.CreateClient();
            _state = state;
            _nodeId = _config["NODE_ID"] ?? "";

            _peers = peerConfigurationLoader.LoadPeers();

            _delta = double.Parse(config["DELTA"] ?? "5");
        }
    }
}
