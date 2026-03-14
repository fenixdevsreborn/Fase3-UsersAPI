using System.Security.Claims;
using Fcg.Users.Contracts.Auth;

namespace Fcg.Users.Api.Authorization;

/// <summary>Owner-based authorization: only the resource owner or an admin may access.</summary>
public static class OwnerAuthorization
{
    public static bool CanAccessResource(this ClaimsPrincipal user, Guid resourceOwnerId)
    {
        if (user.IsAdmin()) return true;
        var userId = user.GetUserId();
        return userId.HasValue && userId.Value == resourceOwnerId;
    }
}
