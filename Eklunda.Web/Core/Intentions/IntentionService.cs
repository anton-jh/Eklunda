using Eklunda.Web.Core.Users;
using Eklunda.Web.Persistence;
using Eklunda.Web.Shared;
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

    public Task<Intention?> Get(IntentionId id, CancellationToken cancellationToken)
        => dbContext.Intentions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<Intention>> GetDueReminders(DateTimeOffset now, CancellationToken cancellationToken)
        => dbContext.Intentions
            .Where(x => x.NextReminderAt <= now && x.DoneAt == null)
            .ToListAsync(cancellationToken);

    public Task<Intention?> GetOpenFor(string doneWhenEvent, CancellationToken cancellationToken)
        => dbContext.Intentions
            .Where(x => x.DoneWhenEvent == doneWhenEvent && x.DoneAt == null)
            .OrderBy(x => x.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

    public Intention Create(string description, UserId createdBy, string? doneWhenEvent = null)
    {
        var intention = new Intention(Strings.Clean(description, MaxDescriptionLength), createdBy, doneWhenEvent);
        dbContext.Intentions.Add(intention);
        return intention;
    }

    // First one wins: returns the existing open intention if someone already took it.
    public async Task<(Intention Intention, bool Created)> TakeFirst(
        string description, UserId user, string doneWhenEvent, CancellationToken cancellationToken)
    {
        var open = await GetOpenFor(doneWhenEvent, cancellationToken);
        return open is null ? (Create(description, user, doneWhenEvent), true) : (open, false);
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
        if (await GetOwnOpen(id, user, cancellationToken) is { } intention)
            dbContext.Intentions.Remove(intention);
    }

    public async Task SetReminder(IntentionId id, UserId user, DateTimeOffset? at, CancellationToken cancellationToken)
        => (await GetOwnOpen(id, user, cancellationToken))?.SetReminder(at);

    public async Task StopReminding(IntentionId id, UserId user, CancellationToken cancellationToken)
        => (await GetOwnOpen(id, user, cancellationToken))?.StopReminding();

    private async Task<Intention?> GetOwnOpen(IntentionId id, UserId user, CancellationToken cancellationToken)
    {
        var intention = await Get(id, cancellationToken);
        return intention is { IsDone: false } && intention.CreatedBy == user ? intention : null;
    }
}
