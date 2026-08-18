namespace Mantenimiento.Data;

// Flujo: Pendiente -> Recibido -> EnDiagnostico -> EnReparacion -> Finalizado -> Entregado
// Cancelado solo es alcanzable desde Pendiente o Recibido.
public static class EstadoMantenimientoConstantes
{
    public const string Pendiente = "PENDIENTE";
    public const string Recibido = "RECIBIDO";
    public const string EnDiagnostico = "EN_DIAGNOSTICO";
    public const string EnReparacion = "EN_REPARACION";
    public const string Finalizado = "FINALIZADO";
    public const string Entregado = "ENTREGADO";
    public const string Cancelado = "CANCELADO";
}
