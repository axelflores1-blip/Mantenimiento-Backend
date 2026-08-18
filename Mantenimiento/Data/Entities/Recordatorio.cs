namespace Mantenimiento.Data.Entities;

public class Recordatorio
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public DateOnly Fecha { get; set; }
    public int Kilometraje { get; set; }
    public string Descripcion { get; set; } = string.Empty;

    public Vehiculo Vehiculo { get; set; } = null!;
}
