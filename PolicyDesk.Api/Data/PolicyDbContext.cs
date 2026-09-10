using Microsoft.EntityFrameworkCore;
using PolicyDesk.Api.Models;

namespace PolicyDesk.Api.Data;

public class PolicyDbContext : DbContext
{
    public PolicyDbContext(DbContextOptions<PolicyDbContext> options) : base(options)
    {
    }

    public DbSet<PolicyRecord> Policies => Set<PolicyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PolicyRecord>(entity =>
        {
            entity.ToTable("Policies");
            entity.HasIndex(p => p.PolicyNumber).IsUnique();
            entity.HasIndex(p => p.CustomerIdentifier);
            entity.HasIndex(p => p.Insurer);
            entity.HasIndex(p => p.CurrentStatus);
            entity.HasIndex(p => p.PolicyType);
            entity.HasIndex(p => p.EffectiveDate);
            entity.HasIndex(p => p.ExpirationDate);

            entity.Property(p => p.CurrentStatus)
                .HasConversion<string>();

            entity.Property(p => p.BillingFrequency)
                .HasConversion<string>();

            entity.Property(p => p.Premium)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.CoverageAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.Deductible)
                .HasColumnType("decimal(18,2)");
        });
    }
}
