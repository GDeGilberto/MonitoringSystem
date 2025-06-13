using Application.Interfaces;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.UseCases
{
    public class GetDescargaSearchUseCase<TModel>
    {
        private readonly IRepositorySearch<TModel, DescargasEntity> _repository;
        
        public GetDescargaSearchUseCase(IRepositorySearch<TModel, DescargasEntity> repositorySearch) 
        {
            _repository = repositorySearch ?? throw new ArgumentNullException(nameof(repositorySearch));
        }

        /// <summary>
        /// Executes the use case to search for descargas based on a predicate
        /// </summary>
        /// <param name="predicate">The search predicate to apply</param>
        /// <returns>A collection of matching descarga entities</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null</exception>
        /// <exception cref="Infrastructure.Repositories.DatabaseConnectionException">Thrown when a database connection error occurs</exception>
        /// <exception cref="Infrastructure.Repositories.RepositoryException">Thrown when a repository error occurs</exception>
        public async Task<IEnumerable<DescargasEntity>> ExecuteAsync(Expression<Func<TModel, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "El predicado de búsqueda no puede ser nulo");
            }
            
            return await _repository.GetAsync(predicate);
        }
    }
}
