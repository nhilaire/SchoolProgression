using System.Text.Json;
using Microsoft.Extensions.Options;
using ProgressionEcole.Models;

namespace ProgressionEcole.Repositories
{
    public class ActivitePersonnaliseeRepository
    {
        private readonly string _filePath;
        private List<ActivitePersonnalisee> _activitesPersonnalisees = new();
        private readonly object _lock = new object();

        public ActivitePersonnaliseeRepository(IOptions<DataPathsConfig> config)
        {
            var dataConfig = config.Value;
            _filePath = Path.Combine(dataConfig.DataDirectory, "activites-personnalisees.json");
            Load();
        }

        public List<ActivitePersonnalisee> GetAll()
        {
            lock (_lock)
            {
                return new List<ActivitePersonnalisee>(_activitesPersonnalisees);
            }
        }

        public List<ActivitePersonnalisee> GetByEleveAndPeriode(string eleveId, string periode)
        {
            lock (_lock)
            {
                return _activitesPersonnalisees
                    .Where(a => a.EleveId == eleveId && a.Periode == periode)
                    .ToList();
            }
        }

        public ActivitePersonnalisee? GetByActiviteAndEleve(string activiteId, string eleveId, string periode)
        {
            lock (_lock)
            {
                return _activitesPersonnalisees
                    .FirstOrDefault(a => a.ActiviteId == activiteId && a.EleveId == eleveId && a.Periode == periode);
            }
        }

        public void Save(ActivitePersonnalisee activite)
        {
            lock (_lock)
            {
                var existing = _activitesPersonnalisees
                    .FirstOrDefault(a => a.ActiviteId == activite.ActiviteId && 
                                        a.EleveId == activite.EleveId && 
                                        a.Periode == activite.Periode);

                if (existing != null)
                {
                    // Mise à jour
                    existing.ValeursParametres = activite.ValeursParametres;
                }
                else
                {
                    // Nouvelle entrée
                    activite.Id = Guid.NewGuid().ToString();
                    activite.DateCreation = DateTime.UtcNow;
                    _activitesPersonnalisees.Add(activite);
                }

                SaveToFile();
            }
        }

        public void Delete(string activiteId, string eleveId, string periode)
        {
            lock (_lock)
            {
                var toRemove = _activitesPersonnalisees
                    .Where(a => a.ActiviteId == activiteId && a.EleveId == eleveId && a.Periode == periode)
                    .ToList();

                foreach (var item in toRemove)
                {
                    _activitesPersonnalisees.Remove(item);
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
                    _activitesPersonnalisees = JsonSerializer.Deserialize<List<ActivitePersonnalisee>>(json) ?? new List<ActivitePersonnalisee>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des activités personnalisées: {ex.Message}");
                _activitesPersonnalisees = new List<ActivitePersonnalisee>();
            }
        }

        private void SaveToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_activitesPersonnalisees, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde des activités personnalisées: {ex.Message}");
            }
        }
    }
}