namespace ProgressionEcole.Models
{
    public class ActiviteDigramme
    {
        public string Id { get; set; } = string.Empty;
        public string EleveId { get; set; } = string.Empty;
        public string Periode { get; set; } = string.Empty;
        // États des digrammes/trigrammes: "inconnu", "en_cours", "acquis"
        // Clés: ch, ou, oi, in, on, an, ai, un, ill, gn, oin
        public Dictionary<string, string> Digrammes { get; set; } = new();
        public DateTime DateModification { get; set; } = DateTime.UtcNow;
    }
}
