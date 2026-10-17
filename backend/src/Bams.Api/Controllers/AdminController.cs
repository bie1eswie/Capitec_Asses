using System;
using System.Threading;
using System.Threading.Tasks;
using Bams.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bams.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController : ControllerBase
{
    private readonly IOutboxRepository outbox;

    public AdminController(IOutboxRepository outbox)
    {
        this.outbox = outbox;
    }

    [HttpPost("outbox/{id:guid}/replay")]
    public async Task<IActionResult> Replay(Guid id, CancellationToken ct)
    {
        var replayed = await outbox.ReplayDeadLetterAsync(id, ct);
        return replayed ? Accepted(new { eventId = id }) : NotFound();
    }
}
