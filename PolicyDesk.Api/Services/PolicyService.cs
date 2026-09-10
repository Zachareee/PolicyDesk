using Microsoft.EntityFrameworkCore;
using PolicyDesk.Api.Data;
using PolicyDesk.Api.Models;
using PolicyDesk.Api.Models.Dtos;

namespace PolicyDesk.Api.Services;

public class PolicyService : IPolicyService
{
    private readonly PolicyDbContext _db;

    public PolicyService(PolicyDbContext db)
    {
        _db = db;
    }

    public async Task<PagedPolicyResult> GetPoliciesAsync(PolicyListRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        pageSize = Math.Min(pageSize, 100);

        IQueryable<PolicyRecord> query = _db.Policies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(p =>
                p.PolicyNumber.Contains(search) ||
                p.CustomerName.Contains(search) ||
                p.CustomerIdentifier.Contains(search) ||
                p.PolicyType.Contains(search) ||
                p.Insurer.Contains(search) ||
                p.AssignedAgent != null && p.AssignedAgent.Contains(search) ||
                p.Notes != null && p.Notes.Contains(search));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.CurrentStatus == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.PolicyType))
        {
            query = query.Where(p => p.PolicyType == request.PolicyType);
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerIdentifier))
        {
            query = query.Where(p => p.CustomerIdentifier == request.CustomerIdentifier);
        }

        if (!string.IsNullOrWhiteSpace(request.Insurer))
        {
            query = query.Where(p => p.Insurer == request.Insurer);
        }

        if (request.MinPremium.HasValue)
        {
            query = query.Where(p => p.Premium >= request.MinPremium.Value);
        }

        if (request.MaxPremium.HasValue)
        {
            query = query.Where(p => p.Premium <= request.MaxPremium.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "policynumber" => request.SortDirection == "asc" ? query.OrderBy(p => p.PolicyNumber) : query.OrderByDescending(p => p.PolicyNumber),
            "customername" => request.SortDirection == "asc" ? query.OrderBy(p => p.CustomerName) : query.OrderByDescending(p => p.CustomerName),
            "insurer" => request.SortDirection == "asc" ? query.OrderBy(p => p.Insurer) : query.OrderByDescending(p => p.Insurer),
            "premium" => request.SortDirection == "asc" ? query.OrderBy(p => p.Premium) : query.OrderByDescending(p => p.Premium),
            "effectivedate" => request.SortDirection == "asc" ? query.OrderBy(p => p.EffectiveDate) : query.OrderByDescending(p => p.EffectiveDate),
            "expirationdate" => request.SortDirection == "asc" ? query.OrderBy(p => p.ExpirationDate) : query.OrderByDescending(p => p.ExpirationDate),
            _ => request.SortDirection == "asc" ? query.OrderBy(p => p.UpdatedAtUtc) : query.OrderByDescending(p => p.UpdatedAtUtc)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PolicyListItemDto
            {
                Id = p.Id,
                PolicyNumber = p.PolicyNumber,
                CustomerName = p.CustomerName,
                CustomerIdentifier = p.CustomerIdentifier,
                PolicyType = p.PolicyType,
                Insurer = p.Insurer,
                CurrentStatus = p.CurrentStatus.ToString(),
                Premium = p.Premium,
                BillingFrequency = p.BillingFrequency.ToString(),
                CoverageAmount = p.CoverageAmount,
                EffectiveDate = p.EffectiveDate,
                ExpirationDate = p.ExpirationDate,
                AvailableActions = GetAvailableActions(p.CurrentStatus),
                CanEdit = CanEdit(p.CurrentStatus),
                CanDelete = CanDelete(p.CurrentStatus),
                UpdatedAtUtc = p.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedPolicyResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<PolicyDetailDto> GetPolicyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var policy = await _db.Policies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (policy == null)
        {
            throw new KeyNotFoundException($"Policy {id} was not found.");
        }

        return MapPolicyDetail(policy);
    }

    public async Task<PolicyDetailDto> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        if (await _db.Policies.AnyAsync(p => p.PolicyNumber == request.PolicyNumber, cancellationToken))
        {
            throw new InvalidOperationException("A policy with this number already exists.");
        }

        var record = new PolicyRecord
        {
            Id = Guid.NewGuid(),
            PolicyNumber = request.PolicyNumber.Trim(),
            CustomerName = request.CustomerName.Trim(),
            CustomerIdentifier = request.CustomerIdentifier.Trim(),
            PolicyType = request.PolicyType.Trim(),
            Insurer = request.Insurer.Trim(),
            CurrentStatus = PolicyStatus.Draft,
            Premium = request.Premium,
            BillingFrequency = request.BillingFrequency,
            CoverageAmount = request.CoverageAmount,
            Deductible = request.Deductible,
            EffectiveDate = request.EffectiveDate,
            ExpirationDate = request.ExpirationDate,
            AssignedAgent = request.AssignedAgent?.Trim(),
            ClaimReference = request.ClaimReference?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.Policies.Add(record);
        await _db.SaveChangesAsync(cancellationToken);

        return MapPolicyDetail(record);
    }

    public async Task<PolicyDetailDto> UpdatePolicyAsync(Guid id, UpdatePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var policy = await _db.Policies.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (policy == null)
        {
            throw new KeyNotFoundException($"Policy {id} was not found.");
        }

        if (!CanEdit(policy.CurrentStatus))
        {
            throw new InvalidOperationException("This policy is not editable in its current state.");
        }

        if (!string.IsNullOrWhiteSpace(request.PolicyNumber))
        {
            var value = request.PolicyNumber.Trim();
            if (value.Length < 3) throw new InvalidOperationException("Policy number is required.");
            if (await _db.Policies.AnyAsync(p => p.Id != id && p.PolicyNumber == value, cancellationToken))
            {
                throw new InvalidOperationException("A policy with this number already exists.");
            }

            policy.PolicyNumber = value;
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerName)) policy.CustomerName = request.CustomerName.Trim();
        if (!string.IsNullOrWhiteSpace(request.CustomerIdentifier)) policy.CustomerIdentifier = request.CustomerIdentifier.Trim();
        if (!string.IsNullOrWhiteSpace(request.PolicyType)) policy.PolicyType = request.PolicyType.Trim();
        if (!string.IsNullOrWhiteSpace(request.Insurer)) policy.Insurer = request.Insurer.Trim();
        if (request.Premium.HasValue) policy.Premium = request.Premium.Value;
        if (request.BillingFrequency.HasValue) policy.BillingFrequency = request.BillingFrequency.Value;
        if (request.CoverageAmount.HasValue) policy.CoverageAmount = request.CoverageAmount.Value;
        if (request.Deductible.HasValue) policy.Deductible = request.Deductible.Value;
        if (request.EffectiveDate.HasValue) policy.EffectiveDate = request.EffectiveDate.Value;
        if (request.ExpirationDate.HasValue) policy.ExpirationDate = request.ExpirationDate.Value;
        if (request.AssignedAgent != null) policy.AssignedAgent = request.AssignedAgent.Trim();
        if (request.ClaimReference != null) policy.ClaimReference = request.ClaimReference.Trim();
        if (request.Notes != null) policy.Notes = request.Notes.Trim();

        if (policy.EffectiveDate >= policy.ExpirationDate)
        {
            throw new InvalidOperationException("Effective date must be before expiration date.");
        }

        if (policy.Premium <= 0 || policy.CoverageAmount <= 0)
        {
            throw new InvalidOperationException("Premium and coverage amount must be greater than zero.");
        }

        policy.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return MapPolicyDetail(policy);
    }

    public async Task DeletePolicyAsync(Guid id, bool confirmation, CancellationToken cancellationToken = default)
    {
        var policy = await _db.Policies.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (policy == null)
        {
            throw new KeyNotFoundException($"Policy {id} was not found.");
        }

        if (!confirmation)
        {
            throw new InvalidOperationException("Deletion requires explicit confirmation.");
        }

        if (!CanDelete(policy.CurrentStatus))
        {
            throw new InvalidOperationException("This policy cannot be deleted in its current state.");
        }

        _db.Policies.Remove(policy);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PolicyDetailDto> ApplyLifecycleActionAsync(Guid id, string actionName, CancellationToken cancellationToken = default)
    {
        var policy = await _db.Policies.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (policy == null)
        {
            throw new KeyNotFoundException($"Policy {id} was not found.");
        }

        var action = actionName.Trim();
        var validActions = GetAvailableActions(policy.CurrentStatus);
        if (!validActions.Contains(action, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Action '{action}' is not valid for status '{policy.CurrentStatus}'.");
        }

        switch (policy.CurrentStatus)
        {
            case PolicyStatus.Draft when action.Equals("Quote", StringComparison.OrdinalIgnoreCase):
                policy.CurrentStatus = PolicyStatus.Quoted;
                break;
            case PolicyStatus.Draft when action.Equals("Delete", StringComparison.OrdinalIgnoreCase):
                _db.Policies.Remove(policy);
                await _db.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException("Policy deleted successfully.");
            case PolicyStatus.Quoted when action.Equals("Purchase", StringComparison.OrdinalIgnoreCase):
                policy.CurrentStatus = PolicyStatus.Active;
                break;
            case PolicyStatus.Quoted when action.Equals("Delete", StringComparison.OrdinalIgnoreCase):
                _db.Policies.Remove(policy);
                await _db.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException("Policy deleted successfully.");
            case PolicyStatus.Active when action.Equals("File claim", StringComparison.OrdinalIgnoreCase):
                policy.CurrentStatus = PolicyStatus.ClaimInReview;
                policy.ClaimReference ??= $"CLAIM-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
                break;
            case PolicyStatus.Active when action.Equals("Suspend", StringComparison.OrdinalIgnoreCase):
                policy.CurrentStatus = PolicyStatus.Suspended;
                break;
            case PolicyStatus.Active when action.Equals("Cancel", StringComparison.OrdinalIgnoreCase):
                policy.CurrentStatus = PolicyStatus.Cancelled;
                break;
            case PolicyStatus.Active when action.Equals("Term ends", StringComparison.OrdinalIgnoreCase):
                policy.CurrentStatus = PolicyStatus.Expired;
                break;
            case PolicyStatus.Suspended when action.Equals("Pay premium", StringComparison.OrdinalIgnoreCase):
                policy.CurrentStatus = PolicyStatus.Active;
                break;
            case PolicyStatus.Suspended when action.Equals("Cancel", StringComparison.OrdinalIgnoreCase):
                policy.CurrentStatus = PolicyStatus.Cancelled;
                break;
            case PolicyStatus.ClaimInReview when action.Equals("Withdraw claim", StringComparison.OrdinalIgnoreCase):
                policy.CurrentStatus = PolicyStatus.Active;
                break;
            case PolicyStatus.ClaimInReview when action.Equals("View claim", StringComparison.OrdinalIgnoreCase):
                // No state change; this is a valid informational action for the current review state.
                break;
            case PolicyStatus.Expired when action.Equals("Renew", StringComparison.OrdinalIgnoreCase):
                policy.CurrentStatus = PolicyStatus.Active;
                break;
            default:
                throw new InvalidOperationException($"Unsupported lifecycle action '{action}' for state '{policy.CurrentStatus}'.");
        }

        policy.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return MapPolicyDetail(policy);
    }

    private static void ValidateCreateRequest(CreatePolicyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PolicyNumber)) throw new InvalidOperationException("Policy number is required.");
        if (string.IsNullOrWhiteSpace(request.CustomerName)) throw new InvalidOperationException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(request.CustomerIdentifier)) throw new InvalidOperationException("Customer identifier is required.");
        if (string.IsNullOrWhiteSpace(request.PolicyType)) throw new InvalidOperationException("Policy type is required.");
        if (string.IsNullOrWhiteSpace(request.Insurer)) throw new InvalidOperationException("Insurer is required.");
        if (request.Premium <= 0) throw new InvalidOperationException("Premium must be greater than zero.");
        if (request.CoverageAmount <= 0) throw new InvalidOperationException("Coverage amount must be greater than zero.");
        if (request.EffectiveDate >= request.ExpirationDate) throw new InvalidOperationException("Effective date must be before expiration date.");
    }

    private static PolicyDetailDto MapPolicyDetail(PolicyRecord policy)
    {
        return new PolicyDetailDto
        {
            Id = policy.Id,
            PolicyNumber = policy.PolicyNumber,
            CustomerName = policy.CustomerName,
            CustomerIdentifier = policy.CustomerIdentifier,
            PolicyType = policy.PolicyType,
            Insurer = policy.Insurer,
            CurrentStatus = policy.CurrentStatus.ToString(),
            Premium = policy.Premium,
            BillingFrequency = policy.BillingFrequency.ToString(),
            CoverageAmount = policy.CoverageAmount,
            Deductible = policy.Deductible,
            EffectiveDate = policy.EffectiveDate,
            ExpirationDate = policy.ExpirationDate,
            AssignedAgent = policy.AssignedAgent,
            ClaimReference = policy.ClaimReference,
            Notes = policy.Notes,
            CreatedAtUtc = policy.CreatedAtUtc,
            UpdatedAtUtc = policy.UpdatedAtUtc,
            AvailableActions = GetAvailableActions(policy.CurrentStatus),
            CanEdit = CanEdit(policy.CurrentStatus),
            CanDelete = CanDelete(policy.CurrentStatus)
        };
    }

    private static bool CanEdit(PolicyStatus status) => status == PolicyStatus.Draft || status == PolicyStatus.Quoted;

    private static bool CanDelete(PolicyStatus status) => status == PolicyStatus.Draft || status == PolicyStatus.Quoted;

    private static string[] GetAvailableActions(PolicyStatus status)
    {
        return status switch
        {
            PolicyStatus.Draft => new[] { "Quote", "Delete" },
            PolicyStatus.Quoted => new[] { "Purchase", "Delete" },
            PolicyStatus.Active => new[] { "File claim", "Suspend", "Cancel", "Term ends" },
            PolicyStatus.ClaimInReview => new[] { "Withdraw claim", "View claim" },
            PolicyStatus.Suspended => new[] { "Pay premium", "Cancel" },
            PolicyStatus.Expired => new[] { "Renew" },
            PolicyStatus.Cancelled => Array.Empty<string>(),
            _ => Array.Empty<string>()
        };
    }
}
