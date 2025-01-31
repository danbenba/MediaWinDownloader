using System;

namespace ConsoleIsoDownloader
{
    /// <summary>
    /// Palette de couleurs centralisée et harmonisée pour tous les logs et messages de l'application.
    /// </summary>
    public static class ColorPalette
    {
        // Couleur pour les logs d'information générique
        public static ConsoleColor InfoColor { get; set; } = ConsoleColor.Gray;

        // Couleur pour les messages statiques ou techniques
        public static ConsoleColor StaticColor { get; set; } = ConsoleColor.Magenta;

        // Couleur pour les logs de succès
        public static ConsoleColor SuccessColor { get; set; } = ConsoleColor.Green;

        // Couleur pour les messages de confirmation
        public static ConsoleColor ConfirmationColor { get; set; } = ConsoleColor.DarkCyan;

        // Couleur pour les avertissements
        public static ConsoleColor WarningColor { get; set; } = ConsoleColor.Yellow;

        // Couleur pour les messages d'annulation
        public static ConsoleColor CancelColor { get; set; } = ConsoleColor.DarkYellow;

        // Couleur pour les séparateurs ou espaces
        public static ConsoleColor SpaceColor { get; set; } = ConsoleColor.Gray;

        // Couleur pour les messages d'initialisation du downloader
        public static ConsoleColor StartupColor { get; set; } = ConsoleColor.Cyan;

        // Couleur pour les messages d'erreur
        public static ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

        // Couleur par défaut pour les logs standards
        public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;

        // Couleur pour les barres de progression
        public static ConsoleColor ProgressBarColor { get; set; } = ConsoleColor.White;

        // Méthode pour réinitialiser les couleurs à leurs valeurs par défaut
        public static void ResetToDefault()
        {
            InfoColor = ConsoleColor.Cyan;
            StaticColor = ConsoleColor.Magenta;
            SuccessColor = ConsoleColor.Green;
            ConfirmationColor = ConsoleColor.DarkCyan;
            WarningColor = ConsoleColor.Yellow;
            CancelColor = ConsoleColor.DarkYellow;
            SpaceColor = ConsoleColor.Gray;
            StartupColor = ConsoleColor.DarkGreen;
            ErrorColor = ConsoleColor.Red;
            DefaultColor = ConsoleColor.White;
            ProgressBarColor = ConsoleColor.DarkGray;
        }
    }
}
