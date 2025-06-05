using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class GetEstacionesUseCase
    {
        private readonly IRepository<EstacionesEntity> _repository;
        public GetEstacionesUseCase(IRepository<EstacionesEntity> repository)
            => _repository = repository;

        public async Task<IEnumerable<EstacionesEntity>> ExecuteAsync()
            => await _repository.GetAllAsync();
    }
}
