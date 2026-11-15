using Eklunda.Web.Core.Users;

namespace Eklunda.Web.Core.Intentions;

public class Intention(string description, UserId createdBy)
{
    public IntentionId Id { get; private set; } = new(Guid.NewGuid());
    public DateTimeOffset CreatedDate { get; private set; } = DateTimeOffset.UtcNow;
    public UserId CreatedBy { get; private set; } = createdBy;
    public string Description { get; set; } = description;
    public bool Done { get; set; } = false;
    public string? DoneWhenEvent { get; set; }
}

public record IntentionId(Guid Value);
