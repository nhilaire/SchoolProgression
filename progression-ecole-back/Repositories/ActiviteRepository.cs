
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Options;
using ProgressionEcole.Models;

namespace ProgressionEcole.Repositories
{
    public class ActiviteRepository
    {
        private readonly string _filePath;
        private List<Activite> _activites = new();

        public ActiviteRepository(IOptions<DataPathsConfig> config)
        {
            var dataConfig = config.Value;
            _filePath = Path.Combine(dataConfig.DataDirectory, dataConfig.ActivitesFile);
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
                // Si on supprime un regroupement, supprimer aussi ses enfants
                var activite = _activites.FirstOrDefault(a => a.Id == id);
                if (activite?.EstRegroupement == true)
                {
                    _activites.RemoveAll(a => a.ParentId == id);
                }
                
                _activites.RemoveAll(a => a.Id == id);
                Save();
            }
        }

        public List<Activite> GetEnfants(string parentId)
        {
            lock (_activites)
            {
                return _activites.Where(a => a.ParentId == parentId).OrderBy(a => a.Ordre).ToList();
            }
        }

        public List<Activite> GetRegroupements()
        {
            lock (_activites)
            {
                return _activites.Where(a => a.EstRegroupement).OrderBy(a => a.Ordre).ToList();
            }
        }

        public List<Activite> GetActivitesIsolees()
        {
            lock (_activites)
            {
                return _activites.Where(a => !a.EstRegroupement && a.ParentId == null).OrderBy(a => a.Ordre).ToList();
            }
        }

        public void ReorganiserOrdre(List<string> ids)
        {
            lock (_activites)
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    var activite = _activites.FirstOrDefault(a => a.Id == ids[i]);
                    if (activite != null)
                    {
                        activite.Ordre = i + 1;
                    }
                }
                Save();
            }
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _activites = JsonSerializer.Deserialize<List<Activite>>(json, _jsonOptions) ?? new();
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
