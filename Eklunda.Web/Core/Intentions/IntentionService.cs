using Eklunda.Web.Core.Users;
using Eklunda.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Core.Intentions;

public class IntentionService(ApplicationDbContext dbContext)
{
    public const int MaxDescriptionLength = 200;

    public Task<List<Intention>> GetOpen(CancellationToken cancellationToken)
        => dbContext.Intentions
            .Where(x => x.DoneAt == null)
            .OrderBy(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

    public Task<List<Intention>> GetDone(int take, CancellationToken cancellationToken)
        => dbContext.Intentions
            .Where(x => x.DoneAt != null)
            .OrderByDescending(x => x.DoneAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountDone(CancellationToken cancellationToken)
        => dbContext.Intentions.CountAsync(x => x.DoneAt != null, cancellationToken);

    public Task<Intention?> GetOpenFor(string doneWhenEvent, CancellationToken cancellationToken)
        => dbContext.Intentions
            .Where(x => x.DoneWhenEvent == doneWhenEvent && x.DoneAt == null)
            .OrderBy(x => x.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

    public Intention Create(string description, UserId createdBy, string? doneWhenEvent = null)
    {
        description = description.Trim();
        if (description.Length > MaxDescriptionLength)
            description = description[..MaxDescriptionLength];

        var intention = new Intention(description, createdBy, doneWhenEvent);
        dbContext.Intentions.Add(intention);
        return intention;
    }

    // First one wins: returns the existing open intention if someone already took it.
    public async Task<Intention> TakeFirst(
        string description, UserId user, string doneWhenEvent, CancellationToken cancellationToken)
    {
        return await GetOpenFor(doneWhenEvent, cancellationToken)
            ?? Create(description, user, doneWhenEvent);
    }

    public async Task MarkDone(IntentionId id, UserId user, CancellationToken cancellationToken)
    {
        var intention = await dbContext.Intentions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        intention?.MarkDone(DateTimeOffset.UtcNow, user);
    }

    public async Task Release(IntentionId id, UserId user, CancellationToken cancellationToken)
    {
        var intention = await dbContext.Intentions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (intention is { IsDone: false } && intention.CreatedBy == user)
            dbContext.Intentions.Remove(intention);
    }
}
