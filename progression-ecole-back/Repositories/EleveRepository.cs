using ProgressionEcole.Models;
using System.Text.Json;

namespace ProgressionEcole.Repositories
{
    public class EleveRepository
    {
        private readonly string _filePath = "Data/eleves.json";
        private List<Eleve> _eleves = new();

        public EleveRepository()
        {
            Load();
        }

        public List<Eleve> GetAll() => _eleves;
        public Eleve? GetById(string id) => _eleves.FirstOrDefault(e => e.Id == id);
        public void Add(Eleve eleve)
        {
            _eleves.Add(eleve);
            Save();
        }
        public void Update(Eleve eleve)
        {
            var idx = _eleves.FindIndex(e => e.Id == eleve.Id);
            if (idx >= 0)
            {
                _eleves[idx] = eleve;
                Save();
            }
        }
        public void Delete(string id)
        {
            _eleves.RemoveAll(e => e.Id == id);
            Save();
        }
        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _eleves = JsonSerializer.Deserialize<List<Eleve>>(json) ?? new();
            }
        }
        private void Save()
        {
            var json = JsonSerializer.Serialize(_eleves);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, json);
        }
    }
}
