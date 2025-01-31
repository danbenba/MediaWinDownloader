# Media Windows Downloader

**Media Windows Downloader** est une application console en C#/.NET qui permet de lister et télécharger facilement différentes versions de Windows (ou autres images ISO).  
Elle s’appuie sur un fichier JSON hébergé en ligne (ou localement) pour proposer un menu interactif permettant de sélectionner l’OS et l’édition à télécharger.  
Un système de mise à jour intégrée avertit également l’utilisateur lorsqu’une version plus récente de l’outil est disponible.

## Sommaire

1. [Aperçu des fonctionnalités](#aperçu-des-fonctionnalités)
2. [Prérequis](#prérequis)
3. [Installation et Compilation](#installation-et-compilation)
4. [Configuration du JSON](#configuration-du-json)
5. [Utilisation](#utilisation)
6. [Mise à jour automatique](#mise-à-jour-automatique)
7. [Journalisation (Logging)](#journalisation-logging)
8. [Limitations et Responsabilités](#limitations-et-responsabilités)
9. [Contribuer](#contribuer)
10. [Licence](#licence)

---

## Aperçu des fonctionnalités

- **Menu interactif** : Utilise les flèches haut/bas et la touche Entrée pour sélectionner l’OS et l’édition à télécharger.  
- **Téléchargement avec barre de progression** : Affichage du pourcentage, vitesse de téléchargement, temps estimé restant, etc.  
- **Annulation (Esc)** : Permet d’annuler le téléchargement en cours en pressant la touche `Echap`.  
- **Vérification d’espace disque** : Alerte l’utilisateur si l’espace disque est insuffisant.  
- **Choix du chemin de sauvegarde** : Propose à l’utilisateur d’enregistrer l’ISO à l’emplacement de son choix (via une boîte de dialogue).  
- **Mise à jour intégrée** : Compare la version locale à une version distante et propose d’ouvrir la page GitHub pour récupérer la dernière version.  
- **Mode “OUTDATED”** : Si l’utilisateur refuse la mise à jour, l’application indique qu’elle est obsolète (`OUTDATED`) dans le titre de la console et les logs.

---

## Prérequis

- **.NET Desktop Runtime 9.0** ou version supérieure (ou le SDK correspondant si vous souhaitez recompiler le projet).  
- **Système Windows** (l’application utilise `System.Windows.Forms` pour afficher des boîtes de dialogue, donc elle n’est pas cross-platform en l’état).  

> Vous pouvez vérifier la présence du runtime .NET avec la commande :  
> ```bash
> dotnet --list-runtimes
> ```

---

## Installation et Compilation

1. **Cloner le dépôt**  
   ```bash
   git clone https://github.com/danbenba/MediaWinDownloader.git
   cd MediaWinDownloader
   ```

2. **Ouvrir le projet dans Visual Studio** (ou tout autre IDE compatible .NET)  
   - Double-cliquez sur `MediaWinDownloader.sln` (si disponible) ou ouvrez-le via Visual Studio.

3. **Restaurer les packages NuGet** (si nécessaire, Visual Studio le fait généralement automatiquement).

4. **Compiler/Publier**  
   - Dans Visual Studio, vous pouvez sélectionner `Release` puis `Build > Build Solution`.
   - Vous obtiendrez l’exécutable dans le répertoire `bin\Release\net9.0-windows` (ou équivalent selon la configuration).

5. **Exécuter l’application**  
   - Soit via Visual Studio,  
   - Soit en exécutant directement `MediaWinDownloader.exe` depuis le dossier `bin\Release\net9.0-windows`.

---

## Configuration du JSON

L’application repose sur un fichier JSON pour la liste des systèmes Windows disponibles. Par défaut, l’URL pointant vers ce JSON se trouve dans :

```csharp
private const string DefaultJsonUrl = "https://raw.githubusercontent.com/danbenba/MediaWinDownloader/refs/heads/project/images.json";
```

Dans ce fichier JSON, la structure est la suivante :

```jsonc
{
  "Windows 1.01 to 4.0": [
    {
      "Name": "Windows 1.01 (x86)",
      "Url": "https://dl.malwarewatch.org/windows/Windows-1.01.zip"
    },
    {
      "Name": "Windows 2.03 (x86)",
      "Url": "https://dl.malwarewatch.org/windows/Windows-2.03.zip"
    }
    // ...
  ],
  "Windows 95": [
    {
      "Name": "Windows 95 (x86)",
      "Url": "https://dl.malwarewatch.org/windows/Windows-95.iso"
    },
    // ...
  ],
  // ...
}
```

- **Clé** : Représente la “famille” ou la version majeure de Windows (ex: "Windows 95", "Windows XP", etc.).  
- **Liste** : Chaque entrée dans la liste contient un objet `Name` et un `Url`.

### Personnaliser le JSON

- Vous pouvez héberger votre propre fichier `images.json` sur un autre serveur ou GitHub.
- Lancer l’application avec un **argument** pointant vers votre JSON personnalisé :  
  ```bash
  MediaWinDownloader.exe "https://votre-url.com/mon-fichier-json.json"
  ```
- Si l’URL est invalide ou non fournie, l’application tente d’utiliser l’URL par défaut.

### Chargement local du JSON

- Si le fichier `wim.img.json` n’est pas déjà présent localement, l’application le télécharge et l’enregistre à la racine du dossier (là où se trouve l’exécutable).  
- En cas d’échec du téléchargement, l’application ne peut pas fonctionner correctement.

---

## Utilisation

Une fois l’application lancée :

1. **Vérification de la version**  
   - L’application compare la version locale avec la version distante.  
   - Si une version plus récente est trouvée, une boîte de dialogue s’ouvre pour demander si vous souhaitez mettre à jour.  

2. **Menu principal**  
   - L’écran liste tous les systèmes Windows chargés depuis le fichier JSON.  
   - Utilisez les flèches **haut/bas** pour sélectionner la famille de Windows, puis **Entrée** pour valider.  
   - Appuyez sur **Esc** pour quitter le menu.

3. **Sélection de l’édition/version**  
   - Pour la famille de Windows choisie, sélectionnez l’édition désirée (par exemple, “Windows 7 Professionnal (x64)”).  
   - Pressez **Entrée** pour passer à l’étape suivante.

4. **Confirmation**  
   - Le programme demande une confirmation pour lancer le téléchargement.  
   - Appuyez sur **Entrée** pour confirmer, ou **Esc** pour annuler.

5. **Choix du chemin d’enregistrement**  
   - Une boîte de dialogue Windows s’ouvre pour sélectionner l’emplacement et le nom du fichier ISO à sauvegarder.  
   - Validez votre choix ou annulez.

6. **Téléchargement**  
   - La console affiche la progression, la vitesse, la taille totale et le temps estimé restant.  
   - Vous pouvez appuyer sur **Esc** à tout moment pour annuler le téléchargement.

7. **Fin**  
   - Une fois le téléchargement terminé, vous pouvez ouvrir le dossier de destination directement depuis la boîte de dialogue ou la console.

---

## Mise à jour automatique

Le programme vérifie la version distante sur GitHub via le fichier `app.version` :

```csharp
private const string VersionFileUrl = "https://raw.githubusercontent.com/danbenba/MediaWinDownloader/refs/heads/project/app.version";
```

- **Fichier `app.version`** : Contient simplement le numéro de version le plus récent (ex: `1.2`).  
- Si la version distante diffère de `CurrentVersion`, l’utilisateur reçoit une notification via `MessageBox`.  
- S’il accepte la mise à jour, la page GitHub s’ouvre par défaut, et l’application se ferme.

---

## Journalisation (Logging)

Le programme utilise plusieurs méthodes de logs (info, warning, error, success, etc.) via une classe `Logger`.  
Ces logs sont affichés dans la console et éventuellement enregistrés dans un fichier, selon l’implémentation que vous avez définie.  

### Modes de logs spéciaux

- **Mode OUTDATED** : Si l’utilisateur refuse une mise à jour disponible, toutes les sorties sont préfixées par “[OUTDATED]” dans le titre de la console.

Vous pouvez personnaliser ces logs dans la classe `Logger` selon vos préférences (fichier, console, etc.).

---

## Limitations et Responsabilités

1. **Images Windows** : Les ISO répertoriées sont soumises à la licence de Microsoft. Assurez-vous de respecter les conditions d’utilisation et la légalité de vos téléchargements.  
2. **Hébergement des fichiers** : Les URLs pointent vers des hébergeurs tiers (Archive.org, malwarewatch.org). Nous ne garantissons pas la pérennité de ces liens ni la légalité des contenus.  
3. **Utilisation pédagogique ou de test** : Ce projet est principalement à vocation éducative (apprentissage de C#, .NET, etc.). Toute utilisation en production ou dans un cadre professionnel doit être évaluée et sécurisée par vos soins.  
4. **Mises à jour** : Les informations de version et URL utilisées pour le check de mise à jour peuvent devenir obsolètes si le dépôt GitHub change ou est supprimé.

---

## Les contraintes des URLs

### le site malwarewatch.org impose une limite de 25 téléchargements par jour. Au-delà de cette limite, les ISO de Windows, de la version 1.01 à la 10 (1067), ne seront plus accessibles au téléchargement. De plus, archive.org dispose d'un débit de téléchargement réduit afin d'éviter de surcharger ses serveurs.

---

## Contribuer

1. **Fork** : Forkez ce dépôt.  
2. **Créez une branche** :  
   ```bash
   git checkout -b ma-nouvelle-fonction
   ```
3. **Implémentez et commitez** vos changements.  
4. **Proposez une Pull Request** sur le dépôt d’origine.

Toute amélioration ou suggestion (fonctionnalités, corrections, documentation) est la bienvenue !

> **Disclaimer** : L’auteur du code et/ou les contributeurs ne peuvent être tenus responsables d’une utilisation illégale ou abusive de ce programme. Vous êtes seul responsable de l’utilisation que vous en faites, notamment en ce qui concerne le téléchargement d’images ISO ou d’autres médias protégés.  

Bon téléchargement et bonne expérimentation avec **Media Windows Downloader** !
