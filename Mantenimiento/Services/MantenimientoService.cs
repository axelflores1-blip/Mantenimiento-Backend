using Microsoft.EntityFrameworkCore;
using Mantenimiento.Data;
using Mantenimiento.Data.Entities;
using Mantenimiento.DTOs;

namespace Mantenimiento.Services
{
    public interface IMantenimientoService
    {
        Task<List<MantenimientoReadDto>> GetAllAsync();
        Task<List<MantenimientoReadDto>> GetByVehiculoIdAsync(int vehiculoId, int usuarioId, bool esAdmin);
        Task<List<MantenimientoReadDto>> GetAsignadosATecnicoAsync(int tecnicoId);
        Task<MantenimientoReadDto?> GetByIdAsync(int id, int usuarioId, bool esAdmin, bool esTecnico);
        Task<MantenimientoReadDto> SolicitarAsync(int usuarioId, MantenimientoSolicitudDto dto);
        Task<MantenimientoReadDto?> AsignarTecnicoAsync(int id, MantenimientoAsignarTecnicoDto dto);
        Task<MantenimientoReadDto?> RegistrarDiagnosticoAsync(int id, int tecnicoId, MantenimientoDiagnosticoDto dto);
        Task<MantenimientoReadDto?> RegistrarTrabajoAsync(int id, int tecnicoId, MantenimientoTrabajoDto dto);
        Task<MantenimientoReadDto?> FinalizarAsync(int id, int tecnicoId);
        Task<MantenimientoReadDto?> EntregarAsync(int id);
        Task<bool> CancelarAsync(int id, int usuarioId, bool esAdmin);
    }

    public class MantenimientoService : IMantenimientoService
    {
        private readonly AppDBContext _context;

        public MantenimientoService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<MantenimientoReadDto>> GetAllAsync()
        {
            var mantenimientos = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .Include(m => m.TipoMantenimiento)
                .Include(m => m.Tecnico)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            return mantenimientos.Select(MapToReadDto).ToList();
        }

        // GET: mantenimientos de un vehiculo puntual (Dueno del vehiculo o Administrador)
        public async Task<List<MantenimientoReadDto>> GetByVehiculoIdAsync(int vehiculoId, int usuarioId, bool esAdmin)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(vehiculoId)
                ?? throw new KeyNotFoundException("El vehículo especificado no existe.");

            if (!esAdmin && vehiculo.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permiso sobre este vehículo.");

            var mantenimientos = await _context.Mantenimientos
                .Include(m => m.TipoMantenimiento)
                .Include(m => m.Tecnico)
                .Where(m => m.VehiculoId == vehiculoId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            return mantenimientos.Select(MapToReadDto).ToList();
        }

        // GET: mantenimientos activos asignados a un tecnico (excluye Entregado/Cancelado)
        public async Task<List<MantenimientoReadDto>> GetAsignadosATecnicoAsync(int tecnicoId)
        {
            var mantenimientos = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .Include(m => m.TipoMantenimiento)
                .Where(m => m.TecnicoId == tecnicoId
                    && m.Estado != EstadoMantenimientoConstantes.Entregado
                    && m.Estado != EstadoMantenimientoConstantes.Cancelado)
                .OrderBy(m => m.Fecha)
                .ToListAsync();

            return mantenimientos.Select(MapToReadDto).ToList();
        }

        // GET: api/mantenimiento/{id} (Dueno del vehiculo, tecnico asignado, o Administrador)
        public async Task<MantenimientoReadDto?> GetByIdAsync(int id, int usuarioId, bool esAdmin, bool esTecnico)
        {
            var mantenimiento = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .Include(m => m.TipoMantenimiento)
                .Include(m => m.Tecnico)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mantenimiento == null) return null;

            bool esDuenio = mantenimiento.Vehiculo.UsuarioId == usuarioId;
            bool esTecnicoAsignado = esTecnico && mantenimiento.TecnicoId == usuarioId;

            if (!esAdmin && !esDuenio && !esTecnicoAsignado)
                throw new UnauthorizedAccessException("No tienes permiso sobre este mantenimiento.");

            return MapToReadDto(mantenimiento);
        }

