using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Web.Components.Productos
{
    public partial class ProductosComponent : ComponentBase
    {
        [Parameter]
        public List<ProductoViewModel> Productos { get; set; }

        [Parameter]
        public bool IsLoading { get; set; }

        private string FormatearDecimal(decimal? valor)
            => valor.HasValue ? valor.Value.ToString("N2") : "-";
    }
}
