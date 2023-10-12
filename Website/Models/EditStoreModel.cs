namespace StoreLocator.Models
{
    public class EditStoreModel
    {
        public bool IsNew { get; set; }
        public Store Store { get; set; }
        public List<Feature> Features { get; set; }
        public List<Country> Countries { get; set; }
        public string AzureMapsClientId { get; set; }
        public string AzureMapsTokenUrl { get; set; }
    }
}
