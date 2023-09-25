using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using StoreLocator.Models;
using StoreLocator.Services;

namespace StoreLocator.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly DataServices _database;
        private readonly ILogger<AdminController> _logger;
        private readonly string _azureMapsClientId;
        private readonly string _azureMapsTokenUrl;

        public AdminController(ILogger<AdminController> logger, DataServices database, IConfiguration configuration)
        {
            _logger = logger;
            _database = database;
            _azureMapsClientId = configuration["AzureMaps:ClientId"];
            _azureMapsTokenUrl = configuration["AzureMaps:TokenUrl"];
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
                ViewBag.AzureMapsClientId = _azureMapsClientId;
                ViewBag.AzureMapsTokenUrl = _azureMapsTokenUrl;

                // If a store with the specified ID is found, return it as Ok (200) response
                return View(store);
            }

            // If no store is found with the specified ID, return NotFound (404) response
            return NotFound();
        }
    }
}
