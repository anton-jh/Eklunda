using Eklunda.Web.Core.Users;
using Eklunda.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Core.Intentions;

public class IntentionService(ApplicationDbContext dbContext)
{
    public const int MaxDescriptionLength = 200;
    public const int KeepDone = 20;

    public Task<List<Intention>> GetOpen(CancellationToken cancellationToken)
        => dbContext.Intentions
            .Where(x => x.DoneAt == null)
            .OrderBy(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

    public Task<List<Intention>> GetDone(CancellationToken cancellationToken)
        => dbContext.Intentions
            .Where(x => x.DoneAt != null)
            .OrderByDescending(x => x.DoneAt)
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
        var intentions = await dbContext.Intentions
            .Where(x => x.Id == id && x.DoneAt == null)
            .ToListAsync(cancellationToken);

        await MarkDone(intentions, DateTimeOffset.UtcNow, user, cancellationToken);
    }

    public async Task MarkDoneFor(
        string doneWhenEvent, DateTimeOffset at, UserId? by, CancellationToken cancellationToken)
    {
        var intentions = await dbContext.Intentions
            .Where(x => x.DoneWhenEvent == doneWhenEvent && x.DoneAt == null)
            .ToListAsync(cancellationToken);

        await MarkDone(intentions, at, by, cancellationToken);
    }

    // Only the latest KeepDone done intentions are kept, counting the ones marked now.
    private async Task MarkDone(
        List<Intention> intentions, DateTimeOffset at, UserId? by, CancellationToken cancellationToken)
    {
        if (intentions.Count == 0)
            return;

        var old = await dbContext.Intentions
            .Where(x => x.DoneAt != null)
            .OrderByDescending(x => x.DoneAt)
            .Skip(Math.Max(KeepDone - intentions.Count, 0))
            .ToListAsync(cancellationToken);
        dbContext.Intentions.RemoveRange(old);

        foreach (var intention in intentions)
            intention.MarkDone(at, by);
    }

    public async Task Release(IntentionId id, UserId user, CancellationToken cancellationToken)
    {
        var intention = await dbContext.Intentions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (intention is { IsDone: false } && intention.CreatedBy == user)
            dbContext.Intentions.Remove(intention);
    }
}
