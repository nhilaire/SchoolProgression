using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProgressionEcole.Models;

namespace ProgressionEcole.Repositories
{
    public class CategorieRepository
    {
        private readonly string _filePath = "Data/categories.json";
        private List<Categorie> _categories = new();

        public CategorieRepository()
        {
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
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_categories);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, json);
        }
    }
}
