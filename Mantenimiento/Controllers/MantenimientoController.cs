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
    public class MantenimientoController : ControllerBase
    {
        private readonly IMantenimientoService _mantenimientoService;

        public MantenimientoController(IMantenimientoService mantenimientoService)
        {
            _mantenimientoService = mantenimientoService;
        }

        // GET: api/mantenimiento (Solo Administrador)
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetAll() => Ok(await _mantenimientoService.GetAllAsync());

        // GET: api/mantenimiento/vehiculo/5 (Dueño del vehículo o Administrador)
        [HttpGet("vehiculo/{vehiculoId}")]
        public async Task<IActionResult> GetByVehiculo(int vehiculoId)
        {
            try
            {
                var mantenimientos = await _mantenimientoService.GetByVehiculoIdAsync(vehiculoId, GetUserIdFromToken(), User.IsInRole("Administrador"));
                return Ok(mantenimientos);
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

        // GET: api/mantenimiento/mis-asignados (Tecnico)
        [HttpGet("mis-asignados")]
        [Authorize(Roles = "Tecnico")]
        public async Task<IActionResult> GetMisAsignados()
        {
            var mantenimientos = await _mantenimientoService.GetAsignadosATecnicoAsync(GetUserIdFromToken());
            return Ok(mantenimientos);
        }

        // GET: api/mantenimiento/5 (Dueño del vehículo, técnico asignado, o Administrador)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var mantenimiento = await _mantenimientoService.GetByIdAsync(
                    id, GetUserIdFromToken(), User.IsInRole("Administrador"), User.IsInRole("Tecnico"));

                if (mantenimiento == null)
                    return NotFound(new { mensaje = "Mantenimiento no encontrado." });

                return Ok(mantenimiento);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // POST: api/mantenimiento (Cliente solicita mantenimiento)
        [HttpPost]
        public async Task<IActionResult> Solicitar([FromBody] MantenimientoSolicitudDto dto)
        {
            try
            {
                var nuevo = await _mantenimientoService.SolicitarAsync(GetUserIdFromToken(), dto);
                return CreatedAtAction(nameof(GetById), new { id = nuevo.Id }, nuevo);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
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

        // PATCH: api/mantenimiento/5/asignar (Solo Administrador) -> Estado: Recibido
        [HttpPatch("{id}/asignar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AsignarTecnico(int id, [FromBody] MantenimientoAsignarTecnicoDto dto)
        {
            try
            {
                var actualizado = await _mantenimientoService.AsignarTecnicoAsync(id, dto);
                if (actualizado == null)
                    return NotFound(new { mensaje = "Mantenimiento no encontrado." });

                return Ok(actualizado);
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

        // PATCH: api/mantenimiento/5/diagnostico (Técnico asignado) -> Estado: EnDiagnostico
        [HttpPatch("{id}/diagnostico")]
        [Authorize(Roles = "Tecnico")]
        public async Task<IActionResult> RegistrarDiagnostico(int id, [FromBody] MantenimientoDiagnosticoDto dto)
        {
            try
            {
                var actualizado = await _mantenimientoService.RegistrarDiagnosticoAsync(id, GetUserIdFromToken(), dto);
                if (actualizado == null)
                    return NotFound(new { mensaje = "Mantenimiento no encontrado." });

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

        // PATCH: api/mantenimiento/5/trabajo (Técnico asignado) -> Estado: EnReparacion
        [HttpPatch("{id}/trabajo")]
        [Authorize(Roles = "Tecnico")]
        public async Task<IActionResult> RegistrarTrabajo(int id, [FromBody] MantenimientoTrabajoDto dto)
        {
            try
            {
                var actualizado = await _mantenimientoService.RegistrarTrabajoAsync(id, GetUserIdFromToken(), dto);
                if (actualizado == null)
                    return NotFound(new { mensaje = "Mantenimiento no encontrado." });

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

        // PATCH: api/mantenimiento/5/finalizar (Técnico asignado) -> Estado: Finalizado
        [HttpPatch("{id}/finalizar")]
        [Authorize(Roles = "Tecnico")]
        public async Task<IActionResult> Finalizar(int id)
        {
            try
            {
                var actualizado = await _mantenimientoService.FinalizarAsync(id, GetUserIdFromToken());
                if (actualizado == null)
                    return NotFound(new { mensaje = "Mantenimiento no encontrado." });

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

        // PATCH: api/mantenimiento/5/entregar (Solo Administrador) -> Estado: Entregado
        [HttpPatch("{id}/entregar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Entregar(int id)
        {
            try
            {
                var actualizado = await _mantenimientoService.EntregarAsync(id);
                if (actualizado == null)
                    return NotFound(new { mensaje = "Mantenimiento no encontrado." });

                return Ok(actualizado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PATCH: api/mantenimiento/5/cancelar (Dueño del vehículo o Administrador) -> Estado: Cancelado
        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var cancelado = await _mantenimientoService.CancelarAsync(id, GetUserIdFromToken(), User.IsInRole("Administrador"));
                if (!cancelado)
                    return NotFound(new { mensaje = "Mantenimiento no encontrado." });

                return Ok(new { mensaje = "Mantenimiento cancelado exitosamente." });
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

        private int GetUserIdFromToken()
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(nameIdentifierClaim, out int usuarioId))
                return usuarioId;

            throw new UnauthorizedAccessException("Usuario no válido en el Token.");
        }
    }
}

