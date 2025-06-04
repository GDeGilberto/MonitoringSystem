using Application.Interfaces;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.UseCases
{
    public class GetDescargaSearchUseCase<TModel>
    {
        private readonly IRepositorySearch<TModel, DescargasEntity> _repository;
        public GetDescargaSearchUseCase(IRepositorySearch<TModel, DescargasEntity> repositorySearch) 
            => _repository = repositorySearch;

        public async Task<IEnumerable<DescargasEntity>> ExecuteAsync(Expression<Func<TModel, bool>> predicate)
            => await _repository.GetAsync(predicate);
    }
}
