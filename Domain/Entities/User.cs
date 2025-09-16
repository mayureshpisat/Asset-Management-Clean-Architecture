namespace Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    public string Role { get; set; } = "Viewer";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
