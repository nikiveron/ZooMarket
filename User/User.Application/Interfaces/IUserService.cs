using User.Application.DTOs;
using User.Application.Features.Users;

namespace User.Application.Interfaces;

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
