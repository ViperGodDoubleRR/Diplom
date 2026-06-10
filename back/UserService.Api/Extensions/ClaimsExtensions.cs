using System.Security.Claims;

namespace UserService.Api.Extensions
{
    public static class ClaimsExtensions
    {
        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (claim is null || !Guid.TryParse(claim.Value, out var userId))
                return null;

            return userId;
        }
    }
}
