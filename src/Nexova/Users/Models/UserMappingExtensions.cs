using Nexova.Core.Entities;

namespace Nexova.Users.Models;

public static class UserMappingExtensions
{
    public static UserResponse ToResponse(this User user) =>
        new(
            user.Id,
            user.Name,
            user.Email,
            user.AvatarUrl,
            user.CreatedAt,
            user.UpdatedAt);
}
