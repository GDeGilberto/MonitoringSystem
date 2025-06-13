using Domain.Entities;
using Application.UseCases;
using Infrastructure.Dtos;
using Infrastructure.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        /// Get all descargas records
        /// </summary>
        /// <returns>A list of all descargas</returns>
        /// <response code="200">Returns the list of descargas</response>
        /// <response code="204">If no descargas records exist</response>
        /// <response code="500">If there was an error processing the request</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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
        /// Get a descarga by its ID
        /// </summary>
        /// <param name="id">The ID of the descarga to retrieve</param>
        /// <returns>The requested descarga</returns>
        /// <response code="200">Returns the requested descarga</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="404">If the descarga was not found</response>
        /// <response code="500">If there was an error processing the request</response>
        /// <response code="503">If there was a database connection error</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<DescargasEntity>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid descarga ID provided: {Id}", id);
                return BadRequest(new { message = "El ID proporcionado no es válido" });
            }

            var result = await _getDescargaByIdUseCase.ExecuteAsync(id);
            
            // If we reach here, the entity was found (otherwise an EntityNotFoundException would have been thrown)
            _logger.LogInformation("Descarga with ID {Id} retrieved successfully", id);
            return Ok(result);
        }

        /// <summary>
        /// Search for descargas by estacion ID and tanque number
        /// </summary>
        /// <param name="idEstacion">The ID of the estacion</param>
        /// <param name="noTanque">The tank number</param>
        /// <returns>A list of matching descargas</returns>
        /// <response code="200">Returns the list of matching descargas</response>
        /// <response code="400">If the parameters are invalid</response>
        /// <response code="404">If no matching descargas were found</response>
        /// <response code="500">If there was an error processing the request</response>
        /// <response code="503">If there was a database connection error</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                    message = "Se requieren tanto el ID de estación como el número de tanque" 
                });
            }

            if (idEstacion <= 0 || noTanque <= 0)
            {
                return BadRequest(new { 
                    message = "El ID de estación y número de tanque deben ser valores positivos" 
                });
            }

            var result = await _getDescargaSearchUseCase.ExecuteAsync(
                d => d.IdEstacion == idEstacion.Value && d.NoTanque == noTanque.Value);
            
            if (result == null || !result.Any())
            {
                _logger.LogInformation("No descargas found for Estacion {IdEstacion} and Tanque {NoTanque}", 
                    idEstacion, noTanque);
                return NotFound(new { 
                    message = $"No se encontraron descargas para la estación {idEstacion} y tanque {noTanque}" 
                });
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Create a new descarga
        /// </summary>
        /// <param name="descargaRequest">The descarga data</param>
        /// <returns>A response indicating success or failure</returns>
        /// <response code="201">If the descarga was created successfully</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="409">If there is a conflict with an existing record</response>
        /// <response code="500">If there was an error processing the request</response>
        /// <response code="503">If there was a database connection error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> Create([FromBody] DescargaRequestDTO descargaRequest)
        {
            if (descargaRequest == null)
            {
                return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vacío" });
            }

            // Validate required fields
            var validationErrors = new List<string>();
            
            if (!descargaRequest.IdEstacion.HasValue || descargaRequest.IdEstacion <= 0)
                validationErrors.Add("El ID de estación es obligatorio y debe ser positivo");
                
            if (!descargaRequest.NoTanque.HasValue || descargaRequest.NoTanque <= 0)
                validationErrors.Add("El número de tanque es obligatorio y debe ser positivo");
                
            if (!descargaRequest.VolumenInicial.HasValue || descargaRequest.VolumenInicial <= 0)
                validationErrors.Add("El volumen inicial es obligatorio y debe ser positivo");
                
            if (!descargaRequest.FechaInicial.HasValue)
                validationErrors.Add("La fecha inicial es obligatoria");
                
            if (!descargaRequest.VolumenDisponible.HasValue || descargaRequest.VolumenDisponible <= 0)
                validationErrors.Add("El volumen disponible es obligatorio y debe ser positivo");
                
            if (!descargaRequest.FechaFinal.HasValue)
                validationErrors.Add("La fecha final es obligatoria");
            
            if (validationErrors.Any())
            {
                return BadRequest(new { message = "Datos de descarga inválidos", errors = validationErrors });
            }

            // Let any exceptions bubble up to the global exception handler middleware
            await _createDescargasUseCase.ExecuteAsync(descargaRequest);
            
            _logger.LogInformation(
                "Descarga created successfully for Estacion {IdEstacion} and Tanque {NoTanque}",
                descargaRequest.IdEstacion, descargaRequest.NoTanque);
            
            // Since we don't know the generated ID, we return the resource location as a general endpoint
            return Created($"/api/descargas", new { 
                message = "Descarga creada con éxito", 
                idEstacion = descargaRequest.IdEstacion,
                noTanque = descargaRequest.NoTanque
            });
        }
    }
}