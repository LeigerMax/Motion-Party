📦 FireflySpawnManager - Module de gestion du spawn des lucioles

🔧 Fonctionnalités principales
- Gestion de la zone de spawn basée sur FireflyDanceConfig.
- Spawn automatique des lucioles dans la zone selon maxFireflies et spawnInterval.
- Suppression automatique des lucioles après leur durée de vie.
- Intégration facile avec le prefab luciole existant.

🏗️ Structure du dossier
/FireflySpawnManager/
├── FireflySpawnManager.cs
└── README.md

🚀 Instructions pour utiliser / intégrer
- Ajouter le prefab FireflySpawnManager sur un GameObject vide dans la scène.
- Assigner la référence au prefab luciole dans l'inspecteur.
- Assigner la référence à l'asset FireflyDanceConfig dans l'inspecteur.
- Le prefab luciole doit contenir au moins un Renderer.
- Aucun autre module spécifique requis à cette étape.

📋 Paramètres configurables dans l'inspecteur

**Configuration :**
- `config` : Référence vers l'asset FireflyDanceConfig (obligatoire)
- `fireflyPrefab` : Référence vers le prefab de luciole (obligatoire)

**Spawn Settings :**
- `autoSpawn` : Démarrage automatique du spawn au Start() (par défaut : true)
- `showDebugGizmos` : Affiche la zone de spawn dans l'éditeur (par défaut : true)

**Runtime Info (Read Only) :**
- `activeFireflyCount` : Nombre de lucioles actuellement actives
- `isSpawning` : Indique si le spawn manager est en cours d'exécution

🎮 API publique

**Méthodes de contrôle :**
- `StartSpawning()` : Démarre le spawn automatique
- `StopSpawning()` : Arrête le spawn automatique
- `ForceSpawnFirefly()` : Force le spawn d'une luciole si possible
- `ClearAllFireflies()` : Supprime toutes les lucioles actives

**Méthodes d'information :**
- `GetActiveFireflyCount()` : Retourne le nombre de lucioles actives
- `IsSpawning()` : Vérifie si le spawn manager est actif

⚙️ Fonctionnement technique

**Système de spawn :**
- Utilise deux coroutines : une pour le spawn, une pour le nettoyage
- Vérifie les limites MaxFireflies avant chaque spawn
- Génère des positions aléaoires dans la zone définie par TopLeft/BottomRight

**Gestion de la durée de vie :**
- Ajoute automatiquement un composant FireflyLifetimeTracker aux lucioles
- Nettoie automatiquement les lucioles expirées chaque seconde
- Gère les références nulles (lucioles supprimées manuellement)

**Debugging :**
- Gizmos visuels pour voir la zone de spawn dans l'éditeur (rectangle jaune + centre rouge)
- Logs détaillés pour le spawn et la suppression des lucioles
- Validation automatique des références au démarrage

🔍 Dépendances
- `FireflyDanceConfig.cs` : Configuration des paramètres de jeu
- Prefab de luciole avec au minimum un Renderer
- Unity 2022.3+ (utilise les coroutines et Time.time)

⚠️ Notes importantes
- Le module est autonome et n'interfère pas avec d'autres systèmes
- Les lucioles sont spawnées à Z=0 par défaut
- Le nettoyage automatique évite les fuites mémoire
- Compatible avec l'édition en temps réel des paramètres dans FireflyDanceConfig
