using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Interfaces.Services;

/// <summary>
/// Defines the contract for member operations.
/// </summary>
public interface IMemberService
{
    /// <summary>
    /// Retrieves a paginated list of members.
    /// </summary>
    Task<PagedResult<MemberResponse>> GetPagedAsync(PagedQuery query, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a member by their identifier.
    /// </summary>
    Task<MemberResponse> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new member.
    /// </summary>
    Task<MemberResponse> CreateAsync(MemberRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing member.
    /// </summary>
    Task UpdateAsync(int id, MemberRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a member.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
}
