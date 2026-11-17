using Eklunda.Web.Core.Users;

namespace Eklunda.Web.Modules.Suggestions.Models;

public class Suggestion(UserId createdBy)
{
    public const int MaxTitleLength = 100;
    public const int MaxDescriptionLength = 2000;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTimeOffset CreatedDate { get; private set; } = DateTimeOffset.UtcNow;
    public UserId CreatedBy { get; private set; } = createdBy;
    public string Title { get; private set; } = "";
    public string Description { get; private set; } = "";

    public void Set(string title, string? description)
    {
        Title = Clean(title, MaxTitleLength);
        Description = Clean(description, MaxDescriptionLength);
    }

    private static string Clean(string? value, int maxLength)
    {
        value = (value ?? "").Trim();
        return value.Length > maxLength ? value[..maxLength] : value;
    }
}
