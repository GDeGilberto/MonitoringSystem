using Domain.Entities;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Web.Components.Tanques
{
    public partial class TanquesComponent : ComponentBase
    {
        [Parameter]
        public EstacionesEntity Estacion { get; set; }

        [Parameter]
        public List<TanqueViewModel>? Tanques { get; set; }

        [Parameter]
        public bool IsLoading { get; set; }

        private string FormatearDecimal(decimal? valor)
            => valor.HasValue ? valor.Value.ToString("N2") : "-";
    }
}
