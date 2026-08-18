namespace Mantenimiento.DTOs;

public class RecordatorioCreateDto
{
    public int VehiculoId { get; set; }
    public DateOnly Fecha { get; set; }
    public int Kilometraje { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

public class RecordatorioReadDto
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public DateOnly Fecha { get; set; }
    public int Kilometraje { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
