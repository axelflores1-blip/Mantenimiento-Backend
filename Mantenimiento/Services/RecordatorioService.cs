using Microsoft.EntityFrameworkCore;
using Mantenimiento.Data;
using Mantenimiento.Data.Entities;
using Mantenimiento.DTOs;

namespace Mantenimiento.Services
{
    public interface IRecordatorioService
    {
        Task<List<RecordatorioReadDto>> GetByVehiculoIdAsync(int vehiculoId, int usuarioId, bool esAdmin);
        Task<RecordatorioReadDto> CreateAsync(RecordatorioCreateDto dto);
        Task<bool> DeleteAsync(int id);
    }

    // Los recordatorios (proxima revision, proximo cambio de aceite, etc.) los
    // genera personal del taller (Administrador/Tecnico), no el cliente -- ver
    // controller. El cliente solo puede CONSULTAR los de sus propios vehiculos.
    public class RecordatorioService : IRecordatorioService
    {
        private readonly AppDBContext _context;

        public RecordatorioService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<RecordatorioReadDto>> GetByVehiculoIdAsync(int vehiculoId, int usuarioId, bool esAdmin)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(vehiculoId)
                ?? throw new KeyNotFoundException("El vehículo especificado no existe.");

            if (!esAdmin && vehiculo.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permiso sobre este vehículo.");

            var recordatorios = await _context.Recordatorios
                .Where(r => r.VehiculoId == vehiculoId)
                .OrderBy(r => r.Fecha)
                .ToListAsync();

            return recordatorios.Select(MapToReadDto).ToList();
        }

        public async Task<RecordatorioReadDto> CreateAsync(RecordatorioCreateDto dto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId)
                ?? throw new KeyNotFoundException("El vehículo especificado no existe.");

            if (string.IsNullOrWhiteSpace(dto.Descripcion))
                throw new InvalidOperationException("La descripción del recordatorio es obligatoria.");

            if (dto.Kilometraje < vehiculo.Kilometraje)
                throw new InvalidOperationException("El kilometraje del recordatorio no puede ser menor al kilometraje actual del vehículo.");

            var recordatorio = new Recordatorio
            {
                VehiculoId = dto.VehiculoId,
                Fecha = dto.Fecha,
                Kilometraje = dto.Kilometraje,
                Descripcion = dto.Descripcion.Trim()
            };

            _context.Recordatorios.Add(recordatorio);
            await _context.SaveChangesAsync();

            return MapToReadDto(recordatorio);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var recordatorio = await _context.Recordatorios.FindAsync(id);
            if (recordatorio == null) return false;

            _context.Recordatorios.Remove(recordatorio);
            await _context.SaveChangesAsync();
            return true;
        }

        private static RecordatorioReadDto MapToReadDto(Recordatorio r) => new()
        {
            Id = r.Id,
            VehiculoId = r.VehiculoId,
            Fecha = r.Fecha,
            Kilometraje = r.Kilometraje,
            Descripcion = r.Descripcion
        };
    }
}
