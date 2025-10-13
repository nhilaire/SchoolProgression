namespace ProgressionEcole.Models
{
    public class DataPathsConfig
    {
        public string DataDirectory { get; set; } = "Data";
        public string ElevesFile { get; set; } = "eleves.json";
        public string CategoriesFile { get; set; } = "categories.json";
        public string ActivitesFile { get; set; } = "activites.json";
        public string PeriodesFile { get; set; } = "periodes.json";
    }
}