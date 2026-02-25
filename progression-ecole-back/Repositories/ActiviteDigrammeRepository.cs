using System.Text.Json;
using Microsoft.Extensions.Options;
using ProgressionEcole.Models;

namespace ProgressionEcole.Repositories
{
    public class ActiviteDigrammeRepository
    {
        private readonly string _filePath;
        private List<ActiviteDigramme> _activitesDigramme = new();
        private readonly object _lock = new object();

        public ActiviteDigrammeRepository(IOptions<DataPathsConfig> config)
        {
            var dataConfig = config.Value;
            _filePath = Path.Combine(dataConfig.DataDirectory, "activites-digramme.json");
            Load();
        }

        public List<ActiviteDigramme> GetAll()
        {
            lock (_lock)
            {
                return new List<ActiviteDigramme>(_activitesDigramme);
            }
        }

        public ActiviteDigramme? GetByEleveAndPeriode(string eleveId, string periode)
        {
            lock (_lock)
            {
                return _activitesDigramme
                    .FirstOrDefault(a => a.EleveId == eleveId && a.Periode == periode);
            }
        }

        public List<ActiviteDigramme> GetByEleve(string eleveId)
        {
            lock (_lock)
            {
                return _activitesDigramme
                    .Where(a => a.EleveId == eleveId)
                    .ToList();
            }
        }

        public void Save(ActiviteDigramme activite)
        {
            lock (_lock)
            {
                var existing = _activitesDigramme
                    .FirstOrDefault(a => a.EleveId == activite.EleveId && a.Periode == activite.Periode);

                if (existing != null)
                {
                    existing.Digrammes = activite.Digrammes;
                    existing.DateModification = DateTime.UtcNow;
                }
                else
                {
                    activite.Id = Guid.NewGuid().ToString();
                    activite.DateModification = DateTime.UtcNow;
                    _activitesDigramme.Add(activite);
                }

                SaveToFile();
            }
        }

        public void Delete(string eleveId, string periode)
        {
            lock (_lock)
            {
                var toRemove = _activitesDigramme
                    .Where(a => a.EleveId == eleveId && a.Periode == periode)
                    .ToList();

                foreach (var item in toRemove)
                {
                    _activitesDigramme.Remove(item);
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
                    _activitesDigramme = JsonSerializer.Deserialize<List<ActiviteDigramme>>(json) ?? new List<ActiviteDigramme>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des activités digramme: {ex.Message}");
                _activitesDigramme = new List<ActiviteDigramme>();
            }
        }

        private void SaveToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_activitesDigramme, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde des activités digramme: {ex.Message}");
            }
        }
    }
}
