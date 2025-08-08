# Motion Party - Intégration Unity + Python Simplifiée

## Vue d'ensemble

Ce système permet l'intégration automatique entre le jeu Unity "Motion Party" et le système de tracking Python. Quand vous lancez le jeu, Python se lance automatiquement et un écran de chargement s'affiche jusqu'à ce que les données de tracking soient reçues.

## Structure du projet

```
Motion-Party/
├── Motion-Party/          # Projet Unity
│   ├── Assets/
│   │   └── Scripts/
│   │       └── Core/
│   │           ├── UDPReceive.cs           # Réception UDP des données Python
│   │           ├── PythonLauncher.cs       # Lance Python automatiquement
│   │           ├── SimpleLoadingScreen.cs  # Écran de chargement simple
│   │           ├── SimpleLoadingCreator.cs # Création de l'interface
│   │           └── PythonIntegrationSetup.cs # Configuration automatique
│   └── ...
├── python-tracker/        # Scripts Python
│   ├── main.py            # Script principal de tracking
│   ├── requirements.txt   # Dépendances Python
│   └── ...
└── setup.bat             # Installation automatique
```

## Fonctionnement Simplifié

### 1. Au démarrage du jeu
- Unity affiche automatiquement un écran de chargement simple
- Python se lance en arrière-plan
- Le système attend les premières données UDP

### 2. Quand Python envoie des données
- L'écran de chargement disparaît automatiquement
- Le jeu principal commence

**C'est tout ! Pas de barres de progression complexes, pas de pourcentages - juste un système simple et fiable.**
## Installation Rapide

### 1. Setup automatique
```batch
# Exécuter le script de setup
setup.bat
```

### 2. Configuration Unity
1. **Option automatique** : Ajoutez `PythonIntegrationSetup.cs` à un GameObject dans votre scène
2. **Option manuelle** : Cliquez sur "Setup Python Integration" dans l'Inspector

## Composants Principaux

### Core Scripts

- **PythonLauncher.cs** : Lance le processus Python automatiquement au démarrage
- **UDPReceive.cs** : Reçoit les données de tracking via UDP (port 5052)
- **SimpleLoadingScreen.cs** : Gère l'affichage simple de l'écran de chargement
- **SimpleLoadingCreator.cs** : Crée l'interface de chargement programmatiquement

### Setup
- **PythonIntegrationSetup.cs** : Configuration automatique en un clic

## Communication UDP

**Port :** 5052  
**Format des données :** JSON avec informations de tracking

Exemple de données reçues :
```json
{
  "hands": [
    {
      "landmarks": [...],
      "handedness": "Left"
    }
  ],
  "pose": {
    "landmarks": [...]
  },
  "timestamp": 1234567890
}
```

## Build et Déploiement

### Structure requise pour le build
```
MonGame.exe
MonGame_Data/
python-tracker/
  main.py
  requirements.txt
  (autres fichiers Python)
```

### Points importants
- Le dossier `python-tracker` doit être dans le même répertoire que l'exe
- Python doit être installé sur la machine cible
- Le port 5052 doit être disponible

## Personnalisation

### Modifier l'écran de chargement
Éditer `SimpleLoadingScreen.cs` pour changer l'apparence :
```csharp
// Modifier le texte affiché
private const string LOADING_TEXT = "Initialisation en cours...";

// Modifier la couleur de fond
private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
```

### Changer le port UDP
Modifier la propriété dans `UDPReceive.cs` :
```csharp
public int port = 5052; // Votre nouveau port
```

### Configuration Python
Ajuster les paramètres dans `PythonLauncher.cs` :
```csharp
[SerializeField] private string pythonExecutable = "python";
[SerializeField] private string pythonScriptPath = "python-tracker/main.py";
```

## Test et Debug

### Script de test inclus
`TestSimpleSystem.cs` affiche le statut des composants :
- État de Python (running/stopped)
- Réception des données UDP (received/waiting)
- Statut de l'écran de chargement (visible/hidden)

