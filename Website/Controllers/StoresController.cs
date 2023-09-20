using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreLocator.Services;

namespace StoreLocator.Controllers
{
    [AllowAnonymous]
    public class StoresController : Controller
    {
        private readonly DataServices _database;
        private readonly ILogger<StoresController> _logger;

        public StoresController(ILogger<StoresController> logger, DataServices database)
        {
            _logger = logger;
            _database = database;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(string id)
        {
            // Validate input values
            if (string.IsNullOrWhiteSpace(id) || id.Length > 63)
            {
                return BadRequest("Invalid input value.");
            }

            var store = await _database.GetStoreByIdAsync(id);

            if (store != null)
            {
                // If a store with the specified ID is found, return it as Ok (200) response
                return View(store);
            }

            // If no store is found with the specified ID, return NotFound (404) response
            return NotFound();
        }
    }
}