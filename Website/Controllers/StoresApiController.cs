using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreLocator.Helpers;
using StoreLocator.Models;
using StoreLocator.Services;

namespace StoreLocator.Controllers
{
    [Route("api/stores")]
    [ApiController]
    [Authorize]
    public class StoresApiController : ControllerBase
    {
        private readonly DataServices _database;
        private readonly ILogger<StoresApiController> _logger;

        public StoresApiController(ILogger<StoresApiController> logger, DataServices database)
        {
            _logger = logger;
            _database = database;
        }

        // GET: api/stores
        [HttpGet()]
        public async Task<ActionResult<List<ListStoreModel>>> GetStoresAsync()
        {
            var stores = await _database.GetStoresAsync();

            return Ok(stores);
        }

        // GET: api/stores/features
        [HttpGet("features")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Feature>>> GetFeaturesAsync()
        {
            var tags = await _database.GetFeaturesAsync();

            return Ok(tags);
        }

        // GET api/stores/search?query={query}&limit={limit}&country={country}&tags={tag1,tag2,tag3}&latitude={latitude}&longitude={longitude}&rangeInKm={rangeInKm}
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<List<StoreWithDistance>>> GetStoresBySearchAsync(
            [FromQuery] string query,
            [FromQuery] int? limit,
            [FromQuery] string country,
            [FromQuery] string tags,
            [FromQuery] double? latitude,
            [FromQuery] double? longitude,
            [FromQuery] double? rangeInKm)
        {
            if (!string.IsNullOrEmpty(query))
            {
                if (query.Length < 3 || query.Length > 64)
                {
                    return BadRequest("Invalid input value for query parameter.");
                }
            }

            if (!string.IsNullOrEmpty(country))
            {
                if (country.Length != 2)
                {
                    return BadRequest("Invalid input value for country parameter.");
                }
            }

            if (longitude.HasValue && latitude.HasValue && rangeInKm.HasValue)
            {
                if (!GeospatialHelper.IsValidLatitude(latitude.Value) || !GeospatialHelper.IsValidLongitude(longitude.Value) || rangeInKm.Value <= 0 || rangeInKm.Value > 100)
                {
                    return BadRequest("Invalid latitude or longitude values or range is invalid.");
                }
            }

            var stores = await _database.GetStoresBySearchAsync(query, limit, country, tags, latitude, longitude, rangeInKm);

            return Ok(stores);
        }

        // GET api/stores/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoreByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 64)
            {
                return BadRequest("Invalid input value.");
            }

            var store = await _database.GetStoreByIdAsync(id);

            if (store != null)
            {
                return Ok(store);
            }

            return NotFound();
        }

        // DELETE: api/stores/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStoreAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 64)
            {
                return BadRequest("Invalid input value.");
            }

            var store = await _database.GetStoreByIdAsync(id);

            if (store == null)
            {
                return NotFound();
            }

            try
            {
                await _database.DeleteStoreAsync(store);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Create a new store
        // POST: api/stores/
        [HttpPost()]
        public async Task<ActionResult> CreateStoreAsync([FromBody] Store store)
        {
            try
            {
                await _database.CreateStoreAsync(store);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Update an existing store
        // PUT: api/stores/
        [HttpPut()]
        public async Task<ActionResult> UpdateStoreAsync([FromBody] Store store)
        {
            try
            {
                await _database.UpdateStoreAsync(store);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}