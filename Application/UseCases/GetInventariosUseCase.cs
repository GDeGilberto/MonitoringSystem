using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class GetInventariosUseCase
    {
        private IRepository<InventarioEntity> _repository;

        public GetInventariosUseCase(IRepository<InventarioEntity> repository)
            => _repository = repository;
 

        public async Task<IEnumerable<InventarioEntity>> ExecuteAsync()
            => await _repository.GetAllAsync();
    }
}
