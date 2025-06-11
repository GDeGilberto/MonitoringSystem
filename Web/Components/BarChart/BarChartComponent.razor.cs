using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Components.BarChart
{
    public partial class BarChartComponent : ComponentBase
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Parameter]
        public List<TanqueViewModel>? Tanques { get; set; }

        private IEnumerable<string>? labels;
        private decimal[]? data;
        private bool datosActualizados = false;

        protected override void OnParametersSet()
        {
            if (Tanques != null && Tanques.Any())
            {
                labels = Tanques.Select(t => $"{t.NoTanque.ToString()} - {t.Producto}");
                data = Tanques.Select(t => t.Volumen ?? 0).ToArray();
                datosActualizados = true;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (datosActualizados && labels != null && data != null)
            {
                datosActualizados = false;
                await JSRuntime.InvokeVoidAsync("HandleBarChart", "myChart", labels, data);
            }
        }
    }
}