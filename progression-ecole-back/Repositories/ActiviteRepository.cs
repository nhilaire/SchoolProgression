
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProgressionEcole.Models;

namespace ProgressionEcole.Repositories
{
    public class ActiviteRepository
    {
        private readonly string _filePath = "Data/activites.json";
        private List<Activite> _activites = new();

        public ActiviteRepository()
        {
            Load();
        }

        public List<Activite> GetAll()
        {
            lock (_activites)
            {
                return new List<Activite>(_activites);
            }
        }

        public Activite? GetById(string id)
        {
            lock (_activites)
            {
                return _activites.FirstOrDefault(a => a.Id == id);
            }
        }

        public void Add(Activite activite)
        {
            lock (_activites)
            {
                _activites.Add(activite);
                Save();
            }
        }

        public void Update(Activite activite)
        {
            lock (_activites)
            {
                var idx = _activites.FindIndex(a => a.Id == activite.Id);
                if (idx >= 0)
                {
                    _activites[idx] = activite;
                    Save();
                }
            }
        }

        public void Delete(string id)
        {
            lock (_activites)
            {
                _activites.RemoveAll(a => a.Id == id);
                Save();
            }
        }

        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _activites = JsonSerializer.Deserialize<List<Activite>>(json) ?? new();
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_activites);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, json);
        }
    }
}
