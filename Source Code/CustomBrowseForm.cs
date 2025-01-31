using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ConsoleIsoDownloader
{
    /// <summary>
    /// Classe permettant d'ouvrir des boîtes de dialogue (Open/Save) via Microsoft.WindowsAPICodePack.Dialogs.
    /// </summary>
    public static class CustomBrowseForm
    {
        public enum Mode
        {
            SelectFile,  // Parcourir pour ouvrir un fichier existant
            SaveFile     // Parcourir pour choisir où sauvegarder
        }

        /// <summary>
        /// Ouvre un CommonOpenFileDialog en STA, et renvoie le chemin sélectionné
        /// (ou null si l'utilisateur annule la sélection).
        /// </summary>
        /// <param name="mode">Mode sélection ou sauvegarde</param>
        /// <param name="defaultFileName">Nom de fichier par défaut en mode SaveFile</param>
        /// <returns>Le chemin sélectionné ou null</returns>
        public static async Task<string?> BrowseAsync(Mode mode, string defaultFileName = "")
        {
            var tcs = new TaskCompletionSource<string?>();

            // Lancer un thread STA pour afficher la boîte de dialogue
            Thread staThread = new Thread(() =>
            {
                try
                {
                    using (var dialog = new CommonOpenFileDialog())
                    {
                        dialog.IsFolderPicker = false;

                        if (mode == Mode.SaveFile)
                        {
                            // Configuration "save" : définir un nom et une extension par défaut
                            dialog.DefaultFileName = defaultFileName;
                            dialog.DefaultExtension = ".iso";
                            dialog.Filters.Add(new CommonFileDialogFilter("Fichiers ISO", "*.iso"));
                            dialog.EnsureFileExists = false; // En mode "save", l'utilisateur peut choisir un nom non-existant
                        }
                        else // mode == Mode.SelectFile
                        {
                            // Exemple de filtre pour un fichier "config.properties" 
                            // (ou autres types de fichiers selon vos besoins)
                            dialog.Filters.Add(new CommonFileDialogFilter("Fichiers properties", "*.properties"));
                            dialog.Filters.Add(new CommonFileDialogFilter("Tous les fichiers", "*.*"));
                            dialog.EnsureFileExists = true; // En mode "open", forcer la sélection d'un fichier existant
                        }

                        // Affichage du dialog
                        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            tcs.SetResult(dialog.FileName);
                        }
                        else
                        {
                            tcs.SetResult(null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();

            return await tcs.Task;
        }
    }
}
