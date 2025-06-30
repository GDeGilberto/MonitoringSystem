using Application.UseCases;
using Domain.Entities;
using Infrastructure.Dtos;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para la gesti�n de descargas de combustible
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class DescargasController : ControllerBase
    {
        private readonly GetDescargasUseCase _getDescargasUseCase;
        private readonly GetDescargaByIdUseCase _getDescargaByIdUseCase;
        private readonly GetDescargaSearchUseCase<ProcDescargaModel> _getDescargaSearchUseCase;
        private readonly CreateDescargasUseCase<DescargaRequestDTO> _createDescargasUseCase;
        private readonly ILogger<DescargasController> _logger;

        public DescargasController(
            GetDescargasUseCase getDescargasUseCase,
            GetDescargaByIdUseCase getDescargaByIdUseCase,
            GetDescargaSearchUseCase<ProcDescargaModel> getDescargaSearchUseCase,
            CreateDescargasUseCase<DescargaRequestDTO> createDescargasUseCase,
            ILogger<DescargasController> logger)
        {
            _getDescargasUseCase = getDescargasUseCase;
            _getDescargaByIdUseCase = getDescargaByIdUseCase;
            _getDescargaSearchUseCase = getDescargaSearchUseCase;
            _createDescargasUseCase = createDescargasUseCase;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los registros de descargas
        /// </summary>
        /// <returns>Lista de todos los registros de descargas</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtiene todos los registros de descargas",
            Description = "Retorna la lista completa de descargas en el sistema",
            OperationId = "GetDescargas",
            Tags = new[] { "Descargas" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<IEnumerable<DescargasEntity>>> Get()
        {
            var result = await _getDescargasUseCase.ExecuteAsync();
            
            if (result == null || !result.Any())
            {
                _logger.LogInformation("No descargas records found");
                return NoContent();
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una descarga por su ID
        /// </summary>
        /// <param name="id">ID de la descarga a consultar</param>
        /// <returns>La descarga solicitada</returns>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtiene una descarga por su ID",
            Description = "Retorna informaci�n detallada de una descarga espec�fica",
            OperationId = "GetDescargaById",
            Tags = new[] { "Descargas" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<DescargasEntity>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid descarga ID provided: {Id}", id);
                return BadRequest(new { message = "El ID proporcionado no es v�lido" });
            }

            var result = await _getDescargaByIdUseCase.ExecuteAsync(id);
            
            _logger.LogInformation("Descarga with ID {Id} retrieved successfully", id);
            return Ok(result);
        }

        /// <summary>
        /// Busca descargas por ID de estaci�n y n�mero de tanque
        /// </summary>
        /// <param name="idEstacion">ID de la estaci�n</param>
        /// <param name="noTanque">N�mero del tanque</param>
        /// <returns>Lista de descargas que coinciden con los criterios de b�squeda</returns>
        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "Busca descargas por estaci�n y tanque",
            Description = "Filtra descargas seg�n el ID de estaci�n y n�mero de tanque",
            OperationId = "SearchDescargas",
            Tags = new[] { "Descargas" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<IEnumerable<DescargasEntity>>> GetByEstacionAndTanque(
            [FromQuery][Required] int? idEstacion, 
            [FromQuery][Required] int? noTanque)
        {
            if (!idEstacion.HasValue || !noTanque.HasValue)
            {
                return BadRequest(new { 
                    message = "Se requieren tanto el ID de estaci�n como el n�mero de tanque" 
                });
            }

            if (idEstacion <= 0 || noTanque <= 0)
            {
                return BadRequest(new { 
                    message = "El ID de estaci�n y n�mero de tanque deben ser valores positivos" 
                });
            }

            var result = await _getDescargaSearchUseCase.ExecuteAsync(
                d => d.IdEstacion == idEstacion.Value && d.NoTanque == noTanque.Value);
            
            if (result == null || !result.Any())
            {
                _logger.LogInformation("No descargas found for Estacion {IdEstacion} and Tanque {NoTanque}", 
                    idEstacion, noTanque);
                return NotFound(new { 
                    message = $"No se encontraron descargas para la estaci�n {idEstacion} y tanque {noTanque}" 
                });
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo registro de descarga
        /// </summary>
        /// <param name="descargaRequest">Datos de la descarga a crear</param>
        /// <returns>Respuesta indicando el �xito o fracaso de la operaci�n</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Crea un nuevo registro de descarga",
            Description = "Registra una nueva descarga en el sistema",
            OperationId = "CreateDescarga",
            Tags = new[] { "Descargas" }
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> Create([FromBody] DescargaRequestDTO descargaRequest)
        {
            if (descargaRequest == null)
            {
                return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vac�o" });
            }

            var validationErrors = ValidateDescargaRequest(descargaRequest);
            if (validationErrors.Any())
            {
                return BadRequest(new { message = "Datos de descarga inv�lidos", errors = validationErrors });
            }

            await _createDescargasUseCase.ExecuteAsync(descargaRequest);
            
            _logger.LogInformation(
                "Descarga created successfully for Estacion {IdEstacion} and Tanque {NoTanque}",
                descargaRequest.IdEstacion, descargaRequest.NoTanque);
            
            return Created($"/api/descargas", new { 
                message = "Descarga creada con �xito", 
                idEstacion = descargaRequest.IdEstacion,
                noTanque = descargaRequest.NoTanque
            });
        }

        private static List<string> ValidateDescargaRequest(DescargaRequestDTO request)
        {
            var errors = new List<string>();
            
            if (!request.IdEstacion.HasValue || request.IdEstacion <= 0)
                errors.Add("El ID de estaci�n es obligatorio y debe ser positivo");
                
            if (!request.NoTanque.HasValue || request.NoTanque <= 0)
                errors.Add("El n�mero de tanque es obligatorio y debe ser positivo");
                
            if (!request.VolumenInicial.HasValue || request.VolumenInicial <= 0)
                errors.Add("El volumen inicial es obligatorio y debe ser positivo");
                
            if (!request.FechaInicial.HasValue)
                errors.Add("La fecha inicial es obligatoria");
                
            if (!request.VolumenDisponible.HasValue || request.VolumenDisponible <= 0)
                errors.Add("El volumen disponible es obligatorio y debe ser positivo");
                
            if (!request.FechaFinal.HasValue)
                errors.Add("La fecha final es obligatoria");
            
            return errors;
        }
    }
}