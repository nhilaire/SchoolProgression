using System.Collections.Generic;
using System.Linq;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProgressionEcole.Repositories;
using ProgressionEcole.Models;

namespace ProgressionEcole.Services
{
    public class GenerationService
    {
        private readonly string _templatePath;
        private readonly EleveRepository _eleveRepo;
        private readonly ActiviteRepository _activiteRepo;
        private readonly PeriodeRepository _periodeRepo;
        private readonly ActivitePersonnaliseeRepository _activitePersonnaliseeRepo;

        public GenerationService(EleveRepository eleveRepo, ActiviteRepository activiteRepo, PeriodeRepository periodeRepo, ActivitePersonnaliseeRepository activitePersonnaliseeRepo, IOptions<DataPathsConfig> config)
        {
            _eleveRepo = eleveRepo;
            _activiteRepo = activiteRepo;
            _periodeRepo = periodeRepo;
            _activitePersonnaliseeRepo = activitePersonnaliseeRepo;
            var dataConfig = config.Value;
            _templatePath = Path.Combine(dataConfig.DataDirectory, "modele_{0}.docx");
        }

        public byte[] GenerateDocx(string periode)
        {
            // Utiliser l'ordre défini dans le fichier eleves.json
            var eleves = _eleveRepo.GetAll().ToList();

            var tempFile = Path.GetTempFileName() + ".docx";
            var finalPath  = string.Format(_templatePath, periode);
            File.Copy(finalPath, tempFile, true);

            try
            {
                using (var doc = WordprocessingDocument.Open(tempFile, true))
                {
                    var body = doc.MainDocumentPart?.Document?.Body;
                    if (body == null) 
                    {
                        var emptyContent = File.ReadAllBytes(tempFile);
                        File.Delete(tempFile);
                        return emptyContent;
                    }

                    for (int i = 0; i < eleves.Count; i++)
                    {
                        var numeroEleve = (i + 1).ToString("D2");
                        var eleve = eleves[i];

                        var activitesIds = _periodeRepo.GetAll().FirstOrDefault(x => x.Periode == periode && x.EleveId == eleve.Id)?.ActiviteIds;
                        var activiteLibelles = activitesIds?
                            .Select(id => _activiteRepo.GetById(id))
                            .Where(activite => activite != null && !activite.EstRegroupement) // Filtrer seulement les activités réelles
                            .OrderBy(activite => activite!.Ordre) // Trier par l'ordre défini dans le JSON
                            .Select(activite => FormatActiviteLibelle(activite!, eleve.Id, periode))
                            .Where(lib => !string.IsNullOrWhiteSpace(lib))
                            .ToList() ?? new List<string>();

                        var prenomParaOrig = body.Descendants<Paragraph>().FirstOrDefault(p => p.InnerText.Contains($"PRENOM{numeroEleve}"));
                        var prenomRunProps = prenomParaOrig?.Descendants<RunProperties>().FirstOrDefault();

                        ReplaceText(body, $"PRENOM{numeroEleve}", eleve.Prenom);
                        ReplaceList(body, $"LISTE{numeroEleve}", activiteLibelles, prenomRunProps);
                    }

                    doc.MainDocumentPart?.Document?.Save();
                }

                var content = File.ReadAllBytes(tempFile);
                return content;
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Formate le libellé d'une activité, en gérant les activités paramétrables
        /// </summary>
        private string FormatActiviteLibelle(Activite activite, string eleveId, string periode)
        {
            if (!activite.EstParametrable)
            {
                // Activité normale
                return activite.LibelleLong;
            }
            
            // Activité paramétrable - chercher les valeurs personnalisées
            var personnalisee = _activitePersonnaliseeRepo.GetByActiviteAndEleve(activite.Id, eleveId, periode);
            
            if (personnalisee != null && personnalisee.ValeursParametres.Any())
            {
                // Utiliser le LibelleLong comme template et remplacer les paramètres
                return RemplaceParametres(activite.LibelleLong, personnalisee.ValeursParametres);
            }
            
            // Fallback sur le libellé long si pas de personnalisation
            return activite.LibelleLong;
        }

        /// <summary>
        /// Remplace les placeholders {param} par les valeurs saisies
        /// </summary>
        private string RemplaceParametres(string template, Dictionary<string, string> valeurs)
        {
            var result = template;
            foreach (var param in valeurs)
            {
                var placeholder = $"{{{param.Key}}}";
                result = result.Replace(placeholder, param.Value);
            }
            return result;
        }

        private void ReplaceText(Body body, string placeholder, string value)
        {
            foreach (var text in body.Descendants<Text>())
            {
                if (text.Text.Contains(placeholder))
                    text.Text = text.Text.Replace(placeholder, value);
            }
        }

        private void ReplaceList(Body body, string placeholder, List<string> items, RunProperties? prenomRunProps)
        {
            var para = body.Descendants<Paragraph>().FirstOrDefault(p => p.InnerText.Contains(placeholder));
            if (para != null && para.Parent != null)
            {
                var parent = para.Parent;
                var previous = para;
                foreach (var item in items)
                {
                    var run = new Run(new Text(item));
                    if (prenomRunProps != null)
                    {
                        run.RunProperties = (RunProperties)prenomRunProps.CloneNode(true);
                    }
                    var bulletPara = new Paragraph(run);
                    bulletPara.ParagraphProperties = new ParagraphProperties(
                        new NumberingProperties(new NumberingLevelReference() { Val = 0 }, new NumberingId() { Val = 1 }),
                        new SpacingBetweenLines { Before = "0", After = "0" }
                    );
                    parent.InsertAfter(bulletPara, previous);
                    previous = bulletPara;
                }
                parent.RemoveChild(para);
            }
        }
    }
}
