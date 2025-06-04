using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class GetInventarioByIdUseCase
    {
        private readonly IRepository<InventarioEntity> _repository;

        public GetInventarioByIdUseCase(IRepository<InventarioEntity> repository)
            => _repository = repository;

        public async Task<InventarioEntity?> ExecuteAsync(int id)
            => await _repository.GetByIdAsync(id);
    }
}
