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
        while (!stoppingToken.IsCancellationRequested)
        {
            int delaySeconds = _random.Next(1, 6);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);

            await SendToNeighbours();

            UpdateLocalWeight();
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
            {
                b += pair.Value;
            }

            if (b == 0)
            {
                return;
            }

            double w_old = _state.Weight;
            if (w_old == 0) w_old = 1;

            double r_new = Math.Log(b / w_old);
            
            double r_old = _state.GrowthRate;
            double w_new = b * Math.Exp(r_old);

            //Console.WriteLine($"weight={w_new}");

            _state.Weight = w_new;
            _state.GrowthRate = r_new;
        }
    }
}
