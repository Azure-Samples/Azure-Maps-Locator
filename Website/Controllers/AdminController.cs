using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreLocator.Models;
using StoreLocator.Services;

namespace StoreLocator.Controllers;

[Authorize]
public class AdminController(ILogger<AdminController> logger, DataServices database, IConfiguration configuration) : Controller
{
    private readonly DataServices _database = database;
    private readonly ILogger<AdminController> _logger = logger;
    private readonly string _azureMapsClientId = configuration["AzureMaps:ClientId"];
    private readonly string _azureMapsTokenUrl = configuration["AzureMaps:TokenUrl"];

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> New()
    {
        var model = new EditStoreModel
        {
            IsNew = true,
            Store = new Store(),
            Features = await _database.GetFeaturesAsync(),
            Countries = await _database.GetCountriesAsync(),
            AzureMapsClientId = _azureMapsClientId,
            AzureMapsTokenUrl = _azureMapsTokenUrl
        };

        model.Store.Location.Coordinates.Add(0.0);
        model.Store.Location.Coordinates.Add(0.0);

        // return View with store and features
        return View("Edit", model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        // Validate input values
        if (string.IsNullOrWhiteSpace(id) || id.Length > 64)
        {
            return BadRequest("Invalid input value.");
        }

        var store = await _database.GetStoreByIdAsync(id);

        if (store != null)
        {
            var model = new EditStoreModel
            {
                IsNew = false,
                Store = store,
                Features = await _database.GetFeaturesAsync(),
                Countries = await _database.GetCountriesAsync(),
                AzureMapsClientId = _azureMapsClientId,
                AzureMapsTokenUrl = _azureMapsTokenUrl
            };

            // return View with store and features
            return View(model);
        }

        // If no store is found with the specified ID, return NotFound (404) response
        return NotFound();
    }
}
