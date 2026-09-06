using CostEstimating.Domain.Enums;

namespace CostEstimating.Domain.Entities;

public sealed class User
{
    private User() { }

    public User(Guid id, string email, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        Id = id;
        Email = email.Trim().ToLowerInvariant();
        Role = role;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
}
