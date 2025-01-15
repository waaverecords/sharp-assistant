using Microsoft.Extensions.VectorData;

public class StoreAction
{
    [VectorStoreRecordKey]
    public string Type { get; set;}

    [VectorStoreRecordData]
    public string Description { get; set;}

    // MiniLM embeddings with dimensionality of 384
    [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}