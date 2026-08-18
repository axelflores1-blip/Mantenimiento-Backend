using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mantenimiento.DTOs;
using Mantenimiento.Services;

namespace Mantenimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación JWT para todos los endpoints
    public class VehiculoController : ControllerBase
    {
        private readonly IVehiculoService _vehiculoService;

        public VehiculoController(IVehiculoService vehiculoService)
        {
            _vehiculoService = vehiculoService;
        }

        // GET: api/vehiculo (Administrador y Tecnico)
        [HttpGet]
        [Authorize(Roles = "Administrador,Tecnico")]
        public async Task<IActionResult> GetAll() => Ok(await _vehiculoService.GetAllAsync());

        // GET: api/vehiculo/mis-vehiculos (Cliente)
        [HttpGet("mis-vehiculos")]
        public async Task<IActionResult> GetMisVehiculos()
        {
            var vehiculos = await _vehiculoService.GetMisVehiculosAsync(GetUserIdFromToken());
            return Ok(vehiculos);
        }

        // GET: api/vehiculo/5 (Dueño o Administrador)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var vehiculo = await _vehiculoService.GetByIdAsync(id, GetUserIdFromToken(), User.IsInRole("Administrador"));
                if (vehiculo == null)
                    return NotFound(new { mensaje = "El vehículo solicitado no existe." });

                return Ok(vehiculo);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // POST: api/vehiculo (Cliente registra su propio vehículo)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehiculoCreateDto dto)
        {
            try
            {
                var nuevoVehiculo = await _vehiculoService.CreateAsync(GetUserIdFromToken(), dto);
                return CreatedAtAction(nameof(GetById), new { id = nuevoVehiculo.Id }, nuevoVehiculo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PUT: api/vehiculo/5 (Dueño o Administrador)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VehiculoUpdateDto dto)
        {
            try
            {
                var actualizado = await _vehiculoService.UpdateAsync(id, dto, GetUserIdFromToken(), User.IsInRole("Administrador"));
                if (actualizado == null)
                    return NotFound(new { mensaje = "El vehículo a actualizar no existe." });

                return Ok(actualizado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // PATCH: api/vehiculo/5/kilometraje (Dueño o Administrador)
        [HttpPatch("{id}/kilometraje")]
        public async Task<IActionResult> ActualizarKilometraje(int id, [FromBody] VehiculoKilometrajeDto dto)
        {
            try
            {
                var actualizado = await _vehiculoService.ActualizarKilometrajeAsync(id, dto, GetUserIdFromToken(), User.IsInRole("Administrador"));
                if (actualizado == null)
                    return NotFound(new { mensaje = "El vehículo no existe." });

                return Ok(actualizado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // PATCH: api/vehiculo/5/status (Solo Administrador)
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var actualizado = await _vehiculoService.ChangeStatusAsync(id);
            if (actualizado == null)
                return NotFound(new { mensaje = "El vehículo no existe." });

            return Ok(actualizado);
        }

        private int GetUserIdFromToken()
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(nameIdentifierClaim, out int usuarioId))
                return usuarioId;

            throw new UnauthorizedAccessException("Usuario no válido en el Token.");
        }
    }
}
