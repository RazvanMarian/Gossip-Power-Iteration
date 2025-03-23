namespace Gossip.Model;

public record PowerIterationMessage(
    string SourceNode,
    double Value,
    DateTime Timestamp
) : BaseMessage(SourceNode, Value);
