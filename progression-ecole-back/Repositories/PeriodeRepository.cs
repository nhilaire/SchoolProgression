using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ProgressionEcole.Models;

namespace ProgressionEcole.Repositories
{
    public class PeriodeActivites
    {
        public string EleveId { get; set; } = "";
        public string Periode { get; set; } = "";
        public List<string> ActiviteIds { get; set; } = new();
    }

    public class PeriodeRepository
    {
        private readonly string _filePath;
        private List<PeriodeActivites> _periodes = new();

        public PeriodeRepository(IOptions<DataPathsConfig> config)
        {
            var dataConfig = config.Value;
            _filePath = Path.Combine(dataConfig.DataDirectory, dataConfig.PeriodesFile);
            Load();
        }

        public List<PeriodeActivites> GetAll()
        {
            lock (_periodes)
            {
            return new List<PeriodeActivites>(_periodes);
            }
        }

        public PeriodeActivites? GetByPeriode(string periode)
        {
            lock (_periodes)
            {
            return _periodes.FirstOrDefault(p => p.Periode == periode);
            }
        }

        public void Save(PeriodeActivites periodeActivites)
        {
            lock (_periodes)
            {
                var idx = _periodes.FindIndex(p => p.EleveId == periodeActivites.EleveId && p.Periode == periodeActivites.Periode);
                if (idx >= 0)
                {
                    _periodes[idx] = periodeActivites;
                }
                else
                {
                    _periodes.Add(periodeActivites);
                }
                SaveFile();
            }
        }

        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _periodes = JsonSerializer.Deserialize<List<PeriodeActivites>>(json) ?? new();
            }
        }

        private void SaveFile()
        {
            var json = JsonSerializer.Serialize(_periodes);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, json);
        }
    }
}
