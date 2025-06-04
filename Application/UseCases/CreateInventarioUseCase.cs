using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class CreateInventarioUseCase<TDTO>
    {
        private readonly IRepository<InventarioEntity> _repository;
        private readonly IMapper<TDTO, InventarioEntity> _mapper;
        public CreateInventarioUseCase(IRepository<InventarioEntity> repository,
                                       IMapper<TDTO, InventarioEntity> mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task ExecuteAsync(TDTO inventarioDTO)
        {
            var inventario = _mapper.ToEntity(inventarioDTO);



            await _repository.AddAsync(inventario);
        }
    }
}
