using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Domain
{
    public class User : BaseEntity
    {
        public required string ExternalId { get; init; }
        public required string Email { get; set; }
        public string DisplayName { get; set; } = "LuckyMaze User";
        public UserRole Role { get; set; } = UserRole.User;
        public string? AvatarUrl { get; set; }
        public bool IsLocked { get; set; }
        public decimal Balance { get; set; } = 1000.00m;
        public bool IsReady { get; set; }
        public required UserPreferences Preferences { get; set; } = new UserPreferences();
    }
}
