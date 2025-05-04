namespace Rendezvous.Model;

public record ConsensusMessage(
        string SourceNode,
        double Value,
        int Iteration,  
        DateTime Timestamp 
    );
