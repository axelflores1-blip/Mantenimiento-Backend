using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mantenimiento.DTOs;
using Mantenimiento.Services;

namespace Mantenimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación JWT para todos los endpoints
    public class TipoMantenimientoController : ControllerBase
    {
        private readonly ITipoMantenimientoService _tipoMantenimientoService;

        public TipoMantenimientoController(ITipoMantenimientoService tipoMantenimientoService)
        {
            _tipoMantenimientoService = tipoMantenimientoService;
        }

        // GET: api/tipomantenimiento (cualquier autenticado)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tipos = await _tipoMantenimientoService.GetAllAsync();
            return Ok(tipos);
        }

        // GET: api/tipomantenimiento/5 (cualquier autenticado)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tipo = await _tipoMantenimientoService.GetByIdAsync(id);
            if (tipo == null)
                return NotFound(new { mensaje = "Tipo de mantenimiento no encontrado." });

            return Ok(tipo);
        }

        // POST: api/tipomantenimiento (Solo Administrador)
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([FromBody] TipoMantenimientoCreateDto dto)
        {
            try
            {
                var nuevoTipo = await _tipoMantenimientoService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = nuevoTipo.Id }, nuevoTipo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PUT: api/tipomantenimiento/5 (Solo Administrador)
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] TipoMantenimientoUpdateDto dto)
        {
            try
            {
                var tipoActualizado = await _tipoMantenimientoService.UpdateAsync(id, dto);
                if (tipoActualizado == null)
                    return NotFound(new { mensaje = "Tipo de mantenimiento no encontrado." });

                return Ok(tipoActualizado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PATCH: api/tipomantenimiento/5/status (Solo Administrador)
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var tipoActualizado = await _tipoMantenimientoService.ChangeStatusAsync(id);
            if (tipoActualizado == null)
                return NotFound(new { mensaje = "Tipo de mantenimiento no encontrado." });

            return Ok(tipoActualizado);
        }
    }
}
