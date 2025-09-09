namespace ProgressionEcole.Models
{
    public class Activite
    {
        public string Id { get; set; }
        public string LibelleCourt { get; set; } = string.Empty;
        public string LibelleLong { get; set; } = string.Empty;
        public string CategorieId { get; set; }
        public int Ordre { get; set; }
    }
}
