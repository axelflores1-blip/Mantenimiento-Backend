using Microsoft.EntityFrameworkCore;
using Mantenimiento.Data;
using Mantenimiento.Data.Entities;
using Mantenimiento.DTOs;

namespace Mantenimiento.Services
{
    public interface IVehiculoService
    {
        Task<List<VehiculoReadDto>> GetAllAsync();
        Task<List<VehiculoReadDto>> GetMisVehiculosAsync(int usuarioId);
        Task<VehiculoReadDto?> GetByIdAsync(int id, int usuarioId, bool esAdmin);
        Task<VehiculoReadDto> CreateAsync(int usuarioId, VehiculoCreateDto dto);
        Task<VehiculoReadDto?> UpdateAsync(int id, VehiculoUpdateDto dto, int usuarioId, bool esAdmin);
        Task<VehiculoReadDto?> ActualizarKilometrajeAsync(int id, VehiculoKilometrajeDto dto, int usuarioId, bool esAdmin);
        Task<VehiculoReadDto?> ChangeStatusAsync(int id);
    }

    public class VehiculoService : IVehiculoService
    {
        private readonly AppDBContext _context;

        public VehiculoService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<VehiculoReadDto>> GetAllAsync()
        {
            var vehiculos = await _context.Vehiculos.Include(v => v.Usuario).ToListAsync();
            return vehiculos.Select(MapToReadDto).ToList();
        }

        public async Task<List<VehiculoReadDto>> GetMisVehiculosAsync(int usuarioId)
        {
            var vehiculos = await _context.Vehiculos
                .Include(v => v.Usuario)
                .Where(v => v.UsuarioId == usuarioId)
                .ToListAsync();

            return vehiculos.Select(MapToReadDto).ToList();
        }

        public async Task<VehiculoReadDto?> GetByIdAsync(int id, int usuarioId, bool esAdmin)
        {
            var vehiculo = await _context.Vehiculos.Include(v => v.Usuario).FirstOrDefaultAsync(v => v.Id == id);
            if (vehiculo == null) return null;

            if (!esAdmin && vehiculo.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permiso sobre este vehículo.");

            return MapToReadDto(vehiculo);
        }

        public async Task<VehiculoReadDto> CreateAsync(int usuarioId, VehiculoCreateDto dto)
        {
            ValidarVehiculo(dto.Marca, dto.Modelo, dto.Anio, dto.Color);

            if (string.IsNullOrWhiteSpace(dto.Placa))
                throw new InvalidOperationException("La placa es obligatoria.");

            if (dto.Kilometraje < 0)
                throw new InvalidOperationException("El kilometraje no puede ser negativo.");

            string placa = dto.Placa.Trim().ToUpper();
            bool placaEnUso = await _context.Vehiculos.AnyAsync(v => v.Placa == placa);
            if (placaEnUso)
                throw new InvalidOperationException("Ya existe un vehículo registrado con esa placa.");

            string vin = dto.Vin?.Trim().ToUpper() ?? string.Empty;
            if (!string.IsNullOrEmpty(vin))
            {
                bool vinEnUso = await _context.Vehiculos.AnyAsync(v => v.Vin == vin);
                if (vinEnUso)
                    throw new InvalidOperationException("Ya existe un vehículo registrado con ese VIN.");
            }

            var vehiculo = new Vehiculo
            {
                UsuarioId = usuarioId,
                Marca = dto.Marca.Trim(),
                Modelo = dto.Modelo.Trim(),
                Anio = dto.Anio,
                Color = dto.Color.Trim(),
                Placa = placa,
                Vin = vin,
                Kilometraje = dto.Kilometraje,
                Activo = true
            };

            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();
            await _context.Entry(vehiculo).Reference(v => v.Usuario).LoadAsync();

            return MapToReadDto(vehiculo);
        }

        public async Task<VehiculoReadDto?> UpdateAsync(int id, VehiculoUpdateDto dto, int usuarioId, bool esAdmin)
        {
            var vehiculo = await _context.Vehiculos.Include(v => v.Usuario).FirstOrDefaultAsync(v => v.Id == id);
            if (vehiculo == null) return null;

            if (!esAdmin && vehiculo.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permiso sobre este vehículo.");

            ValidarVehiculo(dto.Marca, dto.Modelo, dto.Anio, dto.Color);

            vehiculo.Marca = dto.Marca.Trim();
            vehiculo.Modelo = dto.Modelo.Trim();
            vehiculo.Anio = dto.Anio;
            vehiculo.Color = dto.Color.Trim();

            await _context.SaveChangesAsync();
            return MapToReadDto(vehiculo);
        }

        public async Task<VehiculoReadDto?> ActualizarKilometrajeAsync(int id, VehiculoKilometrajeDto dto, int usuarioId, bool esAdmin)
        {
            var vehiculo = await _context.Vehiculos.Include(v => v.Usuario).FirstOrDefaultAsync(v => v.Id == id);
            if (vehiculo == null) return null;

            if (!esAdmin && vehiculo.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permiso sobre este vehículo.");

            if (dto.Kilometraje < vehiculo.Kilometraje)
                throw new InvalidOperationException($"El kilometraje no puede ser menor al ya registrado ({vehiculo.Kilometraje} km).");

            vehiculo.Kilometraje = dto.Kilometraje;
            await _context.SaveChangesAsync();
            return MapToReadDto(vehiculo);
        }

        public async Task<VehiculoReadDto?> ChangeStatusAsync(int id)
        {
            var vehiculo = await _context.Vehiculos.Include(v => v.Usuario).FirstOrDefaultAsync(v => v.Id == id);
            if (vehiculo == null) return null;

            vehiculo.Activo = !vehiculo.Activo;
            await _context.SaveChangesAsync();
            return MapToReadDto(vehiculo);
        }

        // Validaciones de negocio compartidas entre creacion y edicion
        private static void ValidarVehiculo(string marca, string modelo, int anio, string color)
        {
            if (string.IsNullOrWhiteSpace(marca))
                throw new InvalidOperationException("La marca es obligatoria.");

            if (string.IsNullOrWhiteSpace(modelo))
                throw new InvalidOperationException("El modelo es obligatorio.");

            int anioActual = DateTime.UtcNow.Year;
            if (anio < 1950 || anio > anioActual + 1)
                throw new InvalidOperationException($"El año debe estar entre 1950 y {anioActual + 1}.");

            if (string.IsNullOrWhiteSpace(color))
                throw new InvalidOperationException("El color es obligatorio.");
        }

        private static VehiculoReadDto MapToReadDto(Vehiculo v) => new()
        {
            Id = v.Id,
            UsuarioId = v.UsuarioId,
            NombrePropietario = v.Usuario?.Nombre,
            Marca = v.Marca,
            Modelo = v.Modelo,
            Anio = v.Anio,
            Color = v.Color,
            Placa = v.Placa,
            Vin = v.Vin,
            Kilometraje = v.Kilometraje,
            Activo = v.Activo
        };
    }
}
