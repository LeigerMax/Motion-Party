# Guide d'Installation - Système de Badges Global

Ce guide vous accompagne étape par étape pour installer et configurer le système de badges global de Motion-Party.

## Prérequis

- Unity 2021.3 ou plus récent
- Projet Motion-Party configuré
- Accès au dossier `Assets/Scripts/Gameplay/Common/BadgeSystem/`

## Installation Rapide (Recommandée)

### Option 1 : Installation Automatique via l'Éditeur

1. **Ouvrir l'éditeur de badges**
   ```
   Menu Unity → Window → Gameplay → Global Badge System
   ```

2. **Vérifier le statut**
   - L'interface affiche le statut de chaque composant (DB, SYS, TRACK, STORE)
   - Les composants manquants apparaissent en rouge (✗)

3. **Installation automatique**
   - Cliquez sur "Créer Système Complet"
   - Le système créera automatiquement :
     - GlobalBadgeDatabase.asset
     - GameObject avec tous les composants
     - Configuration des liens entre composants

4. **Vérification**
   - Tous les statuts doivent être verts (✓)
   - La console affiche "Système de badges complet créé et configuré"

### Option 2 : Installation Étape par Étape

Si l'installation automatique ne fonctionne pas :

#### Étape 1 : Créer la Base de Données

1. **Ouvrir l'éditeur de badges**
   ```
   Window → Gameplay → Global Badge System
   ```

2. **Onglet "Base de Données"**
   - Si aucune base n'est détectée, cliquer "Créer une nouvelle GlobalBadgeDatabase"
   - Le fichier sera créé dans `Assets/Scripts/Gameplay/Common/BadgeSystem/`

3. **Ajouter les badges d'exemple**
   - Cliquer "Ajouter Badges d'Exemple"
   - Vérifier que les badges apparaissent dans l'onglet

#### Étape 2 : Créer les Composants

1. **Onglet "Système"**
   - Si "GlobalBadgeSystem non trouvé", cliquer "Créer GlobalBadgeSystem"

2. **Onglet "Tracking"**
   - Si "GlobalBadgeTracker non trouvé", cliquer "Créer GlobalBadgeTracker"

3. **Onglet "Joueurs"**
   - Si "GlobalPlayerBadgeStorage non trouvé", cliquer "Créer GlobalPlayerBadgeStorage"

#### Étape 3 : Configuration des Liens

1. **Retourner à l'onglet "Système"**
   - Assigner la "Badge Database" créée à l'étape 1
   - Assigner le "Player Storage" créé à l'étape 2

2. **Onglet "Tracking"**
   - Assigner le "Badge System" dans la configuration

3. **Sauvegarder la scène**

## Installation Manuelle (Avancée)

### Méthode Script

Si vous préférez créer le système par script :

```csharp
// 1. Créer la base de données
var database = ScriptableObject.CreateInstance<GlobalBadgeDatabase>();
UnityEditor.AssetDatabase.CreateAsset(database, 
    "Assets/Scripts/Gameplay/Common/BadgeSystem/GlobalBadgeDatabase.asset");
database.CreateDefaultBadges();

// 2. Créer les GameObjects
var systemRoot = new GameObject("Badge System Global");

var storage = systemRoot.AddComponent<GlobalPlayerBadgeStorage>();
var badgeSystem = systemRoot.AddComponent<GlobalBadgeSystem>();
var tracker = systemRoot.AddComponent<GlobalBadgeTracker>();

// 3. Configurer les liens
UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(badgeSystem);
so.FindProperty("badgeDatabase").objectReferenceValue = database;
so.FindProperty("playerStorage").objectReferenceValue = storage;
so.ApplyModifiedProperties();

so = new UnityEditor.SerializedObject(tracker);
so.FindProperty("badgeSystem").objectReferenceValue = badgeSystem;
so.ApplyModifiedProperties();

Debug.Log("Système créé manuellement avec succès");
```

### Méthode Interface Unity

1. **Créer la Database**
   ```
   Clic droit dans Project → Create → [Votre Menu] → Global Badge Database
   ```

2. **Créer un GameObject**
   ```
   Hierarchy → Clic droit → Create Empty → Nommer "Badge System Global"
   ```

3. **Ajouter les Composants**
   ```
   Add Component → Global Player Badge Storage
   Add Component → Global Badge System  
   Add Component → Global Badge Tracker
   ```

