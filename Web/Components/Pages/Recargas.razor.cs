using Application.UseCases;
using BlazorDateRangePicker;
using Domain.Entities;
using Infrastructure.Jobs;
using Infrastructure.Models;
using Infrastructure.Services;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System.Data;

namespace Web.Components.Pages
{
    public partial class Recargas : ComponentBase
    {
        [Inject] private DescargasJobs? _descargasJobs { get; set; }
        [Inject] private GetDescargaSearchUseCase<ProcDescargaModel> descargaSearchUseCase { get; set; } = default!;
        [Inject] private GetEstacionesByIdUseCase estacionesByIdUseCase { get; set; } = default!;
        [Inject] private IConfiguration Configuration { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private IExcelExportService excelExportService { get; set; } = default!;

        private IEnumerable<DescargasEntity> DescargasEntities = Enumerable.Empty<DescargasEntity>();
        private EstacionesEntity? EstacionEntity;

        public DateTimeOffset? startDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        public DateTimeOffset? endDate { get; set; } = DateTime.Today.AddDays(1).AddTicks(-1);

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
        private bool isExporting = false;

        public IEnumerable<DescargasViewModel> data { get; set; } = Enumerable.Empty<DescargasViewModel>();

        public IEnumerable<string> titles =
        [
            "No. Tanque",
            "Vol. Inicial (m3)",
            "Temp. Ini. (°C)",
            "Fecha Inicial",
            "Vol. Disponible (m3)",
            "Temp. Final (°C)",
            "Fecha Final",
            "Cant. Cargada (m3)"
        ];

        protected override async Task OnInitializedAsync()
        {
            idEstacion = Configuration.GetValue<int>("Estacion:Id");
            EstacionEntity = await estacionesByIdUseCase.ExecuteAsync(idEstacion);

            await LoadDataAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("tooltipManager");
        }

        public async Task ClickFilterData()
        {
            await LoadDataAsync();
        }

        public async Task ClickDownloadExcel()
        {
            if (isExporting) return;

            isExporting = true;
            StateHasChanged();

            // Add a small delay to ensure the UI updates with the spinner
            await Task.Delay(100);

            try
            {
                if (!data.Any())
                {
                    await JSRuntime.InvokeVoidAsync("alert", "No hay datos para exportar.");
                    return;
                }

                // Generate filename with current date and filters
                var fileName = GenerateFileName();
                
                // Generate title for the Excel file
                var title = GenerateExcelTitle();

                // Export data to Excel (this operation might take some time)
                var excelBytes = excelExportService.ExportToExcel(data, "Recargas", title);

                // Add another small delay before download to show spinner longer for better UX
                await Task.Delay(500);

                // Download the Excel file using the specific Excel download function
                await JSRuntime.InvokeVoidAsync("downloadExcelFile", fileName, Convert.ToBase64String(excelBytes));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting to Excel: {ex.Message}");
                await JSRuntime.InvokeVoidAsync("alert", "Error al exportar los datos. Por favor, intente nuevamente.");
            }
            finally
            {
                isExporting = false;
                StateHasChanged();
            }
        }

        public async Task ClickUpdateRecargas()
        {
            isLoadingTable = true;
            try
            {
                if (_descargasJobs != null)
                {
                    await _descargasJobs.Execute();
                }
                await LoadDataAsync();
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

        private async Task LoadDataAsync()
        {
            isLoadingTable = true;
            
            try
            {
                await GetData();
                BuildViewModel();
            }
            finally
            {
                isLoadingTable = false;
                StateHasChanged();
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
                .OrderByDescending(item => item.FechaInicial)
                .Select(item => new DescargasViewModel
                {
                    NoTanque = item.NoTanque,
                    VolumenInicial = item.VolumenInicial.ToString("N2"),
                    TemperaturaInicial = item.TemperaturaInicial?.ToString("N2") ?? "",
                    FechaInicial = item.FechaInicial.ToString("d/M/yy hh:mm tt"),
                    VolumenDisponible = item.VolumenDisponible.ToString("N2"),
                    TemperaturaFinal = item.TemperaturaFinal?.ToString("N2") ?? "",
                    FechaFinal = item.FechaFinal.ToString("d/M/yy hh:mm tt"),
                    CantidadCargada = item.CantidadCargada.ToString("N2"),
                });
            }
            else
            {
                data = Enumerable.Empty<DescargasViewModel>();
            }
            
            StateHasChanged();
        }

        private string GenerateFileName()
        {
            var baseFileName = "Recargas";
            var timestamp = DateTime.Now.ToString("dd/MM/yy");
            
            var filterInfo = string.Empty;
            
            if (selectedOption > 0)
            {
                filterInfo += $"_Tanque{selectedOption}";
            }
            
            if (startDate.HasValue || endDate.HasValue)
            {
                var startStr = startDate?.ToString("dd/MM/yy") ?? "inicio";
                var endStr = endDate?.ToString("dd/MM/yy") ?? "fin";
                filterInfo += $"_Del_{startStr}_Al_{endStr}";
            }
            
            return $"{baseFileName}{filterInfo}_{timestamp}.xlsx";
        }

        private string GenerateExcelTitle()
        {
            var title = $"Reporte de Compras - {EstacionEntity?.Nombre ?? "Estación"}";
            
            if (selectedOption > 0)
            {
                title += $" - Tanque {selectedOption}";
            }
            
            if (startDate.HasValue || endDate.HasValue)
            {
                var dateRange = string.Empty;
                if (startDate.HasValue && endDate.HasValue)
                {
                    dateRange = $"Del {startDate.Value:dd/MM/yy} al {endDate.Value:dd/MM/yy}";
                }
                else if (startDate.HasValue)
                {
                    dateRange = $"Desde {startDate.Value:dd/MM/yy}";
                }
                else if (endDate.HasValue)
                {
                    dateRange = $"Hasta {endDate.Value:dd/MM/yy}";
                }
                title += $" - {dateRange}";
            }
            
            title += $" - Generado el {DateTime.Now:dd/MM/yy HH:mm}";
            
            return title;
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
