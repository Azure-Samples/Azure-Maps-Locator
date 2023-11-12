using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreLocator.Services;

namespace StoreLocator.Controllers;

[AllowAnonymous]
public class StoresController(ILogger<StoresController> logger, DataServices database, IConfiguration configuration) : Controller
{
    private readonly DataServices _database = database;
    private readonly ILogger<StoresController> _logger = logger;
    private readonly string _azureMapsClientId = configuration["AzureMaps:ClientId"];
    private readonly string _azureMapsTokenUrl = configuration["AzureMaps:TokenUrl"];

    public IActionResult Index()
    {
        ViewBag.AzureMapsClientId = _azureMapsClientId;
        ViewBag.AzureMapsTokenUrl = _azureMapsTokenUrl;

        return View();
    }

    public async Task<IActionResult> Details(string id)
    {
        // Validate input values
        if (string.IsNullOrWhiteSpace(id) || id.Length > 64)
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