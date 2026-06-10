using UserService.Application.DTO;
using UserService.Domain.Models;

namespace UserService.Application.Mapping
{
    public static class UserProfileMapper
    {
        public static UserProfileDto ToProfileDto(User user) =>
            new()
            {
                Id = user.Id,
                Email = user.Email,
                Login = user.Login,
                Tag = user.Tag,
                Description = user.Description,
                CreatedAt = user.CreatedAt
            };
    }
}
