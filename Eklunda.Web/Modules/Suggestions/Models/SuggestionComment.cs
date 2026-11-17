using Eklunda.Web.Core.Users;
using Eklunda.Web.Shared;

namespace Eklunda.Web.Modules.Suggestions.Models;

public class SuggestionComment(Guid suggestionId, UserId createdBy)
{
    public const int MaxTextLength = 1000;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid SuggestionId { get; private set; } = suggestionId;
    public DateTimeOffset CreatedDate { get; private set; } = DateTimeOffset.UtcNow;
    public UserId CreatedBy { get; private set; } = createdBy;
    public string Text { get; private set; } = "";

    // Returns whether anything changed.
    public bool Set(string text)
    {
        var clean = Strings.Clean(text, MaxTextLength);
        if (clean == Text)
            return false;

        Text = clean;
        return true;
    }
}
