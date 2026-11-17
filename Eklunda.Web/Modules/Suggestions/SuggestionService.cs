using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Suggestions.Models;
using Eklunda.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Modules.Suggestions;

public class SuggestionService(ApplicationDbContext dbContext)
{
    public Task<List<Suggestion>> GetActive(CancellationToken cancellationToken)
        => dbContext.Suggestions
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

    public Task<List<Suggestion>> GetDeleted(CancellationToken cancellationToken)
        => dbContext.Suggestions
            .Where(x => x.DeletedAt != null)
            .OrderByDescending(x => x.DeletedAt)
            .ToListAsync(cancellationToken);

    public Task<int> CountDeleted(CancellationToken cancellationToken)
        => dbContext.Suggestions.CountAsync(x => x.DeletedAt != null, cancellationToken);

    // Includes deleted suggestions.
    public Task<Suggestion?> Get(Guid id, CancellationToken cancellationToken)
        => dbContext.Suggestions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Suggestion Create(UserId createdBy, string title, string? description)
    {
        var suggestion = new Suggestion(createdBy);
        suggestion.Set(title, description);
        dbContext.Suggestions.Add(suggestion);
        return suggestion;
    }

    public Task<List<SuggestionComment>> GetComments(Guid suggestionId, CancellationToken cancellationToken)
        => dbContext.SuggestionComments
            .Where(x => x.SuggestionId == suggestionId)
            .OrderBy(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

    public Task<Dictionary<Guid, int>> GetCommentCounts(CancellationToken cancellationToken)
        => dbContext.SuggestionComments
            .GroupBy(x => x.SuggestionId)
            .ToDictionaryAsync(x => x.Key, x => x.Count(), cancellationToken);

    // Only the creator's own comments.
    public Task<SuggestionComment?> GetOwnComment(
        Guid suggestionId, Guid id, UserId user, CancellationToken cancellationToken)
        => dbContext.SuggestionComments.SingleOrDefaultAsync(
            x => x.Id == id && x.SuggestionId == suggestionId && x.CreatedBy == user, cancellationToken);

    public SuggestionComment AddComment(Suggestion suggestion, UserId createdBy, string text)
    {
        var comment = new SuggestionComment(suggestion.Id, createdBy);
        comment.Set(text);
        dbContext.SuggestionComments.Add(comment);
        return comment;
    }

    public void DeleteComment(SuggestionComment comment)
        => dbContext.SuggestionComments.Remove(comment);
}