4. **Configurer dans l'Inspector**
   - Sur `GlobalBadgeSystem` : assigner la Database et le Storage
   - Sur `GlobalBadgeTracker` : assigner le Badge System

## Configuration des Mini-Jeux

### Firefly Dance

1. **Ajouter l'adaptateur**
   ```
   Dans votre scène Firefly → Add Component → Firefly Badge Adapter
   ```

2. **Configuration automatique**
   - L'adaptateur trouvera automatiquement le système global
   - Activer "Enable Auto Tracking" et "Enable Debug Logs"

3. **Intégration dans le code**
   ```csharp
   // Dans votre FireflyGameManager
   [Header("Badge System")]
   public FireflyBadgeAdapter badgeAdapter;
   
   // Le système s'initialise automatiquement dans InitializeCompleteGame()
   // Tracking automatique des événements de capture et fin de partie
   
   // Utilisation manuelle (optionnelle)
   if (badgeAdapter != null && currentPlayer != null)
   {
       badgeAdapter.UpdatePlayerScore(currentPlayer.Nickname, score);
       badgeAdapter.IncrementFirefliesCollected(currentPlayer.Nickname);
   }
   
   // Méthodes de debug disponibles
   [ContextMenu("Force Badge Validation")]
   public void ForceBadgeValidation() // Déjà implémentée
   
   [ContextMenu("Debug Player Badges")]  
   public void DebugPlayerBadges() // Déjà implémentée
   ```

### Balloon Pop

1. **Ajouter l'adaptateur**
   ```
   Dans votre scène Balloon → Add Component → Balloon Badge Adapter
   ```

2. **Intégration exemple**
   ```csharp
   // Dans votre BalloonGameManager
   private BalloonBadgeAdapter badgeAdapter;
   
   void Start()
   {
       badgeAdapter = GetComponent<BalloonBadgeAdapter>();
   }
   
   public void OnBalloonPopped(string player, BalloonType type)
   {
       badgeAdapter.IncrementBalloonsPopped(player);
       if (type != BalloonType.Normal)
           badgeAdapter.PopSpecialBalloon(player, type.ToString());
   }
   ```

### Music Note Press

1. **Ajouter l'adaptateur**
   ```
   Dans votre scène Music → Add Component → Music Badge Adapter
   ```

2. **Intégration exemple**
   ```csharp
   // Dans votre MusicGameManager
   private MusicBadgeAdapter badgeAdapter;
   
   void Start()
   {
       badgeAdapter = GetComponent<MusicBadgeAdapter>();
   }
   
   public void OnNoteHit(string player, NoteType type, NoteTiming timing)
   {
       badgeAdapter.IncrementNotesHit(player);
       badgeAdapter.HandleSpecialNote(player, type, timing);
   }
   ```

## Configuration Avancée

### Personnalisation des Badges

1. **Ouvrir l'éditeur**
   ```
   Window → Gameplay → Global Badge System → Onglet "Base de Données"
   ```

2. **Créer un nouveau badge**
   - Choisir l'ID du jeu (firefly, balloon, music, ou nouveau)
   - Entrer l'ID du badge
   - Cliquer "Créer Badge"

3. **Modifier les propriétés**
   - Sélectionner la database dans le Project
   - Modifier les badges dans l'Inspector

### Configuration des Performances

```csharp
// Sur GlobalBadgeTracker
public float validationInterval = 0.5f;        // Validation toutes les 0.5s
public bool enableRealTimeValidation = true;   // Validation en temps réel
public bool enableAutoTracking = true;         // Tracking automatique

// Sur GlobalPlayerBadgeStorage  
public bool enablePersistence = true;          // Sauvegarde automatique
public string storageKey = "MotionParty_AllPlayerBadges"; // Clé de sauvegarde
```

### Configuration des Logs

```csharp
// Sur GlobalBadgeSystem
public bool enableDebugLogs = true;            // Logs du système
public bool validateBadgeConditions = true;    // Validation des conditions

// Sur GlobalBadgeTracker
public bool showTrackingLogs = true;           // Logs de tracking
public bool showValidationDetails = false;     // Logs détaillés

// Sur les Adaptateurs
public bool enableDebugLogs = true;            // Logs des adaptateurs
```

## Vérification de l'Installation

### Tests via l'Éditeur

1. **Ouvrir l'éditeur de badges**
   ```
   Window → Gameplay → Global Badge System
   ```

