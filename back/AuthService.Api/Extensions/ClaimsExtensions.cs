using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.Api.Extensions
{
    public static class ClaimsExtensions
    {
        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            var claim =
                user.FindFirst(ClaimTypes.NameIdentifier)
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirst("sub");

            if (claim is null || !Guid.TryParse(claim.Value, out var userId))
                return null;

            return userId;
        }
    }
}
