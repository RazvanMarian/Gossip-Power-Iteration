namespace Gossip.Model;

public class PowerIterationState
{
    private static readonly Random _random = new();

    public double Weight { get; set; } = 1.0;

    public double GrowthRate { get; set; } = 0.0;

    public double WeightAverage { get; set; } = 1.0;

    public Dictionary<string, double> Bi { get; set; } = [];

    public int iterations = 0;
}