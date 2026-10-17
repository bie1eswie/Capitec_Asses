using System.Threading;
using System.Threading.Tasks;
using Bams.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bams.Api.Controllers;

[ApiController]
[Route("api/v1/services")]
public sealed class ServicesController : ControllerBase
{
    private readonly IServiceQueries reader;

    public ServicesController(IServiceQueries reader)
    {
        this.reader = reader;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await reader.ListAsync(ct);
        return Ok(items);
    }
}
