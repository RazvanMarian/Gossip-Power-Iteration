using Gossip.Model;
using Gossip.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddSingleton<ConsensusState>();
builder.Services.AddHostedService<ConsensusService>();

builder.Logging.AddConsole();

var app = builder.Build();

app.MapPost("/consensus", (ConsensusMessage msg, ConsensusState state) =>
{
    var currentNodeId = Environment.GetEnvironmentVariable("NODE_ID") ?? "UNKNOWN_NODE";

    double weight_i_source = 0;

    lock (state)
    {
        state.IncomingWeightsMap.TryGetValue(msg.SourceNode, out weight_i_source);
    }

    double weightedValue = msg.Value * weight_i_source; 

    lock (state)
    {
        if (!state.IncomingValuesPerIteration.TryGetValue(msg.Iteration, out Dictionary<string, double>? value))
        {
            value = [];
            state.IncomingValuesPerIteration[msg.Iteration] = value;
        }

        value[msg.SourceNode] = weightedValue;

        int expectedMessages = state.IncomingWeightsMap.Count;
        int receivedMessagesCount = value.Count;

        bool iterationComplete = receivedMessagesCount == expectedMessages;

        bool isNextIteration = msg.Iteration == state.IterationsCompleted + 1;

        if (iterationComplete && isNextIteration)
        {
            Monitor.PulseAll(state);
        }
    } 

    return Results.Ok();
});

app.Run();
