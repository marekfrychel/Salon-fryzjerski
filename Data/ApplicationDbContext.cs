using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalonFryzjerski.Models;

namespace SalonFryzjerski.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Service> Services => Set<Service>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Availability> Availabilities { get; set; } = default!;
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<UserModuleAccess> UserModuleAccesses => Set<UserModuleAccess>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Employee>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<UserModuleAccess>()
            .HasIndex(x => new { x.UserId, x.Module })
            .IsUnique();

        builder.Entity<UserModuleAccess>()
            .Property(x => x.Module)
            .HasConversion<int>();

        builder.Entity<UserModuleAccess>()
            .Property(x => x.Access)
            .HasConversion<int>();
    }
}
