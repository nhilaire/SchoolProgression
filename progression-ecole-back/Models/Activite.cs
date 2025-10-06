namespace ProgressionEcole.Models
{
    public class Activite
    {
        public string Id { get; set; }
        public string LibelleCourt { get; set; } = string.Empty;
        public string LibelleLong { get; set; } = string.Empty;
        public string CategorieId { get; set; }
        public int Ordre { get; set; }
        
        // Propriétés pour le regroupement
        public bool EstRegroupement { get; set; } = false;
        public string? ParentId { get; set; } = null; // ID du regroupement parent (null si activité isolée ou regroupement)
    }
}
