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
    [HttpPost("Search")]
    public async Task<IActionResult> SearchEndpoint(
        [FromBody] SearchRequest searchRequest,
        CancellationToken cancellation
    )
    {
        UnifiedSearch.Query query = new UnifiedSearch.Query(searchRequest);
        var result = await _mediator.Send(query, cancellation);
        return result.ToActionResult();
    }
}