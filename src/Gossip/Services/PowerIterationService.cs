using Gossip.Model;
using Gossip.Services.Abstractions;

namespace Gossip.Services;

public class PowerIterationService(IHttpClientFactory httpClientFactory,
    IConfiguration config,
    PowerIterationState state,
    IPeerConfigurationLoader peerConfigurationLoader) : BaseBackgroundService(httpClientFactory, config, state, peerConfigurationLoader)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _state.iterations != 20)
        {
            await Task.Delay(TimeSpan.FromSeconds(_delta), stoppingToken);

            await SendToNeighbours();

            UpdateLocalWeight();

            lock(_state)
            {
                _state.iterations++;
            }
        }
    }

    private async Task SendToNeighbours()
    {
        foreach (var peer in _peers)
        {
            try
            {
                var message = new PowerIterationMessage(
                    SourceNode: _nodeId,
                    Value: _state.Weight * peer.Weight,
                    Timestamp: DateTime.UtcNow
                );

                var url = $"{peer.Url}/powerIteration";

                var response = await _client.PostAsJsonAsync(url, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_nodeId}] Error sending gossip {peer.Url}: {ex.Message}");
            }
        }
    }

    private void UpdateLocalWeight()
    {
        lock (_state)
        {
            double b = 0;
            foreach (var pair in _state.Bi)
                b += pair.Value;

            if (b == 0) return;

            double w_old = _state.Weight;

            double r_new = Math.Log(b / w_old);

            double n_i = _state.WeightAverage;

            double c = Math.Exp(_state.GrowthRate) * ((0.2 / (1 + 1/n_i)) + 0.9);

            double epsilon = 0.1;

            double w_new = (1 - epsilon) * (b/c) + epsilon * n_i;

            Console.WriteLine($"weight={w_new} c={c} b={b} n_i={n_i}");

            _state.Weight = w_new;
            _state.WeightAverage = w_new;
            _state.GrowthRate = r_new;
            _state.Bi.Clear();
        }
    }

}
