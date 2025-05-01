
using Gossip.Model;
using System.Globalization;


namespace Gossip.Services.Abstractions // Or your actual namespace
{
    public abstract class BaseBackgroundService : BackgroundService
    {
        protected readonly IConfiguration _config;
        protected readonly HttpClient _client;
        protected readonly ConsensusState _state;
        protected readonly string _nodeId;
        protected readonly List<string> _outgoingPeerUrls = [];
        protected readonly ILogger<BaseBackgroundService> _logger; 

        protected BaseBackgroundService(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ConsensusState state,
            ILogger<BaseBackgroundService> logger
            )
        {
            _config = config;
            _state = state;
            _logger = logger;
            _client = httpClientFactory.CreateClient(); 

            _nodeId = _config["NODE_ID"] ?? string.Empty; 
            if (string.IsNullOrEmpty(_nodeId))
            {
                _logger.LogCritical("NODE_ID is not set or empty in configuration. Service cannot function.");
                throw new InvalidOperationException("NODE_ID is required and cannot be empty.");
            }

            _logger.LogInformation("[{NodeId}] BaseBackgroundService initializing...", _nodeId);

            LoadConfiguration();

            _logger.LogInformation("[{NodeId}] BaseBackgroundService initialization complete.", _nodeId);
        }

        private void LoadConfiguration()
        {
            _logger.LogDebug("[{NodeId}] Loading configuration values...", _nodeId);

            var initialValueString = _config["INITIAL_VALUE"];
            if (double.TryParse(initialValueString, CultureInfo.InvariantCulture, out double initialValue))
                _state.Value = initialValue;
            else
            {
                _logger.LogError("[{NodeId}] INITIAL_VALUE invalid or missing: '{InitialValue}'. Defaulting to 0.", _nodeId, initialValueString);
                _state.Value = 0;
            }

            var selfWeightString = _config["SELF_WEIGHT"];
            if (double.TryParse(selfWeightString, CultureInfo.InvariantCulture, out double selfWeightValue))
                _state.SelfWeight = selfWeightValue;
            else
            {
                _logger.LogError("[{NodeId}] SELF_WEIGHT invalid or missing: '{SelfWeight}'. Defaulting to 0.", _nodeId, selfWeightString);
                _state.SelfWeight = 0;
            }

            _state.IncomingWeightsMap.Clear();
            var incomingWeightsString = _config["INCOMING_WEIGHTS"] ?? "";
            var incomingPairs = incomingWeightsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in incomingPairs)
            {
                var parts = pair.Trim().Split('|');
                if (parts.Length == 2 && double.TryParse(parts[1], CultureInfo.InvariantCulture, out double weight))
                {
                    _state.IncomingWeightsMap[parts[0].Trim()] = weight;
                }
                else
                {
                    _logger.LogWarning("[{NodeId}] Invalid format in INCOMING_WEIGHTS: '{Pair}'", _nodeId, pair);
                }
            }

            var outgoingPeersString = _config["OUTGOING_PEERS"] ?? "";
            _outgoingPeerUrls.AddRange(outgoingPeersString
                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(url => url.Trim())
                                        .ToList());

            _logger.LogInformation("[{NodeId}] Configuration loaded. Value={Value}, SelfWeight={SelfWeight}, IncomingWeights={IncomingCount}, OutgoingPeers={OutgoingCount}",
                _nodeId, _state.Value, _state.SelfWeight, _state.IncomingWeightsMap.Count, _outgoingPeerUrls.Count);
        }

        protected abstract override Task ExecuteAsync(CancellationToken stoppingToken);
    }
}