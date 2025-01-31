using System;
using System.Windows.Forms;

namespace ConsoleIsoDownloader
{
    /// <summary>
    /// Classe utilitaire pour la gestion des logs en console, avec palette de couleurs harmonisée et formatage amélioré (commentaires en français).
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Génère un timestamp formaté pour accompagner chaque log.
        /// </summary>
        private static string GetTimestamp()
        {
            return $"[{DateTime.Now:HH:mm:ss}]";
        }

        /// <summary>
        /// Log de démarrage (affiche une bannière ou tout autre message de startup).
        /// </summary>
        public static void StartupLog(string message)
        {
            Console.ForegroundColor = ColorPalette.StartupColor;
            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine("            Media Windows Downloader         ");
            Console.WriteLine("                  Version 1.4                ");
            Console.WriteLine("                                             ");
            Console.WriteLine("Project Code: OmniTools.WinMediaDownloader   ");
            Console.WriteLine("Core Version: oem5.2                         ");
            Console.WriteLine("ISOs Sources: malwarewatch.org - archive.org ");
            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine();
            Console.ResetColor();

            if (!string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = ColorPalette.InfoColor;
                Console.WriteLine($"{GetTimestamp()} [STARTUP] {message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log de démarrage (Pour les version OUDATED).
        /// </summary>
        public static void OUTDATEDStartupLog(string message)
        {
            Console.ForegroundColor = ColorPalette.StartupColor;
            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine("            Media Windows Downloader         ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("            Version 1.4 - [OUTDATED]          ");
            Console.ResetColor();
            Console.ForegroundColor = ColorPalette.StartupColor;
            Console.WriteLine("                                             ");
            Console.WriteLine("Project Code: OmniTools.WinMediaDownloader   ");
            Console.WriteLine("Core Version: oem5.2                         ");
            Console.WriteLine("ISOs Sources: malwarewatch.org - archive.org ");
            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine();
            Console.ResetColor();

            if (!string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = ColorPalette.InfoColor;
                Console.WriteLine($"{GetTimestamp()} [STARTUP] {message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log d'information générique.
        /// </summary>
        public static void LogInfo(string message)
        {
            Console.ForegroundColor = ColorPalette.InfoColor;
            Console.WriteLine($"{GetTimestamp()} [INFO] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Log "statique" pour afficher des informations supplémentaires.
        /// </summary>
        public static void StaticLog(string message)
        {
            Console.ForegroundColor = ColorPalette.StaticColor;
            Console.WriteLine($"[+] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Log d'opération réussie.
        /// </summary>
        public static void LogSuccess(string message)
        {
            Console.ForegroundColor = ColorPalette.SuccessColor;
            Console.WriteLine($"{GetTimestamp()} [SUCCESS] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Log pour demander une confirmation.
        /// </summary>
        public static void ConfirmationMsg(string message)
        {
            Console.ForegroundColor = ColorPalette.ConfirmationColor;
            Console.WriteLine($"{GetTimestamp()} [CONFIRM] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Log d'avertissement.
        /// </summary>
        public static void LogWarning(string message)
        {
            Console.ForegroundColor = ColorPalette.WarningColor;
            Console.WriteLine($"{GetTimestamp()} [WARNING] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Log pour signifier une annulation.
        /// </summary>
        public static void LogCancel(string message)
        {
            Console.ForegroundColor = ColorPalette.CancelColor;
            Console.WriteLine($"{GetTimestamp()} [CANCEL] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Log pour l'initialisation du downloader.
        /// </summary>
        public static void DownloaderInisialisationLOG(string message)
        {

            if (!string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = ColorPalette.InfoColor;
                Console.WriteLine($"{GetTimestamp()} [INIT] {message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log d'erreur.
        /// </summary>
        public static void LogError(string message)
        {
            // Petit bip sonore pour souligner l’erreur
            try
            {
                Console.Beep(500, 200);
            }
            catch
            {
                // Certains environnements ne supportent pas Console.Beep()
            }

            Console.ForegroundColor = ColorPalette.ErrorColor;
            Console.WriteLine($"{GetTimestamp()} [ERROR] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Log "sortie" ou standard.
        /// </summary>
        public static void LogOut(string message)
        {
            Console.ForegroundColor = ColorPalette.DefaultColor;
            Console.WriteLine($"{GetTimestamp()} {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Effacer la console (clear).
        /// </summary>
        public static void ClearLog(string message = "")
        {
            Console.ResetColor();
            Console.Clear();
        }

        /// <summary>
        /// Log pour les barres de progression.
        /// </summary>
        public static void LogProgress(string message)
        {
            Console.ForegroundColor = ColorPalette.ProgressBarColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
