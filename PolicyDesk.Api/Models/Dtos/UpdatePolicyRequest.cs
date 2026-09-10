namespace PolicyDesk.Api.Models.Dtos;

public class UpdatePolicyRequest
{
    public string? PolicyNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerIdentifier { get; set; }
    public string? PolicyType { get; set; }
    public string? Insurer { get; set; }
    public decimal? Premium { get; set; }
    public BillingFrequency? BillingFrequency { get; set; }
    public decimal? CoverageAmount { get; set; }
    public decimal? Deductible { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? AssignedAgent { get; set; }
    public string? ClaimReference { get; set; }
    public string? Notes { get; set; }
}
