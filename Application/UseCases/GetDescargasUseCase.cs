using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class GetDescargasUseCase
    {
        private readonly IRepository<DescargasEntity> _repository;

        public GetDescargasUseCase(IRepository<DescargasEntity> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DescargasEntity>> ExecuteAsync()
            => await _repository.GetAllAsync();

    }
}
