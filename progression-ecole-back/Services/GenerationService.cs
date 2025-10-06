using System.Collections.Generic;
using System.Linq;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using ProgressionEcole.Repositories;
using ProgressionEcole.Models;

namespace ProgressionEcole.Services
{
    public class GenerationService
    {
        private readonly string _templatePath = "Data/modele_{0}.docx";
        private readonly EleveRepository _eleveRepo;
        private readonly ActiviteRepository _activiteRepo;
        private readonly PeriodeRepository _periodeRepo;

        public GenerationService(EleveRepository eleveRepo, ActiviteRepository activiteRepo, PeriodeRepository periodeRepo)
        {
            _eleveRepo = eleveRepo;
            _activiteRepo = activiteRepo;
            _periodeRepo = periodeRepo;
        }

        public byte[] GenerateDocx(string periode)
        {
            var eleves = _eleveRepo.GetAll().OrderBy(e => e.Nom).ThenBy(e => e.Prenom).ToList();

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
                            .Select(activite => FormatActiviteLibelle(activite!))
                            .Where(lib => !string.IsNullOrWhiteSpace(lib))
                            .OrderBy(lib => lib)
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
        /// Formate le libellé d'une activité, en incluant le contexte du regroupement si nécessaire
        /// </summary>
        private string FormatActiviteLibelle(Activite activite)
        {
            if (string.IsNullOrWhiteSpace(activite.ParentId))
            {
                // Activité isolée
                return activite.LibelleLong;
            }
            else
            {
                // Activité dans un regroupement - inclure le nom du regroupement
                var regroupement = _activiteRepo.GetById(activite.ParentId);
                if (regroupement != null)
                {
                    return $"{regroupement.LibelleLong} - {activite.LibelleLong}";
                }
                else
                {
                    return activite.LibelleLong;
                }
            }
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
