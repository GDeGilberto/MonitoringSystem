using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Dtos;

namespace Infrastructure.Mappers
{
    public class InventarioMapper : IMapper<InventarioRequestDTO, InventarioEntity>
    {
        public InventarioEntity ToEntity(InventarioRequestDTO dto)
        {
            return new InventarioEntity
            (
                dto.IdEstacion,
                dto.NoTanque,
                dto.ClaveProducto,
                dto.VolumenDisponible,
                dto.Temperatura,
                dto.Fecha
            );
        }
    }
}
