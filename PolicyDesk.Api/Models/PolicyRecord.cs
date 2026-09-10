using System.ComponentModel.DataAnnotations;

namespace PolicyDesk.Api.Models;

public class PolicyRecord
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string CustomerIdentifier { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string PolicyType { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Insurer { get; set; } = string.Empty;

    [Required]
    public PolicyStatus CurrentStatus { get; set; } = PolicyStatus.Draft;

    [Range(0.01, double.MaxValue)]
    public decimal Premium { get; set; }

    [Required]
    public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.Monthly;

    [Range(0.01, double.MaxValue)]
    public decimal CoverageAmount { get; set; }

    [Range(0.00, double.MaxValue)]
    public decimal Deductible { get; set; }

    [Required]
    public DateTime EffectiveDate { get; set; }

    [Required]
    public DateTime ExpirationDate { get; set; }

    [MaxLength(120)]
    public string? AssignedAgent { get; set; }

    [MaxLength(80)]
    public string? ClaimReference { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
