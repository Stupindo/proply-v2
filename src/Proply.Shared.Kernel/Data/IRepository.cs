namespace Proply.Shared.Kernel.Data;

/// <summary>
/// Defines the contract for a generic repository to manage <see cref="BaseEntity"/> items.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique ID of the entity.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<T> GetByIdAsync(string id);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    Task AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity from the repository by its identifier.
    /// </summary>
    /// <param name="id">The unique ID of the entity to delete.</param>
    Task DeleteAsync(string id);
}
