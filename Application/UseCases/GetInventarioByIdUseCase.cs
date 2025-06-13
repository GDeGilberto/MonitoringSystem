using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class GetInventarioByIdUseCase
    {
        private readonly IRepository<InventarioEntity> _repository;

        public GetInventarioByIdUseCase(IRepository<InventarioEntity> repository)
            => _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        /// <summary>
        /// Executes the use case to get an inventory record by its ID
        /// </summary>
        /// <param name="id">The ID of the inventory record to retrieve</param>
        /// <returns>The inventory entity if found</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid</exception>
        /// <exception cref="Infrastructure.Repositories.EntityNotFoundException">Thrown when no entity with the given ID exists</exception>
        /// <exception cref="Infrastructure.Repositories.DatabaseConnectionException">Thrown when a database connection error occurs</exception>
        /// <exception cref="Infrastructure.Repositories.RepositoryException">Thrown when a repository error occurs</exception>
        public async Task<InventarioEntity> ExecuteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser un valor positivo", nameof(id));
            }

            return await _repository.GetByIdAsync(id);
        }
    }
}
