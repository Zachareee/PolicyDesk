namespace PolicyDesk.Api.Models.Dtos;

public class PolicyDetailDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerIdentifier { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public string Insurer { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public decimal Premium { get; set; }
    public string BillingFrequency { get; set; } = string.Empty;
    public decimal CoverageAmount { get; set; }
    public decimal Deductible { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string? AssignedAgent { get; set; }
    public string? ClaimReference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string[] AvailableActions { get; set; } = Array.Empty<string>();
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
