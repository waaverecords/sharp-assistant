using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddActionServices();
        services.AddActionStore();
    })
    .Build();

using var httpClient = new HttpClient();
var jsonContent = new StringContent(
    JsonSerializer.Serialize(new
    {
        model = "phi4",
        prompt = $@"
            {Prompts.Main}
            <user>Help plz.</user>
        "
    }),
    Encoding.UTF8,
    "application/json"
);
var response = await httpClient.PostAsync("http://localhost:11434/api/generate", jsonContent);
var json = await response.Content.ReadAsStringAsync();

var jsonElements = json.Split(["\n", "\r\n"], StringSplitOptions.RemoveEmptyEntries)
    .Select(jsonObject => JsonSerializer.Deserialize<JsonElement>(jsonObject));
foreach(var jsonElement in jsonElements)
    if (!jsonElement.GetProperty("done").GetBoolean())
        Console.Write(jsonElement.GetProperty("response").GetString());
Console.Write("\r\n");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    
    var actionStore = host.Services.GetRequiredService<ActionVectorStore>();
    var results = await actionStore.QueryAsync("i want to insert a product");
    await foreach(var result in results.Results)
    {
        Console.WriteLine($"Type: {result.Record.Type}");
        Console.WriteLine($"Description: {result.Record.Description}");
        Console.WriteLine($"Score: {result.Score}");
        Console.WriteLine();
    }
}