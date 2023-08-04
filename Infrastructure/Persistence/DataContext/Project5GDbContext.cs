using Application.Models.ContrAgents;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.DataContext;

public class Project5GDbContext : DbContext
{
    public DbSet<Antenna> Antennae { get; set; }
    public DbSet<ContrAgent> ContrAgents { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectAntenna> ProjectsAntennae { get; set; }
    public DbSet<ProjectStatus> ProjectsStatuses { get; set; }
    public DbSet<Town> Towns { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public Project5GDbContext(DbContextOptions<Project5GDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Login).IsUnique();
        modelBuilder.Entity<ContrAgent>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<User>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<District>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<Town>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<Antenna>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<EnergyResult>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<Location>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<Project>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<ProjectAntenna>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<ProjectStatus>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<TranslatorSpecs>().HasQueryFilter(x => x.IsDelete == false);
        modelBuilder.Entity<User>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<ContrAgent>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<District>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Town>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Project>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Antenna>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<EnergyResult>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Location>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<ProjectAntenna>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<ProjectStatus>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<TranslatorSpecs>()
            .Property(b => b.Created)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        base.OnModelCreating(modelBuilder);
    }
}