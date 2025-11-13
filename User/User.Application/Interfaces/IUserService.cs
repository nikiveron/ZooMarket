using Identity.Application.DTOs;
using Identity.Application.Features.Users;

namespace Identity.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<UserDto> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> CreateUserAsync(CreateUserCommand command);
    Task UpdateUserAsync(UpdateUserCommand command);
    Task DeleteUserAsync(Guid id);
    Task<AuthResponseDto> LoginAsync(LoginCommand command);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenCommand command);
}