2. **Vérifier le statut**
   - Tous les indicateurs doivent être verts (✓)
   - Aucun message d'erreur dans la console

3. **Test d'intégration**
   - Onglet "Test" → "Test Intégration"
   - La console doit afficher le succès du test

### Tests via Code

```csharp
// Test simple d'attribution
var badgeSystem = GlobalBadgeSystem.Instance;
bool success = badgeSystem.TryEarnBadge("TestPlayer", "firefly", "first_firefly", 1f);
Debug.Log($"Test badge: {(success ? "SUCCÈS" : "ÉCHEC")}");

// Test de tracking
var tracker = GlobalBadgeTracker.Instance;
tracker.UpdateScore("TestPlayer", "firefly", 1000f);
int badges = tracker.ValidatePlayerMetricsForGame("TestPlayer", "firefly");
Debug.Log($"Badges attribués: {badges}");
```

### Tests via Menu Contextuel

Sur chaque adaptateur :
```
Clic droit sur le composant → Test [Jeu] Adapter
Clic droit sur le composant → Simulate Full Game
```

## Problèmes Courants et Solutions

### Problème : L'éditeur n'apparaît pas

**Solution :**
1. Vérifier que `GlobalBadgeSystemEditor.cs` est dans un dossier `Editor`
2. Recompiler les scripts (Assets → Reimport All)
3. Redémarrer Unity si nécessaire

### Problème : Composants non trouvés

**Solution :**
1. Vérifier que tous les scripts sont sans erreur de compilation
2. Utiliser "Refresh" dans l'éditeur de badges
3. Vérifier que les GameObjects sont dans la scène active

### Problème : Badges non attribués

**Solution :**
1. Vérifier que `validateBadgeConditions` est correct
2. Activer `enableDebugLogs` pour voir les détails
3. Utiliser l'attribution forcée pour tester : `ForceEarnBadge()`

### Problème : Données non sauvegardées

**Solution :**
1. Vérifier que `enablePersistence` est activé
2. Vérifier les permissions d'écriture
3. Tester avec `PlayerPrefs` en mode debug

## Migration depuis l'Ancien Système

Si vous migrez depuis l'ancien système FireflyDance :

### Étape 1 : Sauvegarder

```csharp
// Sauvegarder les badges existants
var oldStorage = FindObjectOfType<PlayerBadgeStorage>();
if (oldStorage != null)
{
    // Exporter vers JSON ou autre format
}
```

### Étape 2 : Remplacer

1. Supprimer les anciens composants :
   - `BadgeSystem`
   - `BadgeTracker`  
   - `PlayerBadgeStorage`
   - `BadgeDatabase`

2. Installer le nouveau système selon ce guide

### Étape 3 : Adapter le Code

```csharp
// Ancien code
BadgeSystem.Instance.TryEarnBadge("player", badgeId, value);

// Nouveau code
FireflyBadgeAdapter.UpdateScore("player", value);
// ou
GlobalBadgeSystem.Instance.TryEarnBadge("player", "firefly", badgeId, value);
```

### Étape 4 : Tester

1. Tester chaque mini-jeu individuellement
2. Vérifier la persistence des données
3. Valider l'attribution des badges

## Support et Aide

### Logs de Debug

Activer tous les logs pour diagnostiquer :

```csharp
// Dans la console Unity
GlobalBadgeSystem.Instance.enableDebugLogs = true;
GlobalBadgeTracker.Instance.showTrackingLogs = true;
GlobalBadgeTracker.Instance.showValidationDetails = true;
```

### Commandes de Debug

```csharp
// Forcer la validation
GlobalBadgeTracker.Instance.ValidateAllPlayers();

// Afficher les statistiques
GlobalPlayerBadgeStorage.Instance.ShowGlobalBadgeStatistics();

// Tester un badge
GlobalBadgeSystem.Instance.TestBadgeAttribution();
```

### Reset Complet

En cas de problème majeur :

```csharp
// ATTENTION : Efface toutes les données !
GlobalPlayerBadgeStorage.Instance.ClearAllBadgeData();
```

---

**Installation terminée !** 

Le système de badges global est maintenant prêt à être utilisé dans tous vos mini-jeux Motion-Party.

Pour utiliser le système, consultez le fichier `README.md` principal pour les exemples d'utilisation détaillés.
