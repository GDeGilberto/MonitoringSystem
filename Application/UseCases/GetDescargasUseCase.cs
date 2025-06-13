using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class GetDescargasUseCase
    {
        private readonly IRepository<DescargasEntity> _repository;

        public GetDescargasUseCase(IRepository<DescargasEntity> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Executes the use case to get all descargas
        /// </summary>
        /// <returns>A collection of all descarga entities</returns>
        /// <exception cref="Infrastructure.Repositories.DatabaseConnectionException">Thrown when a database connection error occurs</exception>
        /// <exception cref="Infrastructure.Repositories.RepositoryException">Thrown when a repository error occurs</exception>
        public async Task<IEnumerable<DescargasEntity>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
