using Eklunda.Web.Core.Users;

namespace Eklunda.Web.Core.Intentions;

public class Intention(string description, UserId createdBy, string? doneWhenEvent = null)
{
    public const int MaxReminders = 6;
    public static readonly TimeSpan ReminderInterval = TimeSpan.FromMinutes(5);

    public IntentionId Id { get; private set; } = new(Guid.NewGuid());
    public DateTimeOffset CreatedDate { get; private set; } = DateTimeOffset.UtcNow;
    public UserId CreatedBy { get; private set; } = createdBy;
    public string Description { get; set; } = description;
    public DateTimeOffset? DoneAt { get; private set; }
    public UserId? DoneBy { get; private set; }
    public string? DoneWhenEvent { get; private set; } = doneWhenEvent;

    // Set by the creator, who is the only one reminded and the only one who sees it.
    public DateTimeOffset? RemindAt { get; private set; }
    public DateTimeOffset? NextReminderAt { get; private set; }
    public int RemindersSent { get; private set; }

    public bool IsDone => DoneAt is not null;

    public void MarkDone(DateTimeOffset at, UserId? by)
    {
        if (IsDone)
            return;

        DoneAt = at;
        DoneBy = by;
        StopReminding();
    }

    public void SetReminder(DateTimeOffset? at)
    {
        RemindAt = at?.ToUniversalTime();
        NextReminderAt = RemindAt;
        RemindersSent = 0;
    }

    public void ReminderSent(DateTimeOffset at)
    {
        RemindersSent++;
        NextReminderAt = RemindersSent < MaxReminders ? at + ReminderInterval : null;
    }

    public void StopReminding()
        => NextReminderAt = null;
}

public record IntentionId(Guid Value);
