using System.Text.Json;
using Microsoft.Extensions.Options;
using ProgressionEcole.Models;

namespace ProgressionEcole.Repositories
{
    public class ActiviteChiffreRepository
    {
        private readonly string _filePath;
        private List<ActiviteChiffre> _activitesChiffre = new();
        private readonly object _lock = new object();

        public ActiviteChiffreRepository(IOptions<DataPathsConfig> config)
        {
            var dataConfig = config.Value;
            _filePath = Path.Combine(dataConfig.DataDirectory, "activites-chiffre.json");
            Load();
        }

        public List<ActiviteChiffre> GetAll()
        {
            lock (_lock)
            {
                return new List<ActiviteChiffre>(_activitesChiffre);
            }
        }

        public ActiviteChiffre? GetByEleveAndPeriode(string eleveId, string periode)
        {
            lock (_lock)
            {
                return _activitesChiffre
                    .FirstOrDefault(a => a.EleveId == eleveId && a.Periode == periode);
            }
        }

        public List<ActiviteChiffre> GetByEleve(string eleveId)
        {
            lock (_lock)
            {
                return _activitesChiffre
                    .Where(a => a.EleveId == eleveId)
                    .ToList();
            }
        }

        public void Save(ActiviteChiffre activite)
        {
            lock (_lock)
            {
                var existing = _activitesChiffre
                    .FirstOrDefault(a => a.EleveId == activite.EleveId && a.Periode == activite.Periode);

                if (existing != null)
                {
                    existing.Chiffres = activite.Chiffres;
                    existing.DateModification = DateTime.UtcNow;
                }
                else
                {
                    activite.Id = Guid.NewGuid().ToString();
                    activite.DateModification = DateTime.UtcNow;
                    _activitesChiffre.Add(activite);
                }

                SaveToFile();
            }
        }

        public void Delete(string eleveId, string periode)
        {
            lock (_lock)
            {
                var toRemove = _activitesChiffre
                    .Where(a => a.EleveId == eleveId && a.Periode == periode)
                    .ToList();

                foreach (var item in toRemove)
                {
                    _activitesChiffre.Remove(item);
                }

                SaveToFile();
            }
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    _activitesChiffre = JsonSerializer.Deserialize<List<ActiviteChiffre>>(json) ?? new List<ActiviteChiffre>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des activités chiffre: {ex.Message}");
                _activitesChiffre = new List<ActiviteChiffre>();
            }
        }

        private void SaveToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_activitesChiffre, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde des activités chiffre: {ex.Message}");
            }
        }
    }
}
