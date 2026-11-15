using System.Security.Cryptography;
using Eklunda.Web.Persistence;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Core.Notifications;

// Generated on first use, so nothing has to be configured on the Pi.
// Changing the keys invalidates every push subscription.
public class VapidKeyProvider(IServiceScopeFactory scopeFactory)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private VapidKeys? keys;

    public async Task<VapidKeys> Get(CancellationToken cancellationToken)
    {
        if (keys is not null)
            return keys;

        await gate.WaitAsync(cancellationToken);
        try
        {
            if (keys is not null)
                return keys;

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            keys = await dbContext.VapidKeys.SingleOrDefaultAsync(cancellationToken);
            if (keys is null)
            {
                keys = Generate();
                dbContext.VapidKeys.Add(keys);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return keys;
        }
        finally
        {
            gate.Release();
        }
    }

    private static VapidKeys Generate()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var parameters = ecdsa.ExportParameters(includePrivateParameters: true);

        // Uncompressed point: 0x04 || X || Y.
        var publicKey = new byte[65];
        publicKey[0] = 0x04;
        parameters.Q.X!.CopyTo(publicKey, 1);
        parameters.Q.Y!.CopyTo(publicKey, 33);

        return new VapidKeys(
            WebEncoders.Base64UrlEncode(publicKey),
            WebEncoders.Base64UrlEncode(parameters.D!));
    }
}
