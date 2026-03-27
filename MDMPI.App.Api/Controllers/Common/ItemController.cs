using MDMPI.App.Core.Common.DTOs.Item;
using MDMPI.App.Core.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MDMPI.App.Api.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IItemRepository _itemRepository;
        private readonly ILogger<ItemController> _logger;

        public ItemController(IItemRepository itemRepository, ILogger<ItemController> logger)
        {
            _itemRepository = itemRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all items (and their batches) for a request.
        /// </summary>
        [HttpGet("request/{requestId:long}")]
        public async Task<ActionResult<List<FetchItemDto>>> GetByRequestId(long requestId)
        {
            if (requestId <= 0)
                return BadRequest("requestId must be greater than zero.");

            var items = await _itemRepository.GetItemsByRequestIdAsync(requestId);
            return Ok(items);
        }

        /// <summary>
        /// Insert items (and their batches) for a request.
        /// </summary>
        [HttpPost("request/{requestId:long}")]
        public async Task<ActionResult> InsertForRequest(long requestId, [FromBody] List<InsertItemDto> items)
        {
            if (requestId <= 0)
                return BadRequest("requestId must be greater than zero.");

            if (items is null)
                return BadRequest("Items payload is required.");

            var result = await _itemRepository.InsertItemsAsync(requestId, items);
            if (!result)
            {
                _logger.LogWarning("Failed to insert items for RequestID: {RequestID}", requestId);
                return StatusCode(500, "Failed to insert items.");
            }

            return Ok();
        }

        /// <summary>
        /// Update items (and reconcile their batches) for a request.
        /// </summary>
        [HttpPut("request/{requestId:long}")]
        public async Task<ActionResult> UpdateForRequest(long requestId, [FromBody] List<UpdateItemDto> items)
        {
            if (requestId <= 0)
                return BadRequest("requestId must be greater than zero.");

            if (items is null)
                return BadRequest("Items payload is required.");

            var result = await _itemRepository.UpdateItemsAsync(requestId, items);
            if (!result)
            {
                _logger.LogWarning("Failed to update items for RequestID: {RequestID}", requestId);
                return StatusCode(500, "Failed to update items.");
            }

            return Ok();
        }
    }
}