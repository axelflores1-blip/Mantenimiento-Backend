using Microsoft.EntityFrameworkCore;
using Mantenimiento.Data;
using Mantenimiento.Data.Entities;
using Mantenimiento.DTOs;

namespace Mantenimiento.Services
{
    public interface ITipoMantenimientoService
    {
        Task<List<TipoMantenimientoReadDto>> GetAllAsync();
        Task<TipoMantenimientoReadDto?> GetByIdAsync(int id);
        Task<TipoMantenimientoReadDto> CreateAsync(TipoMantenimientoCreateDto dto);
        Task<TipoMantenimientoReadDto?> UpdateAsync(int id, TipoMantenimientoUpdateDto dto);
        Task<TipoMantenimientoReadDto?> ChangeStatusAsync(int id);
    }

    public class TipoMantenimientoService : ITipoMantenimientoService
    {
        private readonly AppDBContext _context;

        public TipoMantenimientoService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<TipoMantenimientoReadDto>> GetAllAsync()
        {
            var tipos = await _context.TiposMantenimiento.ToListAsync();
            return tipos.Select(MapToReadDto).ToList();
        }

        public async Task<TipoMantenimientoReadDto?> GetByIdAsync(int id)
        {
            var tipo = await _context.TiposMantenimiento.FindAsync(id);
            return tipo == null ? null : MapToReadDto(tipo);
        }

        public async Task<TipoMantenimientoReadDto> CreateAsync(TipoMantenimientoCreateDto dto)
        {
            ValidarTipoMantenimiento(dto.Nombre, dto.Descripcion);

            var nuevoTipo = new TipoMantenimiento
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
                Estado = true
            };

            _context.TiposMantenimiento.Add(nuevoTipo);
            await _context.SaveChangesAsync();
            return MapToReadDto(nuevoTipo);
        }

        public async Task<TipoMantenimientoReadDto?> UpdateAsync(int id, TipoMantenimientoUpdateDto dto)
        {
            var tipo = await _context.TiposMantenimiento.FindAsync(id);
            if (tipo == null) return null;

            ValidarTipoMantenimiento(dto.Nombre, dto.Descripcion);

            tipo.Nombre = dto.Nombre.Trim();
            tipo.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
            tipo.Estado = dto.Estado;

            await _context.SaveChangesAsync();
            return MapToReadDto(tipo);
        }

        public async Task<TipoMantenimientoReadDto?> ChangeStatusAsync(int id)
        {
            var tipo = await _context.TiposMantenimiento.FindAsync(id);
            if (tipo == null) return null;

            tipo.Estado = !tipo.Estado;
            await _context.SaveChangesAsync();
            return MapToReadDto(tipo);
        }

        private static void ValidarTipoMantenimiento(string nombre, string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new InvalidOperationException("El nombre del tipo de mantenimiento es obligatorio.");

            if (nombre.Trim().Length > 100)
                throw new InvalidOperationException("El nombre no puede superar los 100 caracteres.");

            if (descripcion != null && descripcion.Length > 500)
                throw new InvalidOperationException("La descripción no puede superar los 500 caracteres.");
        }

        private static TipoMantenimientoReadDto MapToReadDto(TipoMantenimiento t) => new()
        {
            Id = t.Id,
            Nombre = t.Nombre,
            Descripcion = t.Descripcion,
            Estado = t.Estado
        };
    }
}
