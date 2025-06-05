using Application.Interfaces;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.UseCases
{
    public class GetLatestInventarioByStationUseCase<TModel>
    {
        private readonly IRepositorySearch<TModel, InventarioEntity> _repository;

        public GetLatestInventarioByStationUseCase(IRepositorySearch<TModel, InventarioEntity> repository)
            => _repository = repository;

        public async Task<IEnumerable<InventarioEntity>> ExecuteAsync(Expression<Func<TModel, bool>> predicate)
            => await _repository.GetAsync(predicate);
    }
}
