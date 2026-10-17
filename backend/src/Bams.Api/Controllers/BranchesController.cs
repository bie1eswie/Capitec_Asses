using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bams.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bams.Api.Controllers;


[ApiController]
[Route("api/v1/branches")]
[Authorize]
public sealed class BranchesController : ControllerBase
{
    private readonly IBranchQueries reader;

    public BranchesController(IBranchQueries reader)
    {
        this.reader = reader;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BranchDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? search, CancellationToken ct)
    {
        var items = await reader.ListAsync(search, ct);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BranchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var branch = await reader.FindAsync(id, ct);
        return branch is null ? NotFound() : Ok(branch);
    }

    [HttpGet("{id:guid}/services")]
    public async Task<IActionResult> Services(Guid id, CancellationToken ct)
    {
        if (await reader.FindAsync(id, ct) is null)
        {
            return NotFound();
        }

        var services = await reader.ServicesAsync(id, ct);
        return Ok(services);
    }
}
