using Application.UseCases;
using BlazorDateRangePicker;
using Domain.Entities;
using Infrastructure.Jobs;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;

namespace Web.Components.Pages
{
    public partial class Recargas : ComponentBase
    {
        [Inject] private DescargasJobs? _descargasJobs { get; set; }
        [Inject] private GetDescargaSearchUseCase<ProcDescargaModel> descargaSearchUseCase { get; set; } = default!;
        [Inject] private GetEstacionesByIdUseCase estacionesByIdUseCase { get; set; } = default!;
        [Inject] private IConfiguration Configuration { get; set; } = default!;

        private IEnumerable<DescargasEntity> DescargasEntities;
        private EstacionesEntity EstacionEntity;

        public DateTimeOffset? startDate { get; set; } =
            new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        public DateTimeOffset? endDate { get; set; } = 
            DateTime.Today.AddDays(1).AddTicks(-1);

        public IEnumerable<int> TankNumbers =>
            EstacionEntity?.Tanques?
                .Where(t => int.TryParse(t.NoTanque, out _))
                .Select(t => int.Parse(t.NoTanque!))
                .Distinct()
                .OrderBy(n => n)
            ?? Enumerable.Empty<int>();

        private int selectedOption = 0;
        private int idEstacion;

        public bool isLoadingTable = true;

        public IEnumerable<DescargasViewModel> data { get; set; }

        public IEnumerable<string> titles =
        [
            "No. Tanque",
            "Vol. Inicial (m3)",
            "Temp. Ini. (°C)",
            "Fecha Inial.",
            "Vol. Disponible (m3)",
            "Temp. Final (°C)",
            "Fecha Final",
            "Cant. Cargada (m3)"
        ];

        
        protected override async Task OnInitializedAsync()
        {
            idEstacion = Configuration.GetValue<int>("Estacion:Id");
            EstacionEntity = await estacionesByIdUseCase.ExecuteAsync(idEstacion);

            await GetData();
            BuildViewModel();
            isLoadingTable = false;
        }

        public async Task ClickFilterData()
        {
            isLoadingTable = true;

            await GetData();
            BuildViewModel();

            isLoadingTable = false;
        }

        public async Task ClickUpdateRecargas()
        {
            isLoadingTable = true;
            try
            {
                await _descargasJobs!.Execute();
                
                await GetData();
                BuildViewModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating recargas: {ex.Message}");
            }
            finally
            {
                isLoadingTable = false;
            }
        }

        private async Task GetData()
        {
            DescargasEntities = await descargaSearchUseCase
                .ExecuteAsync(s => s.IdEstacion == idEstacion
                && (selectedOption == 0 || s.NoTanque == selectedOption)
                && (startDate == null || s.FechaInicio >= startDate.Value.DateTime)
                && (endDate == null || s.FechaInicio <= endDate.Value.DateTime));
        }

        private void BuildViewModel()
        {
            if (!DescargasEntities.IsNullOrEmpty())
            {
                data = DescargasEntities
                .OrderByDescending(data => data.FechaInicial)
                .Select(data => new DescargasViewModel
                {
                    NoTanque = data.NoTanque,
                    VolumenInicial = data.VolumenInicial.ToString("N2"),
                    TemperaturaInicial = data.TemperaturaInicial?.ToString("N2") ?? "",
                    FechaInicial = data.FechaInicial.ToString("d/M/yy hh:mm tt"),
                    VolumenDisponible = data.VolumenDisponible.ToString("N2"),
                    TemperaturaFinal = data.TemperaturaFinal?.ToString("N2") ?? "",
                    FechaFinal = data.FechaFinal.ToString("d/M/yy hh:mm tt"),
                    CantidadCargada = data.CantidadCargada.ToString("N2"),
                });
            }
            StateHasChanged();
        }

        public void SetToday(DateRangePicker context)
        {
            var today = DateTime.Today;
            startDate = new DateTimeOffset(today);
            endDate = new DateTimeOffset(today.AddDays(1).AddTicks(-1));
            context.TStartDate = startDate;
            context.TEndDate = endDate;
            context.Close();
            StateHasChanged();
        }

        public void resetDays(DateRangePicker context)
        {
            startDate = null;
            endDate = null;
            context.TStartDate = startDate;
            context.TEndDate = endDate;
            context.Close();
            StateHasChanged();
        }
    }
}
