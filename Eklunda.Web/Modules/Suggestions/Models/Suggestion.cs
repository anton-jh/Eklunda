using Eklunda.Web.Core.Users;
using Eklunda.Web.Shared;

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
    public DateTimeOffset? DeletedAt { get; private set; }
    public UserId? DeletedBy { get; private set; }

    public bool IsDeleted => DeletedAt is not null;

    // Returns whether anything changed.
    public bool Set(string title, string? description)
    {
        var cleanTitle = Strings.Clean(title, MaxTitleLength);
        var cleanDescription = Strings.Clean(description, MaxDescriptionLength);
        if (cleanTitle == Title && cleanDescription == Description)
            return false;

        Title = cleanTitle;
        Description = cleanDescription;
        return true;
    }

    public void Delete(UserId by)
    {
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = by;
    }

    public void Restore()
    {
        DeletedAt = null;
        DeletedBy = null;
    }
}
