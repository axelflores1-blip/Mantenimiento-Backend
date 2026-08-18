namespace Mantenimiento.DTOs;

public class VehiculoCreateDto
{
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public int Kilometraje { get; set; }
}

// No incluye Placa/Vin: cambiar esos datos es una operacion mas sensible,
// se maneja aparte si se necesita en el futuro.
public class VehiculoUpdateDto
{
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class VehiculoKilometrajeDto
{
    public int Kilometraje { get; set; }
}

public class VehiculoReadDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? NombrePropietario { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public int Kilometraje { get; set; }
    public bool Activo { get; set; }
}
