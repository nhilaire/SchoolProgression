namespace ProgressionEcole.Models
{
    public class Periode
    {
        public string Id { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty; // P1, P2, P3, P4, P5
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
    }
}
