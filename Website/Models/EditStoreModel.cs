namespace StoreLocator.Models
{
    public class EditStoreModel
    {
        public readonly Store Store;
        public readonly List<Feature> Features;
        public readonly List<Country> Countries;
        public readonly string AzureMapsClientId;
        public readonly string AzureMapsTokenUrl;

        public EditStoreModel(Store store, List<Feature> features, List<Country> countries, string azureMapsClientId, string zzureMapsTokenUrl)
        {
            Store = store;
            Features = features;
            Countries = countries;
            AzureMapsClientId = azureMapsClientId;
            AzureMapsTokenUrl = zzureMapsTokenUrl;
        }
    }
}
