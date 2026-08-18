namespace Mantenimiento.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

// Autoedicion de perfil (Nombre/Email). A proposito NO incluye RolId.
public class UsuarioUpdateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Cambio de rol, exclusivo de Administrador.
public class UsuarioAdminUpdateDto
{
    public int RolId { get; set; }
}

// Administrador crea usuarios (p.ej. tecnicos u otros administradores).
public class UsuarioCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RolId { get; set; }
}
