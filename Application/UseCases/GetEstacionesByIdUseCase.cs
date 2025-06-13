using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class GetEstacionesByIdUseCase
    {
        private readonly IRepository<EstacionesEntity> _repository;

        public GetEstacionesByIdUseCase(IRepository<EstacionesEntity> repository)
            => _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        /// <summary>
        /// Executes the use case to get a station by its ID
        /// </summary>
        /// <param name="id">The ID of the station to retrieve</param>
        /// <returns>The station entity if found</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid</exception>
        /// <exception cref="Infrastructure.Repositories.EntityNotFoundException">Thrown when no entity with the given ID exists</exception>
        /// <exception cref="Infrastructure.Repositories.DatabaseConnectionException">Thrown when a database connection error occurs</exception>
        /// <exception cref="Infrastructure.Repositories.RepositoryException">Thrown when a repository error occurs</exception>
        public async Task<EstacionesEntity> ExecuteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser un valor positivo", nameof(id));
            }

            return await _repository.GetByIdAsync(id);
        }
    }
}
