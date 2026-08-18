namespace Mantenimiento.Data.Entities;

public class Mantenimientos
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public int TipoMantenimientoId { get; set; }
    public int? TecnicoId { get; set; }
    public DateOnly Fecha { get; set; }
    public int Kilometraje { get; set; }
    public string? Diagnostico { get; set; }
    public string? TrabajoRealizado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Vehiculo Vehiculo { get; set; } = null!;
    public TipoMantenimiento TipoMantenimiento { get; set; } = null!;
    public Usuario? Tecnico { get; set; }
}
