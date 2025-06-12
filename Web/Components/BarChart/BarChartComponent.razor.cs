using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Components.BarChart
{
    public partial class BarChartComponent : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; }
        [Parameter] public List<TanqueViewModel>? Tanques { get; set; }

        private string[]? _labels;
        private decimal[]? _data;

        protected override void OnParametersSet()
        {
            if (Tanques?.Any() == true)
            {
                _labels = Tanques.Select(t => $"{t.NoTanque.ToString()} - {t.Producto}").ToArray();
                _data = Tanques.Select(t => t.Volumen ?? 0).ToArray();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if ( _labels != null && _data != null)
            {
                await JS.InvokeVoidAsync("UpdateChart", "myChart", _labels, _data);
            }
        }

        public async ValueTask DisposeAsync() =>
            await JS.InvokeVoidAsync("DestroyChart");

    }
}