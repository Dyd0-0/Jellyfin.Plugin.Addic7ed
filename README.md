# Jellyfin.Plugin.Addic7ed

Jellyfin plugin for downloading subtitles from Addic7ed.com

[English](#english) | [Français](#français)

---

## English

### Features

- Search and download subtitles from Addic7ed.com
- Support for multiple languages
- User authentication for registered Addic7ed users
- Configurable download limits
- Integration with Jellyfin's subtitle search interface

### Installation

1. Download the latest release from the [releases page](https://github.com/yourusername/Jellyfin.Plugin.Addic7ed/releases)
2. Extract the zip file
3. Copy the `Jellyfin.Plugin.Addic7ed.dll` file to your Jellyfin plugins directory:
   - For Windows: `%PROGRAMDATA%\Jellyfin\Server\plugins`
   - For Linux: `/var/lib/jellyfin/plugins` or `/usr/share/jellyfin/plugins`
   - For Docker: `/jellyfin/plugins`
4. Restart Jellyfin

### Configuration

1. Go to the Jellyfin admin dashboard
2. Navigate to Plugins
3. Find the Addic7ed plugin and click on it
4. Configure the plugin with your Addic7ed.com username and password (optional but recommended)
5. Select the languages you want to download subtitles for
6. Set the maximum number of downloads per day (Addic7ed limits to 40 for registered users)
7. Click Save

### How to Download Subtitles

1. Go to a TV show episode in your Jellyfin library
2. Click on the "..." menu (three dots) in the bottom right corner
3. Select "Edit subtitles"
4. Click "Search for Subtitles"
5. The plugin will search for subtitles on Addic7ed.com
6. Select the subtitle you want to download from the list
7. The subtitle will be downloaded and automatically added to your episode

### Notes

- Addic7ed.com limits downloads to 40 per day for registered users
- The plugin works best with a registered account
- The plugin only supports TV shows, not movies

### Building from Source

1. Make sure you have .NET 6.0 SDK installed
2. Clone the repository
3. Run the build script:
   - Windows: `.\build.ps1`
   - Linux/macOS: `./build.sh`
4. The compiled plugin will be in the `dist` folder

---

## Français

### Fonctionnalités

- Recherche et téléchargement de sous-titres depuis Addic7ed.com
- Support pour plusieurs langues
- Authentification utilisateur pour les utilisateurs enregistrés sur Addic7ed
- Limites de téléchargement configurables
- Intégration avec l'interface de recherche de sous-titres de Jellyfin

### Installation

1. Téléchargez la dernière version depuis la [page des releases](https://github.com/yourusername/Jellyfin.Plugin.Addic7ed/releases)
2. Extrayez le fichier zip
3. Copiez le fichier `Jellyfin.Plugin.Addic7ed.dll` dans le répertoire des plugins Jellyfin :
   - Pour Windows : `%PROGRAMDATA%\Jellyfin\Server\plugins`
   - Pour Linux : `/var/lib/jellyfin/plugins` ou `/usr/share/jellyfin/plugins`
   - Pour Docker : `/jellyfin/plugins`
4. Redémarrez Jellyfin

### Configuration

1. Accédez au tableau de bord d'administration de Jellyfin
2. Naviguez vers Plugins
3. Trouvez le plugin Addic7ed et cliquez dessus
4. Configurez le plugin avec votre nom d'utilisateur et mot de passe Addic7ed.com (optionnel mais recommandé)
5. Sélectionnez les langues pour lesquelles vous souhaitez télécharger des sous-titres
6. Définissez le nombre maximum de téléchargements par jour (Addic7ed limite à 40 pour les utilisateurs enregistrés)
7. Cliquez sur Enregistrer

### Comment Télécharger des Sous-titres

1. Accédez à un épisode de série TV dans votre bibliothèque Jellyfin
2. Cliquez sur le menu "..." (trois points) dans le coin inférieur droit
3. Sélectionnez "Modifier les sous-titres"
4. Cliquez sur "Rechercher des sous-titres"
5. Le plugin recherchera des sous-titres sur Addic7ed.com
6. Sélectionnez le sous-titre que vous souhaitez télécharger dans la liste
7. Le sous-titre sera téléchargé et automatiquement ajouté à votre épisode

### Remarques

- Addic7ed.com limite les téléchargements à 40 par jour pour les utilisateurs enregistrés
- Le plugin fonctionne mieux avec un compte enregistré
- Le plugin ne prend en charge que les séries TV, pas les films

### Compilation depuis les Sources

1. Assurez-vous d'avoir le SDK .NET 6.0 installé
2. Clonez le dépôt
3. Exécutez le script de compilation :
   - Windows : `.\build.ps1`
   - Linux/macOS : `./build.sh`
4. Le plugin compilé se trouvera dans le dossier `dist`
