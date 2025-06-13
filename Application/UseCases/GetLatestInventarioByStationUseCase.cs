using Application.Interfaces;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.UseCases
{
    public class GetLatestInventarioByStationUseCase<TModel>
    {
        private readonly IRepositorySearch<TModel, InventarioEntity> _repository;

        public GetLatestInventarioByStationUseCase(IRepositorySearch<TModel, InventarioEntity> repository)
            => _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        /// <summary>
        /// Executes the use case to get the latest inventory records that match the given predicate
        /// </summary>
        /// <param name="predicate">The predicate to filter inventory records</param>
        /// <returns>A collection of inventory entities that match the predicate</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null</exception>
        /// <exception cref="Infrastructure.Repositories.DatabaseConnectionException">Thrown when a database connection error occurs</exception>
        /// <exception cref="Infrastructure.Repositories.RepositoryException">Thrown when a repository error occurs</exception>
        public async Task<IEnumerable<InventarioEntity>> ExecuteAsync(Expression<Func<TModel, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "El predicado de búsqueda no puede ser nulo");
            }

            return await _repository.GetAsync(predicate);
        }
    }
}
