namespace Mantenimiento.DTOs;

// Cliente solicita mantenimiento para uno de sus vehiculos.
public class MantenimientoSolicitudDto
{
    public int VehiculoId { get; set; }
    public int TipoMantenimientoId { get; set; }
    public int Kilometraje { get; set; }
}

// Administrador asigna un tecnico a un mantenimiento pendiente.
public class MantenimientoAsignarTecnicoDto
{
    public int TecnicoId { get; set; }
}

// Tecnico asignado registra el diagnostico.
public class MantenimientoDiagnosticoDto
{
    public string Diagnostico { get; set; } = string.Empty;
}

// Tecnico asignado registra el trabajo realizado.
public class MantenimientoTrabajoDto
{
    public string TrabajoRealizado { get; set; } = string.Empty;
}

public class MantenimientoReadDto
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public string? PlacaVehiculo { get; set; }
    public int TipoMantenimientoId { get; set; }
    public string? NombreTipoMantenimiento { get; set; }
    public int? TecnicoId { get; set; }
    public string? NombreTecnico { get; set; }
    public DateOnly Fecha { get; set; }
    public int Kilometraje { get; set; }
    public string? Diagnostico { get; set; }
    public string? TrabajoRealizado { get; set; }
    public string Estado { get; set; } = string.Empty;
}
