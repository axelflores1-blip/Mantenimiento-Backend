using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mantenimiento.Data;
using Mantenimiento.DTOs;
using Mantenimiento.Services;

namespace Mantenimiento.Controllers
{
    [Route("api/usuarios")]
    [ApiController]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Authorize(Roles = RolesConstantes.Administrador)]
        public async Task<IActionResult> GetAll() => Ok(await _usuarioService.GetAll());

        // GET: api/usuarios/{id} (Propio usuario o Administrador)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!EsPropioOAdmin(id)) return Forbid();

            var usuario = await _usuarioService.GetById(id);
            return usuario != null ? Ok(usuario) : NotFound();
        }

        // PUT: api/usuarios/{id} (Propio usuario o Administrador) -- solo Nombre/Email
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateDto dto)
        {
            if (!EsPropioOAdmin(id)) return Forbid();

            try
            {
                return Ok(await _usuarioService.Update(id, dto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PUT: api/usuarios/{id}/rol (Solo Administrador)
        [HttpPut("{id}/rol")]
        [Authorize(Roles = RolesConstantes.Administrador)]
        public async Task<IActionResult> UpdateRol(int id, [FromBody] UsuarioAdminUpdateDto dto)
        {
            try
            {
                return Ok(await _usuarioService.UpdateRol(id, dto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/usuarios (Administrador crea usuarios, p.ej. tecnicos)
        [HttpPost]
        [Authorize(Roles = RolesConstantes.Administrador)]
        public async Task<IActionResult> Create([FromBody] UsuarioCreateDto dto)
        {
            try
            {
                var usuario = await _usuarioService.CreateUser(dto);
                return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/estado")]
        [Authorize(Roles = RolesConstantes.Administrador)]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            int currentUserId;
            try
            {
                currentUserId = GetUserIdFromToken();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }

            try
            {
                return Ok(await _usuarioService.ChangeUserStatus(id, currentUserId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetUserIdFromToken()
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(nameIdentifierClaim, out int usuarioId))
                return usuarioId;

            throw new UnauthorizedAccessException("Usuario no válido en el Token.");
        }

        private bool EsPropioOAdmin(int idSolicitado)
        {
            if (User.IsInRole(RolesConstantes.Administrador)) return true;

            try
            {
                return GetUserIdFromToken() == idSolicitado;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}

