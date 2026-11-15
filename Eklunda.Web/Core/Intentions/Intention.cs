using Eklunda.Web.Core.Users;

namespace Eklunda.Web.Core.Intentions;

public class Intention(string description, UserId createdBy, string? doneWhenEvent = null)
{
    public IntentionId Id { get; private set; } = new(Guid.NewGuid());
    public DateTimeOffset CreatedDate { get; private set; } = DateTimeOffset.UtcNow;
    public UserId CreatedBy { get; private set; } = createdBy;
    public string Description { get; set; } = description;
    public DateTimeOffset? DoneAt { get; private set; }
    public UserId? DoneBy { get; private set; }
    public string? DoneWhenEvent { get; private set; } = doneWhenEvent;

    public bool IsDone => DoneAt is not null;

    public void MarkDone(DateTimeOffset at, UserId? by)
    {
        if (IsDone)
            return;

        DoneAt = at;
        DoneBy = by;
    }
}

public record IntentionId(Guid Value);
