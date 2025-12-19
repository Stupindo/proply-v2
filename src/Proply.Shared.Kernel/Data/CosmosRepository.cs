using Microsoft.Azure.Cosmos;

namespace Proply.Shared.Kernel.Data;

/// <summary>
/// Generic repository implementation for Azure Cosmos DB using the SQL API.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class CosmosRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly Container _container;

    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosRepository{T}"/> class.
    /// </summary>
    /// <param name="cosmosClient">The Cosmos DB client instance.</param>
    /// <param name="databaseName">The name of the database.</param>
    /// <param name="containerName">The name of the container.</param>
    public CosmosRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            ItemResponse<T> response = await _container.ReadItemAsync<T>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task AddAsync(T entity)
    {
        await _container.CreateItemAsync(entity, new PartitionKey(entity.Id));
    }

    /// <inheritdoc />
    public async Task UpdateAsync(T entity)
    {
        await _container.UpsertItemAsync(entity, new PartitionKey(entity.Id));
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id)
    {
        await _container.DeleteItemAsync<T>(id, new PartitionKey(id));
    }
}
