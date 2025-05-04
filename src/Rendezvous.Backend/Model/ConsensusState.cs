namespace Rendezvous.Model;

public class ConsensusState
{
    public double Value { get; set; }

    public double SelfWeight { get; set; }

    public Dictionary<string, double> IncomingWeightsMap { get; } = [];

    public Dictionary<int, Dictionary<string, double>> IncomingValuesPerIteration { get; } = [];

    public int IterationsCompleted { get; set; } = -1;
}