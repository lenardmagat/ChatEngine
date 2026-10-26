using System.Runtime.CompilerServices;
using ChatSystem.DTOs.Search;
using ChatSystem.ErrorHandling.Extension;
using ChatSystem.Services.Interfaces;
using ChatSystem.SystemEvents.Documentation;
using ChatSystem.SystemEvents.Search;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Routers.Features;
public partial class FeatureController
{
    [HttpGet("Search")]
    public async Task<IActionResult> SearchEndpoint(
        [FromQuery] string term,
        [FromQuery] SearchTarget target,
        CancellationToken cancellation,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        Dictionary<string, string>? filters = null
    )
    {
        UnifiedSearch.Query query = new UnifiedSearch.Query(
            new SearchRequest(
                term,
                target,
                page,
                pageSize,
                filters      
            )
        );
        var result = await _mediator.Send(query, cancellation);
        return result.ToActionResult();
    }
}