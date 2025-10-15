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
        private readonly CategorieRepository _categorieRepo;

        public GenerationService(EleveRepository eleveRepo, ActiviteRepository activiteRepo, PeriodeRepository periodeRepo, ActivitePersonnaliseeRepository activitePersonnaliseeRepo, CategorieRepository categorieRepo, IOptions<DataPathsConfig> config)
        {
            _eleveRepo = eleveRepo;
            _activiteRepo = activiteRepo;
            _periodeRepo = periodeRepo;
            _activitePersonnaliseeRepo = activitePersonnaliseeRepo;
            _categorieRepo = categorieRepo;
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
                        
                        List<string> activiteLibelles;
                        if (activitesIds == null || !activitesIds.Any())
                        {
                            activiteLibelles = new List<string>();
                        }
                        else
                        {
                            // Récupérer toutes les activités sélectionnées
                            var activitesSelectionnees = activitesIds
                                .Select(id => _activiteRepo.GetById(id))
                                .Where(activite => activite != null)
                                .ToList();

                            // Séparer les activités selon leur nature
                            var activitesSansParent = activitesSelectionnees
                                .Where(a => string.IsNullOrEmpty(a!.ParentId))
                                .OrderBy(a => GetCategorieOrdre(a!.CategorieId))
                                .ThenBy(a => a!.Ordre)
                                .ToList();

                            var activitesAvecParent = activitesSelectionnees
                                .Where(a => !string.IsNullOrEmpty(a!.ParentId))
                                .ToList();

                            // Construire la liste finale en respectant l'ordre et les regroupements
                            var activitesFinales = new List<Activite>();
                            
                            // Créer une liste de toutes les activités/regroupements ordonnés (sans parentId)
                            // puis insérer les enfants à la place de leur regroupement parent
                            var tousLesElements = _activiteRepo.GetAll()
                                .Where(a => string.IsNullOrEmpty(a.ParentId))
                                .OrderBy(a => GetCategorieOrdre(a.CategorieId))
                                .ThenBy(a => a.Ordre)
                                .ToList();

                            foreach (var element in tousLesElements)
                            {
                                if (element.EstRegroupement)
                                {
                                    // C'est un regroupement : insérer ses activités enfants sélectionnées à cet endroit
                                    var enfantsSelectionnes = activitesAvecParent
                                        .Where(a => a!.ParentId == element.Id)
                                        .OrderBy(a => a!.Ordre)
                                        .ToList();
                                    
                                    activitesFinales.AddRange(enfantsSelectionnes!);
                                }
                                else
                                {
                                    // C'est une activité isolée : l'ajouter si elle est sélectionnée
                                    if (activitesSansParent.Any(a => a!.Id == element.Id))
                                    {
                                        activitesFinales.Add(element);
                                    }
                                }
                            }

                            // Formater les libellés
                            activiteLibelles = activitesFinales
                                .Select(activite => FormatActiviteLibelle(activite!, eleve.Id, periode))
                                .Where(lib => !string.IsNullOrWhiteSpace(lib))
                                .ToList();
                        }

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
        /// Obtient l'ordre d'affichage d'une catégorie selon l'ordre défini dans le système
        /// </summary>
        private int GetCategorieOrdre(string categorieId)
        {
            // Ordre des catégories tel qu'affiché dans l'interface
            var ordreCategories = new Dictionary<string, int>
            {
                {"dd55b2a1-fe43-4333-9729-02c260449322", 1}, // Activités préliminaires
                {"21a34dd7-1ce7-490d-857a-a559c7e1bbdf", 2}, // Vie pratique
                {"76d09cd4-b8b0-44ef-97c7-b9a31c23bfb9", 3}, // Activités sensorielles
                {"f9a142f2-5c33-4a45-b0ff-7bedb2c5c0dd", 4}, // Explorer le monde
                {"6712d24e-078d-4388-95e1-55b0162949ae", 5}, // Langage oral
                {"599f9483-777b-4262-8d2e-2281fe64088a", 6}, // Langage lecture/écriture
                {"8b56d5f2-4f1c-457f-bcfb-15248ca27198", 7}  // Mathématiques
            };
            
            return ordreCategories.TryGetValue(categorieId, out var ordre) ? ordre : 999;
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
