using Microsoft.EntityFrameworkCore;
using Mantenimiento.Data;
using Mantenimiento.Data.Entities;
using Mantenimiento.DTOs;

namespace Mantenimiento.Services
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDto>> GetAll();
        Task<UsuarioDto?> GetById(int id);
        Task<UsuarioDto> Update(int id, UsuarioUpdateDto dto);
        Task<UsuarioDto> UpdateRol(int id, UsuarioAdminUpdateDto dto);
        Task<UsuarioDto> CreateUser(UsuarioCreateDto dto);
        Task<UsuarioDto> ChangeUserStatus(int id, int currentUserId);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly AppDBContext _context;

        public UsuarioService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<UsuarioDto>> GetAll()
        {
            var usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync();
            return usuarios.Select(MapToDto).ToList();
        }

        public async Task<UsuarioDto?> GetById(int id)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            return usuario == null ? null : MapToDto(usuario);
        }

        // Autoedicion de perfil: SOLO Nombre y Email.
        public async Task<UsuarioDto> Update(int id, UsuarioUpdateDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new KeyNotFoundException("Usuario no encontrado");

            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email;
            await _context.SaveChangesAsync();

            return MapToDto(usuario);
        }

        // Cambio de rol: operacion administrativa separada, valida que el RolId exista.
        public async Task<UsuarioDto> UpdateRol(int id, UsuarioAdminUpdateDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new KeyNotFoundException("Usuario no encontrado");

            bool rolExiste = await _context.Roles.AnyAsync(r => r.Id == dto.RolId);
            if (!rolExiste)
                throw new InvalidOperationException("El rol especificado no existe.");

            usuario.RolId = dto.RolId;
            await _context.SaveChangesAsync();
            await _context.Entry(usuario).Reference(u => u.Rol).LoadAsync();

            return MapToDto(usuario);
        }

        public async Task<UsuarioDto> CreateUser(UsuarioCreateDto dto)
        {
            bool rolExiste = await _context.Roles.AnyAsync(r => r.Id == dto.RolId);
            if (!rolExiste)
                throw new InvalidOperationException("El rol especificado no existe.");

            bool emailEnUso = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email);
            if (emailEnUso)
                throw new InvalidOperationException("Ya existe un usuario con ese email.");

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RolId = dto.RolId,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            await _context.Entry(usuario).Reference(u => u.Rol).LoadAsync();

            return MapToDto(usuario);
        }

        public async Task<UsuarioDto> ChangeUserStatus(int id, int currentUserId)
        {
            if (id == currentUserId)
                throw new InvalidOperationException("No puedes desactivar tu propia cuenta.");

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new KeyNotFoundException("Usuario no encontrado");

            usuario.Activo = !usuario.Activo;
            await _context.SaveChangesAsync();

            return MapToDto(usuario);
        }

        private static UsuarioDto MapToDto(Usuario u) => new()
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Email = u.Email,
            RolId = u.RolId,
            RolNombre = u.Rol?.Nombre ?? string.Empty,
            Activo = u.Activo
        };
    }
}
