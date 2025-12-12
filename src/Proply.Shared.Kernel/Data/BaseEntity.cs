using Newtonsoft.Json;

namespace Proply.Shared.Kernel.Data;

/// <summary>
/// Abstract base class for all entities stored in Cosmos DB.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Discriminator property to identify the entity type within a container.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class.
    /// </summary>
    /// <param name="type">The type discriminator.</param>
    protected BaseEntity(string type)
    {
        Type = type;
    }
}
