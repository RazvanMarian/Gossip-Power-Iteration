using Gossip.Model;
using Gossip.Services;
using Gossip.Services.Abstractions;
using Gossip.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddSingleton<PowerIterationState>();
builder.Services.AddTransient<IPeerConfigurationLoader, PeerConfigurationLoader>();

builder.Services.AddHostedService<PowerIterationService>();
builder.Services.AddHostedService<NormalizationService>();
builder.Services.AddHostedService<AverageService>();

var app = builder.Build();

app.MapPost("/powerIteration", (PowerIterationMessage msg, PowerIterationState state) =>
{
    //Console.WriteLine($"PowerIteration from {msg.SourceNode}, Value={msg.Value}");

    lock (state)
    {
        state.Bi[msg.SourceNode] = msg.Value;
    }

    return Results.Ok();
});

app.MapPost("/normalization", (NormalizationMessage msg, PowerIterationState state) =>
{
    //Console.WriteLine($"Normalization from {msg.SourceNode}, GrowthRate={msg.Value}");

    double receivedValue = msg.Value;

    lock (state)
    {
        double oldGrowthRate = state.GrowthRate;

        state.GrowthRate = (oldGrowthRate + receivedValue) / 2.0;

        return Results.Ok(new NormalizationMessage(msg.SourceNode, oldGrowthRate));
    }
});

app.MapPost("/average", (AverageMessage msg, PowerIterationState state) =>
{
    //Console.WriteLine($"Average from {msg.SourceNode}, Value={msg.Value}");

    double receivedValue = msg.Value;

    lock (state)
    {
        double oldAvg = state.WeightAverage;
        // Aici, strategia cea mai simplă e pairwise averaging
        state.WeightAverage = (oldAvg + receivedValue) / 2;

        // Returnăm vechea valoare, la fel ca la normalizare
        return Results.Ok(new AverageMessage(msg.SourceNode, oldAvg));
    }
});


app.Run();
