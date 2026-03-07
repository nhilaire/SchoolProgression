namespace ProgressionEcole.Services
{
    public static class FileBackupHelper
    {
        private const int MaxBackups = 3;

        /// <summary>
        /// Crée une copie de sauvegarde du fichier avant écriture.
        /// Garde les 3 sauvegardes les plus récentes et supprime les plus anciennes.
        /// Format du nom : {nom}_yyyyMMddHHmmss.json
        /// </summary>
        public static void CreateBackup(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var directory = Path.GetDirectoryName(filePath)!;
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var backupFileName = $"{fileNameWithoutExt}_{timestamp}{extension}";
            var backupPath = Path.Combine(directory, backupFileName);

            File.Copy(filePath, backupPath, overwrite: true);

            // Supprimer les sauvegardes les plus anciennes au-delà de MaxBackups
            var backupPattern = $"{fileNameWithoutExt}_*{extension}";
            var existingBackups = Directory.GetFiles(directory, backupPattern)
                .Where(f => f != filePath)
                .OrderByDescending(f => f)
                .ToList();

            foreach (var oldBackup in existingBackups.Skip(MaxBackups))
            {
                File.Delete(oldBackup);
            }
        }
    }
}
