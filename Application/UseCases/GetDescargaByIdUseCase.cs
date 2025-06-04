using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class GetDescargaByIdUseCase
    {
        private readonly IRepository<DescargasEntity> _repository;

        public GetDescargaByIdUseCase(IRepository<DescargasEntity> repository)
        {
            _repository = repository;
        }

        public async Task<DescargasEntity?> ExecuteAsync(int id)
            => await _repository.GetByIdAsync(id);
    }
}
