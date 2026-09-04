using MastercardCardUpgrade.Api.Models;
using MastercardCardUpgrade.Api.Models.Acs;
using MastercardCardUpgrade.Api.Options;
using MastercardCardUpgrade.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Controllers;

[ApiController]
[Route("api/mastercard")]
public sealed class MastercardController : ControllerBase
{
    [HttpPost("bin-lookup")]
    public async Task<ActionResult<BinAccountRangeResponse>> BinLookup(
        [FromBody] BinLookupApiRequest request,
        [FromServices] IMastercardBinLookupClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.SearchAccountRangeAsync(
            request.PanOrAccountRange,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("sandbox/bin-lookup")]
    public async Task<ActionResult<BinAccountRangeResponse>> SandboxBinLookup(
        [FromBody] BinLookupApiRequest? request,
        [FromServices] IMastercardBinLookupClient client,
        [FromServices] IOptions<MastercardOptions> options,
        CancellationToken cancellationToken)
    {
        var accountRange = string.IsNullOrWhiteSpace(request?.PanOrAccountRange)
            ? options.Value.SandboxSampleAccountRange
            : request.PanOrAccountRange;

        var result = await client.SearchAccountRangeAsync(accountRange, cancellationToken);
        return Ok(result);
    }

    [HttpPost("upgrade")]
    public async Task<ActionResult<CardUpgradeResult>> Upgrade(
        [FromBody] CardUpgradeRequest request,
        [FromServices] IMastercardUpgradeService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpgradeAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("alm/status/{reference}")]
    public async Task<ActionResult<AcsOperationResult>> Status(
        string reference,
        [FromServices] IAcsClient client,
        CancellationToken cancellationToken)
    {
        var result = await client.GetStatusAsync(reference, cancellationToken);
        return Ok(result);
    }
}

public sealed record BinLookupApiRequest(string PanOrAccountRange);
