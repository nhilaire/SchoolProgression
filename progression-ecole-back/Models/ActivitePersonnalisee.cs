namespace ProgressionEcole.Models
{
    public class ActivitePersonnalisee
    {
        public string Id { get; set; } = string.Empty;
        public string EleveId { get; set; } = string.Empty;
        public string ActiviteId { get; set; } = string.Empty; // ID de l'activité template
        public string Periode { get; set; } = string.Empty; // Période (P1, P2, etc.)
        public Dictionary<string, string> ValeursParametres { get; set; } = new(); // Valeurs saisies (ex: {"0": "10"})
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    }
}