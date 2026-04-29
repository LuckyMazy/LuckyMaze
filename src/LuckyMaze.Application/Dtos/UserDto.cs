using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Application.Dtos;

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    UserRole Role,
    bool IsLocked);
