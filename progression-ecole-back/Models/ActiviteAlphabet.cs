namespace ProgressionEcole.Models
{
    public class ActiviteAlphabet
    {
        public string Id { get; set; } = string.Empty;
        public string EleveId { get; set; } = string.Empty;
        public string Periode { get; set; } = string.Empty;
        // États des lettres: "inconnu", "en_cours", "acquis"
        public Dictionary<string, string> LettresMajuscules { get; set; } = new();
        public Dictionary<string, string> LettresMinuscules { get; set; } = new();
        public DateTime DateModification { get; set; } = DateTime.UtcNow;
    }
}
