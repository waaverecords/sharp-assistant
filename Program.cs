using System.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

var actionTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => typeof(Action).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        foreach (var actionType in actionTypes)
            services.AddScoped(actionType);
    })
    .Build();

var storeActions = actionTypes.Select(actionType => new StoreAction
{
    Type = actionType.Name,
    Description = ((ActionDescriptionAttribute?)Attribute.GetCustomAttribute(actionType, typeof(ActionDescriptionAttribute)))?.Description,
});

// https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-vector-data/?utm_source=chatgpt.com
var vectorStore = new InMemoryVectorStore();
var store = vectorStore.GetCollection<string, StoreAction>("actions");
await store.CreateCollectionIfNotExistsAsync();

IEmbeddingGenerator<string, Embedding<float>> generator =  new OllamaEmbeddingGenerator(new Uri("http://localhost:11434/"), "all-minilm");

foreach(var storeAction in storeActions)
{
    storeAction.Vector = await generator.GenerateEmbeddingVectorAsync(storeAction.Description);
    await store.UpsertAsync(storeAction);
}

var query = "i want to insert a product";
var queryEmbedding = await generator.GenerateEmbeddingVectorAsync(query);

var searchOptions = new VectorSearchOptions()
{
    Top = 3,
    VectorPropertyName = "Vector"
};

var results = await store.VectorizedSearchAsync(queryEmbedding, searchOptions);

await foreach(var result in results.Results)
{
    Console.WriteLine($"Type: {result.Record.Type}");
    Console.WriteLine($"Description: {result.Record.Description}");
    Console.WriteLine($"Score: {result.Score}");
    Console.WriteLine();
}