        // POST: el cliente solicita mantenimiento para uno de sus propios vehiculos
        public async Task<MantenimientoReadDto> SolicitarAsync(int usuarioId, MantenimientoSolicitudDto dto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId)
                ?? throw new KeyNotFoundException("El vehículo especificado no existe.");

            if (vehiculo.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("Solo puedes solicitar mantenimiento para tus propios vehículos.");

            var tipo = await _context.TiposMantenimiento.FindAsync(dto.TipoMantenimientoId)
                ?? throw new KeyNotFoundException("El tipo de mantenimiento especificado no existe.");

            if (!tipo.Estado)
                throw new InvalidOperationException("Este tipo de mantenimiento no está disponible actualmente.");

            if (dto.Kilometraje < vehiculo.Kilometraje)
                throw new InvalidOperationException($"El kilometraje no puede ser menor al último registrado ({vehiculo.Kilometraje} km).");

            var mantenimiento = new Mantenimientos
            {
                VehiculoId = dto.VehiculoId,
                TipoMantenimientoId = dto.TipoMantenimientoId,
                Fecha = DateOnly.FromDateTime(DateTime.UtcNow),
                Kilometraje = dto.Kilometraje,
                Estado = EstadoMantenimientoConstantes.Pendiente
            };

            _context.Mantenimientos.Add(mantenimiento);
            await _context.SaveChangesAsync();

            return (await GetByIdInternoAsync(mantenimiento.Id))!;
        }

        // PATCH: Administrador asigna un tecnico a un mantenimiento pendiente
        public async Task<MantenimientoReadDto?> AsignarTecnicoAsync(int id, MantenimientoAsignarTecnicoDto dto)
        {
            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento == null) return null;

            if (mantenimiento.Estado != EstadoMantenimientoConstantes.Pendiente)
                throw new InvalidOperationException("Solo se puede asignar un técnico a un mantenimiento pendiente.");

            var tecnico = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == dto.TecnicoId)
                ?? throw new KeyNotFoundException("El técnico especificado no existe.");

            if (tecnico.Rol?.Nombre != RolesConstantes.Tecnico)
                throw new InvalidOperationException("El usuario especificado no tiene el rol de Técnico.");

            if (!tecnico.Activo)
                throw new InvalidOperationException("El técnico especificado no está activo.");

            mantenimiento.TecnicoId = dto.TecnicoId;
            mantenimiento.Estado = EstadoMantenimientoConstantes.Recibido;
            await _context.SaveChangesAsync();

