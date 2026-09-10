using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Api.Data;
using PolicyDesk.Api.Models;
using PolicyDesk.Api.Models.Dtos;
using PolicyDesk.Api.Services;

namespace PolicyDesk.Tests;

public class PolicyServiceTests
{
    [Fact]
    public async Task ApplyLifecycleActionAsync_Quote_MovesDraftToQuoted()
    {
        await using var db = CreateDbContext();
        var service = new PolicyService(db);

        var policy = new PolicyRecord
        {
            Id = Guid.NewGuid(),
            PolicyNumber = "POL-1001",
            CustomerName = "Alice Smith",
            CustomerIdentifier = "CUST-001",
            PolicyType = "Auto",
            Insurer = "Acme Insurance",
            CurrentStatus = PolicyStatus.Draft,
            Premium = 120.50m,
            BillingFrequency = BillingFrequency.Monthly,
            CoverageAmount = 5000m,
            Deductible = 250m,
            EffectiveDate = new DateTime(2026, 01, 01),
            ExpirationDate = new DateTime(2026, 12, 31),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        db.Policies.Add(policy);
        await db.SaveChangesAsync();

        var result = await service.ApplyLifecycleActionAsync(policy.Id, "Quote");

        result.CurrentStatus.Should().Be("Quoted");
    }

    [Fact]
    public async Task ApplyLifecycleActionAsync_ViewClaim_KeepsStatusInClaimInReview()
    {
        await using var db = CreateDbContext();
        var service = new PolicyService(db);

        var policy = new PolicyRecord
        {
            Id = Guid.NewGuid(),
            PolicyNumber = "POL-2002",
            CustomerName = "Bob Jones",
            CustomerIdentifier = "CUST-002",
            PolicyType = "Home",
            Insurer = "Northwind",
            CurrentStatus = PolicyStatus.ClaimInReview,
            Premium = 90m,
            BillingFrequency = BillingFrequency.Monthly,
            CoverageAmount = 3000m,
            Deductible = 500m,
            EffectiveDate = new DateTime(2026, 02, 01),
            ExpirationDate = new DateTime(2026, 11, 30),
            ClaimReference = "CLAIM-123",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        db.Policies.Add(policy);
        await db.SaveChangesAsync();

        var result = await service.ApplyLifecycleActionAsync(policy.Id, "View claim");

        result.CurrentStatus.Should().Be("ClaimInReview");
        result.ClaimReference.Should().Be("CLAIM-123");
    }

    private static PolicyDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PolicyDbContext(options);
    }
}
