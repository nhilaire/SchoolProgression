namespace ProgressionEcole.Models
{
    public class Categorie
    {
        public string? Id { get; set; }
        public string Libelle { get; set; }
        public string Couleur { get; set; } = "#e0e0e0"; // Couleur par défaut en hexa
    }
}
