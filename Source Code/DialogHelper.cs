using System;
using System.Threading;
using System.Windows.Forms;

namespace ConsoleIsoDownloader
{
    /// <summary>
    /// Classe Helper pour afficher des dialogues WinForms.
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Affiche une MessageBox sur un thread STA et retourne le résultat.
        /// </summary>
        public static DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            DialogResult result = DialogResult.None;

            Thread thread = new Thread(() =>
            {
                result = MessageBox.Show(text, caption, buttons, icon);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return result;
        }

        // La méthode ShowSaveFileDialog a été supprimée car elle n'est plus nécessaire.
    }
}
