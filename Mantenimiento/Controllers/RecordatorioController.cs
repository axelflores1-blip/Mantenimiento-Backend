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
    public class RecordatorioController : ControllerBase
    {
        private readonly IRecordatorioService _recordatorioService;

        public RecordatorioController(IRecordatorioService recordatorioService)
        {
            _recordatorioService = recordatorioService;
        }

        // GET: api/recordatorio/vehiculo/5 (Dueño del vehículo, Administrador o Tecnico)
        [HttpGet("vehiculo/{vehiculoId}")]
        public async Task<IActionResult> GetByVehiculo(int vehiculoId)
        {
            try
            {
                bool permisoAmplio = User.IsInRole("Administrador") || User.IsInRole("Tecnico");
                var recordatorios = await _recordatorioService.GetByVehiculoIdAsync(vehiculoId, GetUserIdFromToken(), permisoAmplio);
                return Ok(recordatorios);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // POST: api/recordatorio (Administrador o Tecnico)
        [HttpPost]
        [Authorize(Roles = "Administrador,Tecnico")]
        public async Task<IActionResult> Create([FromBody] RecordatorioCreateDto dto)
        {
            try
            {
                var nuevo = await _recordatorioService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetByVehiculo), new { vehiculoId = nuevo.VehiculoId }, nuevo);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // DELETE: api/recordatorio/5 (Administrador o Tecnico)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Tecnico")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _recordatorioService.DeleteAsync(id);
            if (!eliminado)
                return NotFound(new { mensaje = "Recordatorio no encontrado." });

            return Ok(new { mensaje = "Recordatorio eliminado exitosamente." });
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
