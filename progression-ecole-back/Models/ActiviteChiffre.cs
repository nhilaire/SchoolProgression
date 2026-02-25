namespace ProgressionEcole.Models
{
    public class ActiviteChiffre
    {
        public string Id { get; set; } = string.Empty;
        public string EleveId { get; set; } = string.Empty;
        public string Periode { get; set; } = string.Empty;
        // États des chiffres 1-9: "inconnu", "en_cours", "acquis"
        public Dictionary<string, string> Chiffres { get; set; } = new();
        public DateTime DateModification { get; set; } = DateTime.UtcNow;
    }
}
