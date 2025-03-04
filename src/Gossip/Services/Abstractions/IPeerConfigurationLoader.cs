using Gossip.Model;

namespace Gossip.Services.Abstractions;

public interface IPeerConfigurationLoader
{
    List<Peer> LoadPeers();
}
