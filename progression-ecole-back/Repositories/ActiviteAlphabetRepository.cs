using System.Text.Json;
using Microsoft.Extensions.Options;
using ProgressionEcole.Models;

namespace ProgressionEcole.Repositories
{
    public class ActiviteAlphabetRepository
    {
        private readonly string _filePath;
        private List<ActiviteAlphabet> _activitesAlphabet = new();
        private readonly object _lock = new object();

        public ActiviteAlphabetRepository(IOptions<DataPathsConfig> config)
        {
            var dataConfig = config.Value;
            _filePath = Path.Combine(dataConfig.DataDirectory, "activites-alphabet.json");
            Load();
        }

        public List<ActiviteAlphabet> GetAll()
        {
            lock (_lock)
            {
                return new List<ActiviteAlphabet>(_activitesAlphabet);
            }
        }

        public ActiviteAlphabet? GetByEleveAndPeriode(string eleveId, string periode)
        {
            lock (_lock)
            {
                return _activitesAlphabet
                    .FirstOrDefault(a => a.EleveId == eleveId && a.Periode == periode);
            }
        }

        public List<ActiviteAlphabet> GetByEleve(string eleveId)
        {
            lock (_lock)
            {
                return _activitesAlphabet
                    .Where(a => a.EleveId == eleveId)
                    .ToList();
            }
        }

        public void Save(ActiviteAlphabet activite)
        {
            lock (_lock)
            {
                var existing = _activitesAlphabet
                    .FirstOrDefault(a => a.EleveId == activite.EleveId && a.Periode == activite.Periode);

                if (existing != null)
                {
                    // Mise à jour
                    existing.LettresMajuscules = activite.LettresMajuscules;
                    existing.LettresMinuscules = activite.LettresMinuscules;
                    existing.DateModification = DateTime.UtcNow;
                }
                else
                {
                    // Nouvelle entrée
                    activite.Id = Guid.NewGuid().ToString();
                    activite.DateModification = DateTime.UtcNow;
                    _activitesAlphabet.Add(activite);
                }

                SaveToFile();
            }
        }

        public void Delete(string eleveId, string periode)
        {
            lock (_lock)
            {
                var toRemove = _activitesAlphabet
                    .Where(a => a.EleveId == eleveId && a.Periode == periode)
                    .ToList();

                foreach (var item in toRemove)
                {
                    _activitesAlphabet.Remove(item);
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
                    _activitesAlphabet = JsonSerializer.Deserialize<List<ActiviteAlphabet>>(json) ?? new List<ActiviteAlphabet>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des activités alphabet: {ex.Message}");
                _activitesAlphabet = new List<ActiviteAlphabet>();
            }
        }

        private void SaveToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_activitesAlphabet, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde des activités alphabet: {ex.Message}");
            }
        }
    }
}
