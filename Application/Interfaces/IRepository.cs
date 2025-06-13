namespace Application.Interfaces
{
    public interface IRepository<T>
    {
        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve</param>
        /// <returns>The entity if found, or throws EntityNotFoundException if not found</returns>
        Task<T> GetByIdAsync(int id);
        
        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>A collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        Task AddAsync(T entity);
    }
}
