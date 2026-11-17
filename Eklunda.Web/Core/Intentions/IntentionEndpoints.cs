using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Shared;

namespace Eklunda.Web.Core.Intentions;

// Called from the reminder notification's buttons (see wwwroot/sw.js).
public static class IntentionEndpoints
{
    public static string DoneUrl(IntentionId id) => $"{Base(id)}/done";
    public static string ReleaseUrl(IntentionId id) => $"{Base(id)}/release";
    public static string StopRemindingUrl(IntentionId id) => $"{Base(id)}/stop-reminding";

    private static string Base(IntentionId id) => $"/api/intentions/{id.Value:N}";

    private static readonly PushActionResult Gone = new("Avsikten finns inte längre", "");

    public static IEndpointRouteBuilder MapIntentionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/intentions/{id:guid}");

        group.MapPost("/done", async (
            Guid id, IntentionService intentionService, ICurrentUser currentUser, IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var intention = await intentionService.Get(new IntentionId(id), cancellationToken);
            if (intention is null)
                return Gone;
            if (intention.IsDone)
                return new PushActionResult("Redan klar", intention.Description);

            await intentionService.MarkDone(intention.Id, currentUser.Id, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return new PushActionResult("Klar ✓", intention.Description);
        });

        group.MapPost("/release", async (
            Guid id, IntentionService intentionService, ICurrentUser currentUser, IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var intention = await intentionService.Get(new IntentionId(id), cancellationToken);
            if (intention is null)
                return Gone;
            if (intention.IsDone)
                return new PushActionResult("Redan klar", intention.Description);

            await intentionService.Release(intention.Id, currentUser.Id, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return new PushActionResult("Släppt", intention.Description);
        });

        group.MapPost("/stop-reminding", async (
            Guid id, IntentionService intentionService, ICurrentUser currentUser, IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            await intentionService.StopReminding(new IntentionId(id), currentUser.Id, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return Results.NoContent();
        });

        return app;
    }
}
