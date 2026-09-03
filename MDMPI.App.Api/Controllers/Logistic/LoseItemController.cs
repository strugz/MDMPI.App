using MDMPI.App.Core.Logistic.DTOs.LoseItem;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MDMPI.App.Api.Controllers.Logistic
{
    /// <summary>
    /// Lost items on Pull Out / Return requests: items listed on the request
    /// that were NOT actually pulled out, each with the courier's remarks.
    /// A request id must exist in a_tblRequestPullOutReturnPickUp.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoseItemController : ControllerBase
    {
        private readonly ILoseItemService _service;
        private readonly ILogger<LoseItemController> _logger;

        public LoseItemController(ILoseItemService service, ILogger<LoseItemController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get the lost items (with remarks) recorded for a pull-out request.
        /// </summary>
        [HttpGet("request/{requestId:long}")]
        public async Task<ActionResult<List<FetchLoseItemDto>>> GetByRequestId(long requestId)
        {
            if (requestId <= 0)
                return BadRequest("requestId must be greater than zero.");

            if (!await _service.PullOutRequestExistsAsync(requestId))
                return NotFound("RequestID not found in pull out requests.");

            var items = await _service.GetByRequestIdAsync(requestId);
            return Ok(items);
        }

        /// <summary>
        /// Replace the lost-item set for a pull-out request. Posting an empty
        /// list clears previously recorded lost items.
        /// </summary>
        [HttpPost("request/{requestId:long}")]
        public async Task<ActionResult> ReplaceForRequest(long requestId, [FromBody] List<InsertLoseItemDto> items)
        {
            if (requestId <= 0)
                return BadRequest("requestId must be greater than zero.");

            if (items is null)
                return BadRequest("Items payload is required.");

            if (!await _service.PullOutRequestExistsAsync(requestId))
                return NotFound("RequestID not found in pull out requests.");

            var result = await _service.ReplaceForRequestAsync(requestId, items);
            if (!result)
            {
                _logger.LogWarning("Failed to save lost items for RequestID: {RequestID}", requestId);
                return StatusCode(500, "Failed to save lost items.");
            }

            return Ok();
        }
    }
}
