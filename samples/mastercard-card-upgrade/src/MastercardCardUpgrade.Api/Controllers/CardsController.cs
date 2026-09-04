using MastercardCardUpgrade.Api.Models.Cards;
using MastercardCardUpgrade.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MastercardCardUpgrade.Api.Controllers;

[ApiController]
[Route("api/cards")]
public sealed class CardsController : ControllerBase
{
    private readonly ICardLifecycleService _lifecycle;
    private readonly IProductCatalog _catalog;

    public CardsController(ICardLifecycleService lifecycle, IProductCatalog catalog)
    {
        _lifecycle = lifecycle;
        _catalog = catalog;
    }

    [HttpGet("/api/products")]
    public IActionResult Products() => Ok(_catalog.Products);

    [HttpPost]
    public async Task<ActionResult<CardResponse>> Create(
        [FromBody] CreateCardRequest request,
        CancellationToken cancellationToken)
    {
        var card = await _lifecycle.CreateAsync(request, cancellationToken);
        return Created($"/api/cards/{card.CardId}", card);
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<CardResponse>> List() => Ok(_lifecycle.List());

    [HttpGet("{cardId}")]
    public ActionResult<CardResponse> Get(string cardId) => Ok(_lifecycle.Get(cardId));

    [HttpPost("{cardId}/register")]
    public async Task<ActionResult<MigrationResponse>> Register(
        string cardId,
        [FromQuery] string? correlationId,
        CancellationToken cancellationToken)
    {
        var result = await _lifecycle.RegisterAsync(cardId, correlationId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{cardId}/upgrades")]
    public async Task<ActionResult<MigrationResponse>> Upgrade(
        string cardId,
        [FromBody] UpgradeCardRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _lifecycle.UpgradeAsync(cardId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{cardId}/upgrades")]
    public ActionResult<IReadOnlyList<MigrationResponse>> ListUpgrades(string cardId) =>
        Ok(_lifecycle.ListMigrations(cardId));

    [HttpPost("{cardId}/upgrades/{migrationId}/reconcile")]
    public async Task<ActionResult<MigrationResponse>> Reconcile(
        string cardId,
        string migrationId,
        CancellationToken cancellationToken)
    {
        var result = await _lifecycle.ReconcileAsync(cardId, migrationId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{cardId}/upgrades/{migrationId}/rollback")]
    public async Task<ActionResult<MigrationResponse>> Rollback(
        string cardId,
        string migrationId,
        CancellationToken cancellationToken)
    {
        var result = await _lifecycle.RollbackAsync(cardId, migrationId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{cardId}/close")]
    public async Task<ActionResult<MigrationResponse>> Close(
        string cardId,
        [FromQuery] string? correlationId,
        CancellationToken cancellationToken)
    {
        var result = await _lifecycle.CloseAsync(cardId, correlationId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{cardId}/treatment")]
    public ActionResult<TreatmentCheckResponse> Treatment(string cardId) =>
        Ok(_lifecycle.CheckTreatment(cardId));

    [HttpPost("/api/migrations/reconcile")]
    public async Task<ActionResult<object>> ReconcileOpen(CancellationToken cancellationToken)
    {
        var count = await _lifecycle.ReconcileOpenAsync(cancellationToken);
        return Ok(new { reconciled = count });
    }

    [HttpPost("/api/demo/e2e")]
    public async Task<ActionResult<EndToEndDemoResult>> Demo(
        [FromBody] EndToEndDemoRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _lifecycle.RunDemoAsync(request ?? new EndToEndDemoRequest(), cancellationToken);
        return Ok(result);
    }
}
