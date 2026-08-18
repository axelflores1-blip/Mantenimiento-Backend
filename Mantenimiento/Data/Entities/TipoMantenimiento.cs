namespace Mantenimiento.Data.Entities;

public class TipoMantenimiento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;

    public ICollection<Mantenimientos> Mantenimientos { get; set; } = new List<Mantenimientos>();
}