### Logs importants à surveiller
```
[PythonLauncher] Python démarré avec succès. PID: 1234
[UDPReceive] Première donnée UDP reçue !
[SimpleLoadingScreen] Masquage de l'écran de chargement
```

## Dépannage

### Problèmes courants

1. **Python ne démarre pas**
   - Vérifier que `python` est dans le PATH système
   - Vérifier que le chemin vers `main.py` est correct

2. **Pas de données UDP**
   - Vérifier que Python envoie bien sur le port 5052
   - S'assurer que le firewall n'bloque pas les connexions locales

3. **Écran de chargement reste affiché**
   - Vérifier les logs Unity pour voir si les données UDP arrivent
   - Tester manuellement la réception UDP avec `TestSimpleSystem.cs`

4. **Python s'arrête immédiatement**
   - Vérifier les dépendances Python avec `pip install -r requirements.txt`
   - Lancer `python main.py` manuellement pour voir les erreurs

## Évolution depuis la version complexe

Cette version simplifie drastiquement le système précédent :
- ❌ Suppression des barres de progression complexes
- ❌ Suppression des multiples étapes de chargement
- ❌ Suppression des scripts de debug avancés
- ✅ Interface simple et claire
- ✅ Système binaire : chargement ON/OFF
- ✅ Plus fiable et plus facile à maintenir

**Principe :** Afficher un écran de chargement au démarrage, le masquer dès que les données UDP arrivent. Simple et efficace !
```

2. Vérifiez les permissions d'exécution

3. Consultez les logs Unity (Console)

4. **NOUVEAU**: Vérifiez l'écran de chargement pour les messages d'erreur

### Pas de données reçues
1. Vérifiez que le port 5052 n'est pas occupé
2. Vérifiez que la webcam fonctionne
3. Activez `DEBUG = True` dans config.py
4. L'écran de chargement vous indiquera si la connexion UDP fonctionne

### L'écran de chargement reste bloqué (CORRIGÉ!)
1. **Utilisez le debugger intégré** : `PythonDebugger` est automatiquement ajouté
2. **Vérifiez les logs Unity** pour les erreurs Python
3. **Testez la connexion UDP** : `cd python-tracker && python test_udp.py`
4. **Forcez la fermeture** avec le bouton dans l'interface debug
5. **Vérifiez que les dépendances sont installées**
6. Le timeout par défaut est de 30 secondes
7. **Nouveau**: L'écran se ferme automatiquement dès réception des premières données UDP

### Erreurs de dépendances
```batch
# Réinstaller les dépendances
cd python-tracker
pip install --force-reinstall -r requirements.txt
```

## Distribution

Pour distribuer votre jeu :

1. **Buildez Unity** avec `PythonBuildLauncher` activé
2. **Incluez le dossier** `python-tracker` dans votre package
3. **Fournissez les scripts** `setup.bat` et `start_motion_party.bat`
4. **Instructions utilisateur** :
   - Installer Python si nécessaire
   - Exécuter `setup.bat` une fois
   - Utiliser `start_motion_party.bat` pour jouer

## Optimisations Build

- Désactivez `DEBUG` dans config.py pour la production
- Activez `hideConsoleWindow` dans `PythonBuildLauncher`
- Testez sur des machines sans environnement de développement

## Support et Debug

### Outils de diagnostic intégrés
- **PythonDebugger** : Interface debug avec contrôles en temps réel
- **test_udp.py** : Script de test pour vérifier la connexion UDP
- **Logs Unity** : Messages détaillés dans la Console

### Tests rapides
```batch
# Tester Python séparément
cd python-tracker
python main.py

# Tester uniquement la connexion UDP  
cd python-tracker
python test_udp.py
```

Pour les problèmes :
1. Vérifiez les logs Unity (Console window)
2. Utilisez l'interface debug (coin supérieur gauche en jeu)
3. Testez la connexion UDP avec test_udp.py
4. Vérifiez la configuration réseau/firewall
