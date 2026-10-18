using Encurtador.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Encurtador.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Link> Links => Set<Link>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Link>(entity =>
        {
            entity.ToTable("Links");
            entity.HasKey(link => link.Id);
            entity.Property(link => link.Code)
                .HasMaxLength(20)
                .UseCollation("BINARY")
                .IsRequired();
            entity.Property(link => link.TargetUrl).HasMaxLength(2048).IsRequired();
            entity.Property(link => link.CreatedAtUtc).IsRequired();
            entity.Property(link => link.AccessCount).IsRequired().HasDefaultValue(0L);
            entity.HasIndex(link => link.Code).IsUnique();
        });
    }
}
