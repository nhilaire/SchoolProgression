using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Options;
using ProgressionEcole.Models;

namespace ProgressionEcole.Repositories
{
    public class DefinitionPeriodeRepository
    {
        private readonly string _filePath;
        private List<Periode> _periodes = new();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public DefinitionPeriodeRepository(IOptions<DataPathsConfig> config)
        {
            var dataConfig = config.Value;
            _filePath = Path.Combine(dataConfig.DataDirectory, dataConfig.DefinitionPeriodesFile);
            Load();
        }

        public List<Periode> GetAll()
        {
            lock (_periodes)
            {
                return new List<Periode>(_periodes).OrderBy(p => p.Nom).ToList();
            }
        }

        public Periode? GetById(string id)
        {
            lock (_periodes)
            {
                return _periodes.FirstOrDefault(p => p.Id == id);
            }
        }

        public Periode? GetByNom(string nom)
        {
            lock (_periodes)
            {
                return _periodes.FirstOrDefault(p => p.Nom == nom);
            }
        }

        public Periode? GetCurrentPeriode()
        {
            lock (_periodes)
            {
                var today = DateTime.Today;
                return _periodes.FirstOrDefault(p => p.DateDebut <= today && p.DateFin >= today);
            }
        }

        public void Update(Periode periode)
        {
            lock (_periodes)
            {
                var idx = _periodes.FindIndex(p => p.Id == periode.Id);
                if (idx >= 0)
                {
                    _periodes[idx] = periode;
                    Save();
                }
            }
        }

        public void Add(Periode periode)
        {
            lock (_periodes)
            {
                _periodes.Add(periode);
                Save();
            }
        }

        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _periodes = JsonSerializer.Deserialize<List<Periode>>(json, _jsonOptions) ?? new();
            }
            else
            {
                InitializeDefaultPeriodes();
            }
        }

        private void InitializeDefaultPeriodes()
        {
            _periodes = new List<Periode>
            {
                new() { Id = "p1", Nom = "P1", DateDebut = new DateTime(2024, 9, 2), DateFin = new DateTime(2024, 10, 18) },
                new() { Id = "p2", Nom = "P2", DateDebut = new DateTime(2024, 11, 4), DateFin = new DateTime(2024, 12, 20) },
                new() { Id = "p3", Nom = "P3", DateDebut = new DateTime(2025, 1, 6), DateFin = new DateTime(2025, 2, 14) },
                new() { Id = "p4", Nom = "P4", DateDebut = new DateTime(2025, 3, 3), DateFin = new DateTime(2025, 4, 11) },
                new() { Id = "p5", Nom = "P5", DateDebut = new DateTime(2025, 4, 28), DateFin = new DateTime(2025, 7, 4) }
            };
            Save();
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_periodes, _jsonOptions);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, json);
        }
    }
}
