using Gossip.Model;
using Gossip.Services.Abstractions;

namespace Gossip.Services.Implementations;

public class PeerConfigurationLoader(IConfiguration config) : IPeerConfigurationLoader
{
    private readonly IConfiguration _config = config;

    public List<Peer> LoadPeers()
    {
        try
        {
            var peersString = _config["PEERS"] ?? "";
            var peerUrls = peersString
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            var weightsEnv = Environment.GetEnvironmentVariable("WEIGHTS");
            var weightDict = new Dictionary<string, double>();
            if (!string.IsNullOrEmpty(weightsEnv))
            {
                var weightPairs = weightsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in weightPairs)
                {
                    var parts = pair.Split('|');
                    if (parts.Length == 2 && double.TryParse(parts[1], out double weight))
                    {
                        weightDict[parts[0]] = weight;
                    }
                }
            }

            return peerUrls.Select(url => new Peer(url, weightDict[url])).ToList();
        }
        catch (Exception ex) 
        { 
            Console.WriteLine($"Node construction error {_config["NODE_ID"] ?? ""} : {ex.Message}");
            return [];
        }
    }
}
