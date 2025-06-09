using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Web.Components.BarChart
{
    public partial class BarChartComponent : ComponentBase
    {
        [Parameter]
        public List<TanqueViewModel>? Tanques { get; set; }

        private IEnumerable<string>? labels;
        private decimal[]? data;

        protected override async Task OnParametersSetAsync()
        {
            if (Tanques != null && Tanques.Any())
            {
                labels = Tanques.Select(t => $"{t.NoTanque.ToString()} - {t.Producto}");
                data = Tanques.Select(t => t.Volumen ?? 0).ToArray();
            }
        }
            
        
    }
}
