using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Locator.Models;
using Locator.Services;

namespace Locator.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly DataServices _database;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger, DataServices database)
        {
            _logger = logger;
            _database = database;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Edit(string id)
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
