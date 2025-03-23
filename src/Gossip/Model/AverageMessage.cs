namespace Gossip.Model;

public record AverageMessage(string SourceNode, double Value) : BaseMessage(SourceNode, Value);