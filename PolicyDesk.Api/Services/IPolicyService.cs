using PolicyDesk.Api.Models.Dtos;

namespace PolicyDesk.Api.Services;

public interface IPolicyService
{
    Task<PagedPolicyResult> GetPoliciesAsync(PolicyListRequest request, CancellationToken cancellationToken = default);
    Task<PolicyDetailDto> GetPolicyByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PolicyDetailDto> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken cancellationToken = default);
    Task<PolicyDetailDto> UpdatePolicyAsync(Guid id, UpdatePolicyRequest request, CancellationToken cancellationToken = default);
    Task DeletePolicyAsync(Guid id, bool confirmation, CancellationToken cancellationToken = default);
    Task<PolicyDetailDto> ApplyLifecycleActionAsync(Guid id, string actionName, CancellationToken cancellationToken = default);
}
