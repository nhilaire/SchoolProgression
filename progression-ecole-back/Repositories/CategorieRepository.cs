using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ProgressionEcole.Models;

namespace ProgressionEcole.Repositories
{
    public class CategorieRepository
    {
        private readonly string _filePath;
        private List<Categorie> _categories = new();

        public CategorieRepository(IOptions<DataPathsConfig> config)
        {
            var dataConfig = config.Value;
            _filePath = Path.Combine(dataConfig.DataDirectory, dataConfig.CategoriesFile);
            Load();
        }

        public List<Categorie> GetAll() => _categories;

        public Categorie? GetById(string id) => _categories.FirstOrDefault(c => c.Id == id);

        public void Add(Categorie categorie)
        {
            _categories.Add(categorie);
            Save();
        }

        public void Update(Categorie categorie)
        {
            var idx = _categories.FindIndex(c => c.Id == categorie.Id);
            if (idx >= 0)
            {
                _categories[idx] = categorie;
                Save();
            }
        }

        public void Delete(string id)
        {
            _categories.RemoveAll(c => c.Id == id);
            Save();
        }

        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _categories = JsonSerializer.Deserialize<List<Categorie>>(json) ?? new();
                
                // Assigner les couleurs par défaut selon les libellés
                bool needsSave = false;
                foreach (var categorie in _categories)
                {
                    if (string.IsNullOrEmpty(categorie.Couleur))
                    {
                        categorie.Couleur = GetCouleurByLibelle(categorie.Libelle);
                        needsSave = true;
                    }
                }
                
                // Sauvegarder si des couleurs ont été ajoutées
                if (needsSave)
                {
                    Save();
                }
            }
        }

        private string GetCouleurByLibelle(string libelle)
        {
            return libelle switch
            {
                "Activités préliminaires - autres" => "#D9A0FA", // RGB(217,160, 250)
                "Vie pratique - Motricité fine" => "#A863EB",     // RGB(168, 99, 235)
                "Activités sensorielles - Formes et grandeurs" => "#85A5FC", // RGB(133, 165, 252)
                "Explorer le monde" => "#00DAFF",                // RGB(0, 218, 255)
                "Langage : oral - phonologie" => "#99F8E0",       // RGB(153, 248, 224)
                "Langage : lecture / encodage, écriture" => "#83BA2B", // RGB(131, 186, 43)
                "Mathématiques - Numération" => "#DBDB15",        // RGB(219, 219, 21)
                _ => "#E0E0E0" // Couleur par défaut grise
            };
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_categories);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, json);
        }
    }
}
