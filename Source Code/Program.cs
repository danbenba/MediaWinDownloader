using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms; // Nécessaire pour MessageBox

namespace ConsoleIsoDownloader
{
    internal static class Program
    {
        // Version actuelle de l'application
        private const string CurrentVersion = "1.2";

        // URL du fichier contenant la version distante
        private const string VersionFileUrl = "https://raw.githubusercontent.com/danbenba/MediaWinDownloader/refs/heads/project/app.version";

        // URL de la page de mise à jour (modifier si nécessaire)
        private const string UpdatePageUrl = "https://github.com/danbenba/MediaWinDownloader";

        private static readonly HttpClient _httpClient = new HttpClient();
        private static Dictionary<string, List<VersionInfo>> _allWindowsData;
        private static CancellationTokenSource _cancellationTokenSource;

        // Indique si l'application est en mode "outdated" (update disponible mais non acceptée)
        private static bool _isOutdated = false;

        // URL JSON par défaut
        private const string DefaultJsonUrl = "https://raw.githubusercontent.com/danbenba/MediaWinDownloader/refs/heads/project/images.json";

        [STAThread]
        static async Task Main(string[] args)
        {
            // Vérification de la version au démarrage.
            // Si l'utilisateur accepte la mise à jour, la page s'ouvre et l'application se termine.
            bool updateAccepted = await CheckForUpdatesAsync();
            if (updateAccepted)
            {
                // Lancement de la page de mise à jour dans le navigateur par défaut puis sortie du programme
                Process.Start(new ProcessStartInfo
                {
                    FileName = UpdatePageUrl,
                    UseShellExecute = true
                });
                return;
            }

            // Définir le titre de la console en fonction du mode (outdated ou non)
            Console.Title = _isOutdated 
                ? "Media Windows Downloader [OUTDATED]" 
                : "Media Windows Downloader";
            if (_isOutdated)
                {
                    Logger.OUTDATEDStartupLog("");
                }
                else
                {
                    Logger.StartupLog("");
                }

            // Détermine l'URL du JSON à utiliser
            string jsonUrl = DefaultJsonUrl;
            if (args.Length > 0)
            {
                string argUrl = args[0];
                if (Uri.IsWellFormedUriString(argUrl, UriKind.Absolute))
                {
                    jsonUrl = argUrl;
                    Logger.LogInfo($"Custom JSON URL provided via arguments: {jsonUrl}");
                }
                else
                {
                    Logger.LogWarning($"Invalid JSON URL provided: {argUrl}. Using default URL instead.");
                }
            }
            else
            {
                Logger.LogInfo($"Using default JSON URL: {DefaultJsonUrl}");
            }

            // Vérifie l'existence d'un JSON local, sinon le télécharge
            string localJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wim.img.json");
            if (!File.Exists(localJsonPath))
            {
                Logger.LogInfo($"Local JSON file does not exist. Downloading from: {jsonUrl}");
            }

            if (!await LoadJsonDataAsync(jsonUrl, localJsonPath))
            {
                // En cas d'échec de chargement, on arrête
                return;
            }

            // Boucle principale : choix de l'OS -> choix de l'édition -> téléchargement
            while (true)
            {
                string chosenOs = ChooseWindowsOs();
                if (string.IsNullOrEmpty(chosenOs))
                {
                    Console.WriteLine("\n");
                    Logger.LogOut("Exiting...");
                    break;
                }

                var chosenEdition = ChooseEdition(chosenOs);
                if (chosenEdition == null)
                {
                    Logger.LogWarning("No edition selected. Returning to main menu...");
                    continue;
                }

                // Lance la procédure de téléchargement
                await DownloadIso(chosenEdition);

                Logger.LogOut("Operation completed. Press any key to return to the main menu...");
                Console.ReadKey(true);
                Console.Clear();
            }
        }

        /// <summary>
        /// Vérifie s'il existe une version plus récente en téléchargeant le fichier de version.
        /// Si une mise à jour est disponible, affiche une notification et demande à l'utilisateur s'il souhaite mettre à jour.
        /// Retourne true si l'utilisateur souhaite mettre à jour (ce qui ouvrira la page d'update et fermera l'application),
        /// sinon false.
        /// </summary>
        private static async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                Logger.LogInfo("Checking for updates...");

                // Télécharge le contenu du fichier de version distant
                string remoteVersionRaw = await _httpClient.GetStringAsync(VersionFileUrl);
                string remoteVersion = remoteVersionRaw.Trim();

