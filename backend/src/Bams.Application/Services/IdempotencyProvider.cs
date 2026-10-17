using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Bams.Application;

public sealed class IdempotencyProvider
{
    public string ComputeHash(CreateAppointmentCommand command, string ownerUserId)
    {
        var normalized = JsonSerializer.Serialize(new
        {
            ownerUserId,
            command.BranchId,
            command.ServiceId,
            command.StartsAt
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }
}
