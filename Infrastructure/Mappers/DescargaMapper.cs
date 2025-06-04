using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Dtos;

namespace Infrastructure.Mappers
{
    public class DescargaMapper : IMapper<DescargaRequestDTO, DescargasEntity>
    {
        public DescargasEntity ToEntity(DescargaRequestDTO dto)
        {
            return new DescargasEntity
            (
                dto.IdEstacion ?? 0,
                dto.NoTanque ?? 0,
                dto.VolumenInicial ?? 0,
                dto.TemperaturaInicial,
                dto.FechaInicial ?? DateTime.Now,
                dto.VolumenDisponible ?? 0,
                dto.TemperaturaFinal,
                dto.FechaFinal ?? DateTime.Now.AddMinutes(5),
                dto.CantidadCargada ?? 0
            );
        }
    }
}
