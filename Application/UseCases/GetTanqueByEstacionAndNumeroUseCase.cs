using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class GetTanqueByEstacionAndNumeroUseCase
    {
        private readonly ITanqueRepository _tanqueRepository;

        public GetTanqueByEstacionAndNumeroUseCase(ITanqueRepository tanqueRepository)
        {
            _tanqueRepository = tanqueRepository;
        }

        public async Task<TanqueEntity?> ExecuteAsync(int idEstacion, int noTanque)
        {
            return await _tanqueRepository.GetTanqueByEstacionAndNumeroAsync(idEstacion, noTanque);
        }
    }
}