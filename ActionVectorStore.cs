using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

public class ActionVectorStore
{
    private readonly InMemoryVectorStore _store;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public ActionVectorStore()
    {
        _store = new InMemoryVectorStore();
        var store = _store.GetCollection<string, StoreAction>("actions");
        store.CreateCollectionIfNotExistsAsync().Wait(); // TODO: proper async?

        _embeddingGenerator = new OllamaEmbeddingGenerator(new Uri("http://localhost:11434/"), "all-minilm");

        var storeActions = ActionHelper.GetActionTypes().Select(actionType => new StoreAction
        {
            Type = actionType.Name,
            Description = ((ActionDescriptionAttribute?)Attribute.GetCustomAttribute(actionType, typeof(ActionDescriptionAttribute)))?.Description,
        });
        foreach(var storeAction in storeActions)
        {
            storeAction.Vector = _embeddingGenerator.GenerateEmbeddingVectorAsync(storeAction.Description).Result; // TODO: proper async?
            store.UpsertAsync(storeAction).Wait(); // TODO: proper async?
        }       
    }

    public async Task<VectorSearchResults<StoreAction>> QueryAsync(string query, VectorSearchOptions? searchOptions = null)
    {
        var queryEmbedding = await _embeddingGenerator.GenerateEmbeddingVectorAsync(query);

        return await _store.GetCollection<string, StoreAction>("actions")
            .VectorizedSearchAsync(queryEmbedding, searchOptions);
    }
}