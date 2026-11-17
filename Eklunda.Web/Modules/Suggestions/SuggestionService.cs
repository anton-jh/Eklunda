using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Suggestions.Models;
using Eklunda.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Modules.Suggestions;

public class SuggestionService(ApplicationDbContext dbContext)
{
    public Task<List<Suggestion>> GetAll(CancellationToken cancellationToken)
        => dbContext.Suggestions
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

    public Task<Suggestion?> Get(Guid id, CancellationToken cancellationToken)
        => dbContext.Suggestions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Suggestion Create(UserId createdBy, string title, string? description)
    {
        var suggestion = new Suggestion(createdBy);
        suggestion.Set(title, description);
        dbContext.Suggestions.Add(suggestion);
        return suggestion;
    }

    public void Delete(Suggestion suggestion)
        => dbContext.Suggestions.Remove(suggestion);
}
