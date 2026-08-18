namespace Mantenimiento.Data.Entities;

public class Vehiculo
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public int Kilometraje { get; set; }
    public bool Activo { get; set; } = true;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<Mantenimientos> Mantenimientos { get; set; } = new List<Mantenimientos>();
    public ICollection<Recordatorio> Recordatorios { get; set; } = new List<Recordatorio>();
}
