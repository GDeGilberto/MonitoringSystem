using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class GetEstacionesByIdUseCase
    {
        private readonly IRepository<EstacionesEntity> _repository;

        public GetEstacionesByIdUseCase(IRepository<EstacionesEntity> repository)
            => _repository = repository;

        public async Task<EstacionesEntity?> ExecuteAsync(int id)
            => await _repository.GetByIdAsync(id);
    }
}
