using Gossip.Model;
using Gossip.Services.Abstractions;

namespace Gossip.Services;

public class AverageService : BaseBackgroundService
{
    private new readonly double _delta;

    public AverageService(
        IHttpClientFactory httpClientFactory,
        PowerIterationState state,
        IPeerConfigurationLoader peerLoader,
        IConfiguration config
    ) : base(httpClientFactory, config, state, peerLoader)
    {
        _delta = base._delta / 30;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _state.iterations != 20)
        {
            await Task.Delay(TimeSpan.FromSeconds(_delta), stoppingToken); // Interval la alegere

            await DoAverageStep();
        }
    }

    private async Task DoAverageStep()
    {
        if (_peers.Count == 0) return;

        var peer = _peers[_random.Next(_peers.Count)];

        double localAvg;
        lock (_state)
        {
            // Poți alege să folosești direct WeightAverage sau Weight
            localAvg = _state.WeightAverage;
        }

        try
        {
            var url = $"{peer.Url}/average";
            var reqMsg = new AverageMessage(_nodeId, localAvg);
            var response = await _client.PostAsJsonAsync(url, reqMsg);

            var respMsg = await response.Content.ReadFromJsonAsync<AverageMessage>();
            if (respMsg != null)
            {
                double remoteValue = respMsg.Value;

                lock (_state)
                {
                    _state.WeightAverage = (_state.WeightAverage + remoteValue) / 2;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Average error node:{_nodeId} {peer.Url}: {ex.Message}");
        }
    }
}