            return await GetByIdInternoAsync(id);
        }

        // PATCH: el tecnico ASIGNADO registra el diagnostico
        public async Task<MantenimientoReadDto?> RegistrarDiagnosticoAsync(int id, int tecnicoId, MantenimientoDiagnosticoDto dto)
        {
            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento == null) return null;

            if (mantenimiento.TecnicoId != tecnicoId)
                throw new UnauthorizedAccessException("No eres el técnico asignado a este mantenimiento.");

            if (mantenimiento.Estado != EstadoMantenimientoConstantes.Recibido
                && mantenimiento.Estado != EstadoMantenimientoConstantes.EnDiagnostico)
                throw new InvalidOperationException("Solo se puede registrar diagnóstico en un mantenimiento recibido o en diagnóstico.");

            if (string.IsNullOrWhiteSpace(dto.Diagnostico))
                throw new InvalidOperationException("El diagnóstico no puede estar vacío.");

            mantenimiento.Diagnostico = dto.Diagnostico.Trim();
            mantenimiento.Estado = EstadoMantenimientoConstantes.EnDiagnostico;
            await _context.SaveChangesAsync();

            return await GetByIdInternoAsync(id);
        }

        // PATCH: el tecnico ASIGNADO registra el trabajo realizado
        public async Task<MantenimientoReadDto?> RegistrarTrabajoAsync(int id, int tecnicoId, MantenimientoTrabajoDto dto)
        {
            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento == null) return null;

            if (mantenimiento.TecnicoId != tecnicoId)
                throw new UnauthorizedAccessException("No eres el técnico asignado a este mantenimiento.");

            if (mantenimiento.Estado != EstadoMantenimientoConstantes.EnDiagnostico
                && mantenimiento.Estado != EstadoMantenimientoConstantes.EnReparacion)
                throw new InvalidOperationException("Debes registrar el diagnóstico antes de registrar el trabajo realizado.");

            if (string.IsNullOrWhiteSpace(dto.TrabajoRealizado))
                throw new InvalidOperationException("El trabajo realizado no puede estar vacío.");

            mantenimiento.TrabajoRealizado = dto.TrabajoRealizado.Trim();
            mantenimiento.Estado = EstadoMantenimientoConstantes.EnReparacion;
            await _context.SaveChangesAsync();

            return await GetByIdInternoAsync(id);
        }

        // PATCH: el tecnico ASIGNADO finaliza el mantenimiento (actualiza el kilometraje del vehiculo)
        public async Task<MantenimientoReadDto?> FinalizarAsync(int id, int tecnicoId)
        {
            var mantenimiento = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mantenimiento == null) return null;

            if (mantenimiento.TecnicoId != tecnicoId)
                throw new UnauthorizedAccessException("No eres el técnico asignado a este mantenimiento.");

            if (mantenimiento.Estado != EstadoMantenimientoConstantes.EnReparacion)
                throw new InvalidOperationException("Solo se puede finalizar un mantenimiento que está en reparación.");

            if (string.IsNullOrWhiteSpace(mantenimiento.TrabajoRealizado))
                throw new InvalidOperationException("Debes registrar el trabajo realizado antes de finalizar.");

            mantenimiento.Estado = EstadoMantenimientoConstantes.Finalizado;

            if (mantenimiento.Kilometraje > mantenimiento.Vehiculo.Kilometraje)
                mantenimiento.Vehiculo.Kilometraje = mantenimiento.Kilometraje;

            await _context.SaveChangesAsync();
            return await GetByIdInternoAsync(id);
        }

        // PATCH: Administrador entrega el vehiculo al cliente
        public async Task<MantenimientoReadDto?> EntregarAsync(int id)
        {
            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento == null) return null;

            if (mantenimiento.Estado != EstadoMantenimientoConstantes.Finalizado)
                throw new InvalidOperationException("Solo se puede entregar un mantenimiento finalizado.");

            mantenimiento.Estado = EstadoMantenimientoConstantes.Entregado;
            await _context.SaveChangesAsync();
            return await GetByIdInternoAsync(id);
        }

        // PATCH: Dueno del vehiculo o Administrador cancela (solo antes de iniciar diagnostico)
        public async Task<bool> CancelarAsync(int id, int usuarioId, bool esAdmin)
        {
            var mantenimiento = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mantenimiento == null) return false;

            if (!esAdmin && mantenimiento.Vehiculo.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permiso para cancelar este mantenimiento.");

            if (mantenimiento.Estado != EstadoMantenimientoConstantes.Pendiente
                && mantenimiento.Estado != EstadoMantenimientoConstantes.Recibido)
                throw new InvalidOperationException("Solo se puede cancelar un mantenimiento que aún no ha iniciado diagnóstico.");

            mantenimiento.Estado = EstadoMantenimientoConstantes.Cancelado;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<MantenimientoReadDto?> GetByIdInternoAsync(int id)
        {
            var mantenimiento = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .Include(m => m.TipoMantenimiento)
                .Include(m => m.Tecnico)
                .FirstOrDefaultAsync(m => m.Id == id);

            return mantenimiento == null ? null : MapToReadDto(mantenimiento);
        }

        private static MantenimientoReadDto MapToReadDto(Mantenimientos m) => new()
        {
            Id = m.Id,
            VehiculoId = m.VehiculoId,
            PlacaVehiculo = m.Vehiculo?.Placa,
            TipoMantenimientoId = m.TipoMantenimientoId,
            NombreTipoMantenimiento = m.TipoMantenimiento?.Nombre,
            TecnicoId = m.TecnicoId,
            NombreTecnico = m.Tecnico?.Nombre,
            Fecha = m.Fecha,
            Kilometraje = m.Kilometraje,
            Diagnostico = m.Diagnostico,
            TrabajoRealizado = m.TrabajoRealizado,
            Estado = m.Estado
        };
    }
}