                // Compare les versions
                if (!string.Equals(remoteVersion, CurrentVersion, StringComparison.OrdinalIgnoreCase))
                {
                    // Affiche une notification Windows
                    DialogResult dr = MessageBox.Show(
                        $"Une nouvelle version est disponible !\nVersion distante : {remoteVersion}\nVotre version : {CurrentVersion}\n\nVoulez-vous mettre à jour ?",
                        "Mise à jour disponible",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information
                    );

                    // Affiche également un message dans la console
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("------------------------------------------------------");
                    Console.WriteLine($"[WARNING] A new version ({remoteVersion}) is available. Your current version is {CurrentVersion}.");
                    Console.WriteLine("------------------------------------------------------");
                    Console.ResetColor();

                    if (dr == DialogResult.Yes)
                    {
                        Logger.LogInfo("User accepted the update.");
                        return true;
                    }
                    else
                    {
                        Logger.LogWarning("User did not accept the update. Running in outdated mode.");
                        _isOutdated = true;
                    }
                }
                else
                {
                    Logger.LogInfo("Application is up to date.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error while checking for updates: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Télécharge et parse le fichier JSON depuis une URL pour remplir _allWindowsData.
        /// </summary>
        private static async Task<bool> LoadJsonDataAsync(string url, string savePath)
        {
            try
            {
                Logger.LogInfo($"Downloading JSON from: {url}");
                string jsonContent = await _httpClient.GetStringAsync(url);

                // Sauvegarde du JSON localement
                await File.WriteAllTextAsync(savePath, jsonContent);

                _allWindowsData = JsonSerializer.Deserialize<Dictionary<string, List<VersionInfo>>>(jsonContent);

                Logger.LogSuccess("JSON file downloaded and loaded successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error downloading or reading JSON: {ex.Message}");
                return false;
            }
        }

        #region Menu Interactif (Flèches)

        /// <summary>
        /// Affiche un menu interactif utilisant les flèches du clavier pour la sélection.
        /// </summary>
        /// <param name="options">Liste des options à afficher.</param>
        /// <param name="title">Titre du menu.</param>
        /// <returns>L'index de l'option sélectionnée, ou -1 si l'utilisateur souhaite quitter (Esc).</returns>
        private static int DisplayInteractiveMenu(List<string> options, string title)
        {
            int selectedIndex = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                if (_isOutdated)
                {
                    Logger.OUTDATEDStartupLog("");
                }
                else
                {
                    Logger.StartupLog("");
                }
                Console.WriteLine($"=== {title} ===\n");

                for (int i = 0; i < options.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine(options[i]);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(options[i]);
                    }
                }

                Console.WriteLine("\nUse Up/Down arrow keys to navigate and Enter to select.");
                Console.WriteLine("Press Escape to exit this menu.");

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex == 0) ? options.Count - 1 : selectedIndex - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex == options.Count - 1) ? 0 : selectedIndex + 1;
                        break;
                    case ConsoleKey.Escape:
                        return -1;
                }

            } while (key != ConsoleKey.Enter);

            return selectedIndex;
        }

        /// <summary>
        /// Affiche la liste des OS disponibles et permet de sélectionner avec les flèches.
        /// </summary>
        private static string ChooseWindowsOs()
        {
            if (_allWindowsData == null || _allWindowsData.Count == 0)
            {
                Logger.LogError("No data loaded from JSON.");
                return null;
            }

            var osNames = new List<string>(_allWindowsData.Keys);
            osNames.Add("Exit");

            int choice = DisplayInteractiveMenu(osNames, "Select Windows OS");
            if (choice == -1 || choice == osNames.Count - 1)
            {
                return null; // Échap ou "Exit"
            }

            return osNames[choice];
        }

        /// <summary>
        /// Affiche la liste des éditions pour l'OS sélectionné, toujours avec les flèches.
        /// </summary>
        private static VersionInfo ChooseEdition(string osName)
        {
            if (!_allWindowsData.ContainsKey(osName)) return null;

            var editions = _allWindowsData[osName];
            if (editions == null || editions.Count == 0)
            {
                Logger.LogWarning($"No editions available for {osName}.");
                return null;
            }

            var editionNames = new List<string>();
            foreach (var edition in editions)
            {
                editionNames.Add(edition.Name);
            }
            editionNames.Add("Back");

            int choice = DisplayInteractiveMenu(editionNames, $"Select version/edition for {osName}");
            if (choice == -1 || choice == editionNames.Count - 1)
            {
                // Échap ou "Back"
                return null;
            }

            return editions[choice];
        }

        #endregion

        #region Téléchargement (Confirmation, choix d'emplacement, barre de progression, etc.)

        /// <summary>
        /// Demande confirmation, demande où sauvegarder le fichier ISO (CustomBrowseForm),
        /// puis lance le téléchargement avec barre de progression et annulation (Esc).
        /// </summary>
        private static async Task DownloadIso(VersionInfo edition)
        {
            if (edition == null || string.IsNullOrEmpty(edition.Url))
            {
                Logger.LogError("Edition or URL invalid.");
                return;
            }

            // 1. Confirmation du téléchargement
            try
            {
                Logger.ClearLog("");
                if (_isOutdated)
                {
                    Logger.OUTDATEDStartupLog("");
                }
                else
                {
                    Logger.StartupLog("");
                }Logger.DownloaderInisialisationLOG("");
                Logger.StaticLog("Selected version: " + edition.Name + "\n");

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" ┌──────────────────────────────────┐");
                Console.WriteLine(" │       Download Confirmation      │");
                Console.WriteLine(" ├──────────────────────────────────┤");
                Console.WriteLine(" │                                  │");
                Console.WriteLine(" │     Press [Enter] to confirm     │");
                Console.WriteLine(" │     Press [Escape] to cancel     │");
                Console.WriteLine(" └──────────────────────────────────┘\n");
                Console.ResetColor();

                ConsoleKey key;
                do
                {
                    key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Escape)
                    {
                        Logger.LogCancel("Download canceled by user.");
                        return;
                    }
                } while (key != ConsoleKey.Enter);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error showing confirmation: {ex.Message}");
                return;
            }

            // 2. Demande où sauvegarder l'ISO via CustomBrowseForm
            string destinationPath = null;
            try
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[+] Please select the file save location for the ISO.");
                destinationPath = await CustomBrowseForm.BrowseAsync(
                    CustomBrowseForm.Mode.SaveFile,
                    edition.Name.Replace(" ", "_") + ".iso" // Nom par défaut
                );

                if (!string.IsNullOrEmpty(destinationPath))
                {
                    Console.WriteLine();
                    Logger.LogInfo($"Downloading to: {destinationPath}");
                }
                else
                {
                    Logger.LogCancel("Saving canceled by user (CommonFileDialog).");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error showing CommonFileDialog: {ex.Message}");
                return;
            }

            Console.ResetColor();

            // 3. Téléchargement du fichier
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();

                // Tâche pour écouter la touche ESC et annuler
                Task listenTask = Task.Run(() =>
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        if (Console.KeyAvailable)
                        {
                            var keyInfo = Console.ReadKey(intercept: true);
                            if (keyInfo.Key == ConsoleKey.Escape)
                            {
                                _cancellationTokenSource.Cancel();
                                break;
                            }
                        }
                        Thread.Sleep(100);
                    }
                }, _cancellationTokenSource.Token);

                await DownloadFileAsync(edition.Url, destinationPath, _cancellationTokenSource.Token);

                // Proposer d'ouvrir l'emplacement du fichier
                DialogResult dr = DialogHelper.ShowMessageBox(
                    "Download complete! Do you want to open the file location?",
                    "Open folder",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{destinationPath}\"")
                        {
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        Logger.LogWarning("Unable to open file explorer.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" Canceled.");
                Console.ResetColor();
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error while downloading: {ex.Message}");
            }
        }

        /// <summary>
        /// Formate un nombre d'octets en chaîne lisible (Ko, Mo, Go...).
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            double fileSize = bytes;
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;

            while (fileSize >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                fileSize /= 1024;
                suffixIndex++;
            }

            return $"{fileSize:F2} {suffixes[suffixIndex]}";
        }

        /// <summary>
        /// Formate un TimeSpan en hh:mm:ss (ou mm:ss si < 1 heure).
        /// </summary>
        private static string FormatTime(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours:D2}h:{timeSpan.Minutes:D2}m:{timeSpan.Seconds:D2}s";
            else
                return $"{timeSpan.Minutes:D2}m:{timeSpan.Seconds:D2}s";
        }

        /// <summary>
        /// Télécharge un fichier depuis une URL vers un chemin local, avec affichage de progression
        /// et gestion de l'annulation (Esc).
        /// </summary>
        private static async Task DownloadFileAsync(string url, string destinationPath, CancellationToken cancellationToken)
        {
            // Crée le répertoire si nécessaire
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

            using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;

                // Vérification espace disque (optionnel)
                if (totalBytes.HasValue)
                {
                    string driveRoot = Path.GetPathRoot(destinationPath);
                    if (!string.IsNullOrEmpty(driveRoot))
                    {
                        DriveInfo drive = new DriveInfo(driveRoot);
                        if (drive.AvailableFreeSpace < totalBytes.Value)
                        {
                            // Espace insuffisant
                            DialogResult drSpace = DialogHelper.ShowMessageBox(
                                "Insufficient disk space for this download.\n" +
                                $"Available: {FormatBytes(drive.AvailableFreeSpace)} / " +
                                $"Required: {FormatBytes(totalBytes.Value)}\n\n" +
                                "Do you want to continue anyway?",
                                "Not enough disk space",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );

                            if (drSpace == DialogResult.No)
                            {
                                Logger.LogCancel("Download canceled due to insufficient disk space.");
                                return;
                            }
                        }
                    }
                }

                using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[81920];
                    long totalBytesRead = 0;
                    int bytesRead;
                    DateTime startTime = DateTime.Now;
                    DateTime lastUpdate = DateTime.Now;
                    int progressBarWidth = 30;

                    bool initialCursorVisible = Console.CursorVisible;
                    Console.CursorVisible = false;

                    Console.Write("[+] Downloading... (Press ESC to cancel)\n");

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        totalBytesRead += bytesRead;

                        if ((DateTime.Now - lastUpdate).TotalMilliseconds > 200)
                        {
                            lastUpdate = DateTime.Now;

                            double bytesPerSecond = totalBytesRead / (DateTime.Now - startTime).TotalSeconds;
                            double speedInMb = bytesPerSecond / (1024 * 1024);

                            double progressPercent = 0;
                            string progressString;
                            string sizeInfo;
                            string speedInfo = $"{speedInMb:F2} MB/s";
                            string timeLeftInfo = "";

                            if (totalBytes.HasValue)
                            {
                                progressPercent = (double)totalBytesRead / totalBytes.Value;
                                progressString = $"{progressPercent * 100:F1}%";
                                sizeInfo = $"{FormatBytes(totalBytesRead)}/{FormatBytes(totalBytes.Value)}";

                                long remainingBytes = totalBytes.Value - totalBytesRead;
                                double secondsRemaining = bytesPerSecond > 0 ? remainingBytes / bytesPerSecond : 0;
                                var timeRemaining = TimeSpan.FromSeconds(secondsRemaining);
                                timeLeftInfo = FormatTime(timeRemaining);
                            }
                            else
                            {
                                progressString = "?%";
                                sizeInfo = $"{FormatBytes(totalBytesRead)}/???";
                            }

                            int progressBlocks = totalBytes.HasValue
                                ? (int)(progressPercent * progressBarWidth)
                                : (int)((DateTime.Now - startTime).TotalSeconds % progressBarWidth);

                            string bar = new string('█', Math.Min(progressBlocks, progressBarWidth))
                                         + new string(' ', Math.Max(progressBarWidth - progressBlocks, 0));

                            string progressLine =
                                $"[{bar}] {progressString}  {sizeInfo}  {speedInfo}  {timeLeftInfo}";

                            int consoleWidth = Console.WindowWidth;
                            if (progressLine.Length >= consoleWidth)
                                progressLine = progressLine.Substring(0, consoleWidth - 1);

                            Console.Write("\r" + progressLine);
                        }
                    }

                    Console.Write("\r");
                    int cleanWidth = Console.WindowWidth - 1;
                    Console.Write(new string(' ', cleanWidth));
                    Console.Write("\r");

                    if (totalBytes.HasValue)
                    {
                        TimeSpan totalTime = DateTime.Now - startTime;
                        double finalSpeedInMb = (totalBytes.Value / totalTime.TotalSeconds) / (1024 * 1024);

                        Console.Write("[");
                        Console.Write(new string('█', progressBarWidth));
                        Console.Write("] 100%");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" Done\n");
                        Console.ResetColor();
                        Console.WriteLine(
                            $"\nDownload completed:\n" +
                            $"  Size: {FormatBytes(totalBytes.Value)}\n" +
                            $"  Avg speed: {finalSpeedInMb:F2} MB/s\n" +
                            $"  Elapsed time: {FormatTime(totalTime)}"
                        );
                    }
                    else
                    {
                        Console.WriteLine($"Download completed (unknown file size): {destinationPath}");
                    }

                    Console.CursorVisible = initialCursorVisible;
                }
            }

            Console.WriteLine("\n");
            Logger.LogSuccess($"Download completed: {destinationPath}");
        }

        #endregion
    }
}
