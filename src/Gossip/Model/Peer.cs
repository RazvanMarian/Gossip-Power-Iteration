namespace Gossip.Model;

public class Peer(string url, double weight)
{
    public string Url { get; set; } = url;
    public double Weight { get; set; } = weight;
}