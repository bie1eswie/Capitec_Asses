using Bams.Application;
using Bams.Api.Contracts.Appointments;
using Microsoft.AspNetCore.Mvc;

namespace Bams.Api.Controllers;

using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("api/v1")]
public sealed class AppointmentsController(IAppointmentService appointments) : ControllerBase
{
    [HttpGet("availability")]
    public async Task<IActionResult> Availability([FromQuery] Guid branchId, [FromQuery] Guid serviceId, [FromQuery] DateOnly date, CancellationToken ct) => Ok(await appointments.AvailabilityAsync(branchId, serviceId, date, ct));

    [Authorize(Policy = "Customer")]
    [HttpPost("appointments")]
    public async Task<IActionResult> Book([FromBody] CreateAppointmentRequest request, [FromHeader(Name = "Idempotency-Key")] string? key, CancellationToken ct)
    {
        var command = new CreateAppointmentCommand
        {
            BranchId = request.BranchId,
            ServiceId = request.ServiceId,
            StartsAt = request.StartsAt
        };
        var booked = await appointments.BookAsync(command, key ?? "", ct);
        return StatusCode(201, booked);
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> ListMine(CancellationToken ct) => Ok(await appointments.ListMineAsync(ct));

    [HttpGet("appointments/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        (await appointments.GetAsync(id, ct)) is { } item ? Ok(item) : NotFound();

    [HttpPost("appointments/{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) => Ok(await appointments.CancelAsync(id, ct));

    [HttpPost("appointments/{id:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequest request, CancellationToken ct) =>
        Ok(await appointments.RescheduleAsync(new RescheduleAppointmentCommand(id, request.StartsAt), ct));
}
