using GossipAPI.Model;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/setupGraph", (Graph input) =>
{
    var sb = new StringBuilder();
    sb.AppendLine("services:");

    int portCounter = 5001;
    foreach (var page in input.Pages)
    {
        string peers = string.Join(",", page.Links.Select(link => $"http://{link}:8080"));

        sb.AppendLine($"  {page.Id}:");
        sb.AppendLine("    build: .");
        sb.AppendLine($"    container_name: {page.Id}");
        sb.AppendLine("    environment:");
        sb.AppendLine($"      - NODE_ID={page.Id}");
        sb.AppendLine("      - ASPNETCORE_URLS=http://0.0.0.0:8080");
        sb.AppendLine($"      - PEERS={peers}");
        sb.AppendLine("    ports:");
        sb.AppendLine($"      - \"{portCounter}:8080\"");
        portCounter++;
    }

    string yamlContent = sb.ToString();

    File.WriteAllText("docker-compose.yml", yamlContent);

    return Results.Ok(new { message = "docker-compose.yml generated successfully", content = yamlContent });
});

app.Run();
