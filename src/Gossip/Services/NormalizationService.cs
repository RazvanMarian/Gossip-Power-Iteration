using Gossip.Model;
using Gossip.Services.Abstractions;

namespace Gossip.Services;

public class NormalizationService : BaseBackgroundService
{
    private new readonly double _delta;

    public NormalizationService(
        IHttpClientFactory httpClientFactory,
        PowerIterationState state,
        IPeerConfigurationLoader peerLoader,
        IConfiguration config): base(httpClientFactory, config, state, peerLoader)
    {              
        _delta = base._delta / 5;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(_delta), stoppingToken);

            await DoNormalizationStep();
        }
    }

    private async Task DoNormalizationStep()
    {
        if (_peers.Count == 0) return;

        var peer = _peers[_random.Next(_peers.Count)];

        double localValue;
        lock (_state)
        {
            localValue = _state.Weight;
        }

        try
        {
            var url = $"{peer.Url}/normalization";

            var reqMsg = new NormalizationMessage(_nodeId, localValue);
            var response = await _client.PostAsJsonAsync(url, reqMsg);

            var respMsg = await response.Content.ReadFromJsonAsync<NormalizationMessage>();
            if (respMsg != null)
            {
                double remoteValue = respMsg.Value;

                lock (_state)
                {
                    _state.Weight = (_state.Weight + remoteValue) / 2.0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Normalization error node:{_nodeId} {peer.Url}: {ex.Message}");
        }
    }
}