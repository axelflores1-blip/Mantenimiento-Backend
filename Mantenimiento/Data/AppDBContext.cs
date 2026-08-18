using Microsoft.EntityFrameworkCore;
using Mantenimiento.Data.Entities;
using Mantenimiento.Data;

namespace Mantenimiento.Data;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<TipoMantenimiento> TiposMantenimiento { get; set; }
    public DbSet<Mantenimientos> Mantenimientos { get; set; }
    public DbSet<Recordatorio> Recordatorios { get; set; }
    public DbSet<CodigoRecuperacion> CodigosRecuperacion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rol>().ToTable("Rol");
        modelBuilder.Entity<Usuario>().ToTable("Usuario");
        modelBuilder.Entity<Usuario>().Property(u => u.Activo).HasDefaultValue(true);
        modelBuilder.Entity<Vehiculo>().ToTable("Vehiculo");
        modelBuilder.Entity<Vehiculo>().Property(v => v.Activo).HasDefaultValue(true);
        modelBuilder.Entity<TipoMantenimiento>().ToTable("TipoMantenimiento");
        modelBuilder.Entity<Mantenimientos>().ToTable("Mantenimiento");
        modelBuilder.Entity<Recordatorio>().ToTable("Recordatorio");

        // Un Mantenimiento se relaciona opcionalmente con un Tecnico (Usuario).
        // Restrict para no borrar en cascada al usuario si se elimina un mantenimiento.
        modelBuilder.Entity<Mantenimientos>()
            .HasOne(m => m.Tecnico)
            .WithMany()
            .HasForeignKey(m => m.TecnicoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = RolesConstantes.Cliente },
            new Rol { Id = 2, Nombre = RolesConstantes.Administrador },
            new Rol { Id = 3, Nombre = RolesConstantes.Tecnico }
        );

        modelBuilder.Entity<CodigoRecuperacion>().ToTable("CodigoRecuperacion");

        modelBuilder.Entity<CodigoRecuperacion>()
            .HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
