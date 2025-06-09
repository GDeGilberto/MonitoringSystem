using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Web.Components.Estacion
{
    public partial class EstacionComponent : ComponentBase
    {
        [Parameter]
        public EstacionViewModel Estacion { get; set; }
    }
}
