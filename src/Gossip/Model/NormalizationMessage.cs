namespace Gossip.Model;

public record NormalizationMessage(string SourceNode, double Value) : BaseMessage(SourceNode, Value);
