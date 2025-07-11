# Système de Badges Global pour Motion-Party

## Vue d'ensemble

Le système de badges global permet d'attribuer des badges aux joueurs de tous les mini-jeux de Motion-Party. Il remplace le système précédent spécifique à FireflyDance par une architecture universelle et extensible.

## Architecture du Système

### Structure des Fichiers

```
Assets/Scripts/Gameplay/Common/BadgeSystem/
├── Core/
│   ├── BadgeDefinition.cs          # Définition des badges
│   ├── BadgeInstance.cs            # Instance de badge obtenu
│   ├── GlobalBadgeDatabase.cs      # Base de données globale
│   ├── GlobalBadgeSystem.cs        # Système principal
│   ├── GlobalBadgeTracker.cs       # Système de tracking
│   └── GlobalPlayerBadgeStorage.cs # Stockage des joueurs
├── MiniGames/
│   ├── FireflyBadgeAdapter.cs      # Adaptateur Firefly
│   ├── BalloonBadgeAdapter.cs      # Adaptateur Balloon
│   └── MusicBadgeAdapter.cs        # Adaptateur Music
└── Editor/
    └── GlobalBadgeSystemEditor.cs  # Interface d'édition
```

### Composants Principaux

#### 1. GlobalBadgeDatabase
- **Rôle** : Stocke toutes les définitions de badges pour tous les jeux
- **Type** : ScriptableObject
- **Fonctionnalités** :
  - Gestion multi-jeux
  - Filtrage par jeu
  - Validation des données
  - Badges d'exemple inclus

#### 2. GlobalBadgeSystem
- **Rôle** : Gère l'attribution des badges
- **Type** : MonoBehaviour (Singleton)
- **Fonctionnalités** :
  - Attribution conditionnelle
  - Validation des prérequis
  - Gestion des événements
  - Attribution forcée (debug)

#### 3. GlobalBadgeTracker
- **Rôle** : Suit les performances des joueurs
- **Type** : MonoBehaviour (Singleton)
- **Fonctionnalités** :
  - Tracking en temps réel
  - Métriques personnalisables
  - Validation automatique
  - Cooldown anti-spam

#### 4. GlobalPlayerBadgeStorage
- **Rôle** : Stocke les badges obtenus par tous les joueurs
- **Type** : MonoBehaviour (Singleton)
- **Fonctionnalités** :
  - Persistance automatique
  - Multi-joueurs
  - Statistiques globales
  - Sauvegarde locale

## Mini-Jeux Supportés

### 1. Firefly Dance (`firefly`)
**Badges disponibles :**
- `first_firefly` - Premier capture
- `score_master` - Score élevé (1000 points)
- `speedster` - Temps rapide (< 60s)
- `collector` - Collecte massive (25 lucioles)
- `perfectionist` - Précision parfaite (95%)
- `combo_master` - Combo élevé (20+)
- `endurance_master` - Longue durée (5 min)
- `score_legend` - Score légendaire (2000 points)

**Métriques trackées :**
- `score` - Score total
- `time` - Temps de jeu
- `collected` - Lucioles collectées
- `accuracy` - Précision
- `combo` - Combo actuel
- `maxcombo` - Combo maximum
- `perfect` - Lucioles parfaites
- `duration` - Durée totale

### 2. Balloon Pop (`balloon`)
**Badges disponibles :**
- `first_pop` - Premier ballon éclaté
- `balloon_master` - Maître des ballons (5000 points)
- `lightning_hands` - Mains rapides (10 ballons/s)
- `sharpshooter` - Tireur d'élite (95% précision)

**Métriques trackées :**
- `score` - Score total
- `time` - Temps de jeu
- `popped` - Ballons éclatés
- `accuracy` - Précision
- `maxspeed` - Vitesse maximum
- `maxchain` - Chaîne maximum
- `special_*` - Ballons spéciaux
- `missed` - Ballons manqués

### 3. Music Note Press (`music`)
**Badges disponibles :**
- `first_note` - Première note
- `rhythm_legend` - Légende du rythme (100k points)
- `perfect_pitch` - Timing parfait (98% précision)
- `combo_king` - Roi du combo (500+ combo)

**Métriques trackées :**
- `score` - Score total
- `time` - Durée de chanson
- `notes_hit` - Notes réussies
- `notes_missed` - Notes manquées
- `accuracy` - Précision
- `max_combo` - Combo maximum
- `perfect_notes` - Notes parfaites
- `max_bpm` - BPM maximum
- `max_difficulty` - Difficulté maximum

## Installation et Configuration

### 1. Installation Automatique

Le système peut être créé automatiquement via l'éditeur :

1. Ouvrir `Window → Gameplay → Global Badge System`
2. Cliquer sur "Créer Système Complet"
3. Le système sera configuré automatiquement

### 2. Installation Manuelle

#### Étape 1 : Base de Données
```csharp
// Créer une instance de GlobalBadgeDatabase
var database = CreateInstance<GlobalBadgeDatabase>();
AssetDatabase.CreateAsset(database, "Assets/BadgeDatabase.asset");
database.CreateDefaultBadges(); // Ajoute les badges d'exemple
```

#### Étape 2 : Composants de Scène
```csharp
// Dans une scène persistante, ajouter :
var systemGO = new GameObject("Badge System");
systemGO.AddComponent<GlobalPlayerBadgeStorage>();
systemGO.AddComponent<GlobalBadgeSystem>();
systemGO.AddComponent<GlobalBadgeTracker>();
```

#### Étape 3 : Configuration
```csharp
// Lier la base de données au système
var badgeSystem = FindObjectOfType<GlobalBadgeSystem>();
badgeSystem.badgeDatabase = database;
```

## Utilisation dans les Mini-Jeux

### Méthode 1 : Adaptateurs (Recommandée)

Chaque mini-jeu dispose d'un adaptateur qui simplifie l'intégration :

#### Firefly Dance
```csharp
// Ajouter FireflyBadgeAdapter à votre scène
var adapter = GetComponent<FireflyBadgeAdapter>();

// Utilisation simple
adapter.UpdatePlayerScore("Player1", 1500f);
adapter.IncrementFirefliesCollected("Player1", 5);
adapter.UpdatePlayerAccuracy("Player1", 92f);

// Ou utilisation statique
FireflyBadgeAdapter.UpdateScore("Player1", 1500f);
FireflyBadgeAdapter.CollectFirefly("Player1");
```

#### Balloon Pop
```csharp
// Ajouter BalloonBadgeAdapter à votre scène
var adapter = GetComponent<BalloonBadgeAdapter>();

adapter.UpdatePlayerScore("Player1", 3500f);
adapter.IncrementBalloonsPopped("Player1", 10);
adapter.RegisterChainPop("Player1", 8);

// Utilisation statique
BalloonBadgeAdapter.UpdateScore("Player1", 3500f);
BalloonBadgeAdapter.PopBalloon("Player1", BalloonType.Golden);
```

#### Music Note Press
```csharp
// Ajouter MusicBadgeAdapter à votre scène
var adapter = GetComponent<MusicBadgeAdapter>();

adapter.UpdatePlayerScore("Player1", 85000f);
adapter.IncrementNotesHit("Player1", 50);
adapter.UpdateCombo("Player1", 100, 150);

// Utilisation statique
MusicBadgeAdapter.UpdateScore("Player1", 85000f);
MusicBadgeAdapter.HitNote("Player1", NoteType.Perfect, NoteTiming.Perfect);
```

### Méthode 2 : Système Global Direct

Pour un contrôle plus fin ou de nouveaux mini-jeux :

```csharp
// Obtenir les références
var badgeSystem = GlobalBadgeSystem.Instance;
var badgeTracker = GlobalBadgeTracker.Instance;

// Tracking manuel
badgeTracker.UpdateScore("Player1", "nouveauJeu", 1000f);
badgeTracker.UpdateMetric("Player1", "nouveauJeu", "custom_metric", 50f);

// Attribution manuelle
badgeSystem.TryEarnBadge("Player1", "nouveauJeu", "badge_id", 100f);

// Attribution forcée (debug)
badgeSystem.ForceEarnBadge("Player1", "nouveauJeu", "badge_id");
```

## Exemples d'Intégration

### Intégration avec Événements de Jeu

```csharp
public class FireflyGameManager : MonoBehaviour
{
    private FireflyBadgeAdapter badgeAdapter;
    
    void Start()
    {
        badgeAdapter = GetComponent<FireflyBadgeAdapter>();
        
        // S'abonner aux événements du jeu
        FireflyEvents.OnScoreChanged += OnScoreChanged;
        FireflyEvents.OnFireflyCollected += OnFireflyCollected;
        FireflyEvents.OnGameCompleted += OnGameCompleted;
    }
    
    private void OnScoreChanged(string player, float score)
    {
        badgeAdapter.UpdatePlayerScore(player, score);
    }
    
    private void OnFireflyCollected(string player)
    {
        badgeAdapter.IncrementFirefliesCollected(player);
    }
    
    private void OnGameCompleted(string player, FireflyGameResult result)
    {
        badgeAdapter.UpdateGameResult(player, result);
    }
}
```

### Mise à jour en Lot

```csharp
// Après une partie complète
var gameResult = new FireflyBadgeAdapter.FireflyGameResult(
    finalScore: 1800f,
    gameTime: 45f,
    firefliesCollected: 20,
    accuracy: 95f,
    maxCombo: 25,
    perfectFireflies: 15,
    totalDuration: 60f
);

badgeAdapter.UpdateGameResult("Player1", gameResult);
```

## Ajout de Nouveaux Mini-Jeux

### 1. Créer les Badges

```csharp
// Dans GlobalBadgeDatabase, ajouter :
var newGameBadge = new BadgeDefinition
{
    MiniGameId = "nouveauJeu",
    BadgeId = "premier_badge",
    BadgeName = "Premier Badge",
    Description = "Obtenir le premier badge",
    Category = BadgeCategory.Achievement,
    Rarity = BadgeRarity.Common,
    TargetValue = 1f,
    IsRepeatable = false
};

database.AddBadge(newGameBadge);
```

### 2. Créer un Adaptateur

```csharp
public class NouveauJeuBadgeAdapter : MonoBehaviour
{
    private const string GAME_ID = "nouveauJeu";
    private GlobalBadgeSystem globalBadgeSystem;
    private GlobalBadgeTracker globalBadgeTracker;
    
    void Start()
    {
        globalBadgeSystem = GlobalBadgeSystem.Instance;
        globalBadgeTracker = GlobalBadgeTracker.Instance;
    }
    
    public void UpdateCustomMetric(string playerName, float value)
    {
        globalBadgeTracker?.UpdateMetric(playerName, GAME_ID, "custom_metric", value);
    }
    
    public void AwardCustomBadge(string playerName, string badgeId)
    {
        globalBadgeSystem?.TryEarnBadge(playerName, GAME_ID, badgeId);
    }
}
```

### 3. Intégration

```csharp
// Dans votre nouveau mini-jeu
public class NouveauJeuManager : MonoBehaviour
{
    private NouveauJeuBadgeAdapter badgeAdapter;
    
    void Start()
    {
        badgeAdapter = GetComponent<NouveauJeuBadgeAdapter>();
    }
    
    public void OnJoueurAction(string player, float value)
    {
        badgeAdapter.UpdateCustomMetric(player, value);
        
        // Validation immédiate si nécessaire
        badgeAdapter.ValidatePlayerBadges(player);
    }
}
```

## Interface d'Administration

### Éditeur Global

Accès via `Window → Gameplay → Global Badge System`

**Fonctionnalités :**
- Vue d'ensemble du système
- Gestion de la base de données
- Configuration des composants
- Statistiques des joueurs
- Outils de test et debug

**Onglets disponibles :**
1. **Base de Données** - Gestion des badges
2. **Système** - Configuration du système principal
3. **Tracking** - Configuration du tracking
4. **Joueurs** - Statistiques des joueurs
5. **Test** - Outils de test et debug

### Tests et Debug

```csharp
// Tests via l'éditeur
[ContextMenu("Test Badge Attribution")]
public void TestBadgeAttribution()
{
    var badgeSystem = GlobalBadgeSystem.Instance;
    badgeSystem.TryEarnBadge("TestPlayer", "firefly", "first_firefly", 1f);
}

// Statistiques
[ContextMenu("Show Statistics")]
public void ShowStatistics()
{
    var storage = GlobalPlayerBadgeStorage.Instance;
    var stats = storage.GetGlobalStatistics();
    Debug.Log($"Joueurs: {stats["totalPlayers"]}, Badges: {stats["totalBadges"]}");
}
```

## Persistance et Sauvegarde

### Sauvegarde Automatique

Le système sauvegarde automatiquement :
- À la fermeture de l'application
- Lors de la mise en pause
- Quand la fenêtre perd le focus

### Gestion Manuelle

```csharp
var storage = GlobalPlayerBadgeStorage.Instance;

// Effacer les données d'un joueur
storage.ClearPlayerBadges("Player1");

// Effacer les données d'un jeu
storage.ClearPlayerBadgesForGame("Player1", "firefly");

// Effacer toutes les données (attention !)
storage.ClearAllBadgeData();
```

## Affichage des Badges dans l'UI

### Utilisation Simple via BadgeUIHelper

```csharp
// Obtenir tous les badges d'un joueur
var badges = BadgeUIHelper.GetPlayerBadgesForUI("Player1");
foreach (var badge in badges)
{
    Debug.Log($"{badge.BadgeName}: {badge.Description} ({badge.GameName})");
}

// Obtenir les badges d'un jeu spécifique
var fireflyBadges = BadgeUIHelper.GetPlayerBadgesForGame("Player1", "firefly");

// Obtenir les statistiques
var stats = BadgeUIHelper.GetPlayerBadgeStats("Player1");
Debug.Log($"Total: {stats.TotalBadges}, Firefly: {stats.FireflyBadges}");

// Vérifier si un joueur a un badge
bool hasFirstFirefly = BadgeUIHelper.HasPlayerBadge("Player1", "firefly", "first_firefly");

// Debug : afficher tous les badges d'un joueur
BadgeUIHelper.DebugShowPlayerBadges("Player1");
```

### Intégration UI Unity

```csharp
public class BadgeDisplayUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform badgeContainer;
    public GameObject badgePrefab;
    public Text playerNameText;
    public Text totalBadgesText;
    
    public void DisplayPlayerBadges(string playerName)
    {
        // Nettoyer l'affichage précédent
        foreach (Transform child in badgeContainer)
            Destroy(child.gameObject);
            
        // Obtenir les badges
        var badges = BadgeUIHelper.GetPlayerBadgesForUI(playerName);
        var stats = BadgeUIHelper.GetPlayerBadgeStats(playerName);
        
        // Mettre à jour l'UI
        playerNameText.text = playerName;
        totalBadgesText.text = $"Badges: {stats.TotalBadges}";
        
        // Créer les éléments de badge
        foreach (var badge in badges)
        {
            var badgeUI = Instantiate(badgePrefab, badgeContainer);
            var badgeComponent = badgeUI.GetComponent<BadgeUIElement>();
            badgeComponent.SetBadgeData(badge);
        }
    }
}

public class BadgeUIElement : MonoBehaviour
{
    [Header("Badge UI")]
    public Image badgeIcon;
    public Text badgeName;
    public Text badgeDescription;
    public Text gameSource;
    public Text earnedDate;
    
    public void SetBadgeData(BadgeDisplayInfo badgeData)
    {
        badgeName.text = badgeData.BadgeName;
        badgeDescription.text = badgeData.Description;
        gameSource.text = badgeData.GameName;
        earnedDate.text = $"Obtenu le {badgeData.EarnedDate}";
        
        if (badgeData.Icon != null)
            badgeIcon.sprite = badgeData.Icon;
    }
}
```

## Diagnostic et Debug

### Méthodes de Debug Disponibles

```csharp
// Dans FireflyDanceGameManager (menu contextuel)
[ContextMenu("Test Badge System Complete")]
public void TestBadgeSystemComplete() // Déjà implémentée

[ContextMenu("Quick Badge Diagnostic")]  
public void QuickBadgeDiagnostic() // Déjà implémentée

// Dans FireflyBadgeAdapter (menu contextuel)
[ContextMenu("Diagnostic Firefly Adapter")]
public void DiagnosticAdapter() // Déjà implémentée

[ContextMenu("Test Complete Firefly Badge System")]
public void TestCompleteBadgeSystem() // Déjà implémentée

[ContextMenu("Force First Firefly Badge")]
public void ForceFirstFireflyBadge() // Déjà implémentée

// Dans GlobalPlayerBadgeStorage (menu contextuel)
[ContextMenu("Diagnostic Player")]
public void DiagnosticPlayer() // Déjà implémentée
```

### Debug Step-by-Step

1. **Vérifier les composants** :
   ```csharp
   // Clic droit sur FireflyDanceGameManager → "Quick Badge Diagnostic"
   ```

2. **Tester l'attribution** :
   ```csharp
   // Clic droit sur FireflyBadgeAdapter → "Test Complete Firefly Badge System"
   ```

3. **Forcer un badge** :
   ```csharp
   // Clic droit sur FireflyBadgeAdapter → "Force First Firefly Badge"
   ```

4. **Afficher les badges** :
   ```csharp
   BadgeUIHelper.DebugShowPlayerBadges("PlayerName");
   ```

### Problèmes Courants et Solutions

**❌ Problème : "Aucun badge attribué après capture"**
```csharp
// Solutions :
1. Vérifier que GlobalBadgeDatabase contient les badges Firefly
2. Vérifier que FireflyBadgeAdapter est configuré avec enableAutoTracking=true
3. Utiliser "Quick Badge Diagnostic" pour vérifier les composants
4. Tester avec "Force First Firefly Badge"
```

**❌ Problème : "L'éditeur de badges affiche 'Joueurs' vide"**
```csharp
// Solutions :
1. Vérifier que les badges sont bien sauvegardés avec enablePersistence=true
2. Rafraîchir l'éditeur avec le bouton "Refresh"
3. Utiliser BadgeUIHelper.DebugShowPlayerBadges() dans la console
4. Vérifier les PlayerPrefs avec la clé de sauvegarde
```

**❌ Problème : "Badge 'first_firefly' non trouvé"**
```csharp
// Solutions :
1. Vérifier que la database a été initialisée avec CreateDefaultBadges()
2. Utiliser l'éditeur pour ajouter manuellement les badges d'exemple
3. Vérifier que le badge ID est exactement "first_firefly"
4. Utiliser "Diagnostic Firefly Adapter" pour voir les badges disponibles
```
{
    Debug.Log($"Métrique mise à jour: {player}.{game}.{metric} = {value}");
}
```

## Optimisation et Performance

### Bonnes Pratiques

1. **Validation Groupée** : Valider les badges après plusieurs actions
2. **Cooldown** : Le système inclut un cooldown anti-spam
3. **Cache** : Les données sont mises en cache pour un accès rapide
4. **Lazy Loading** : Les composants se trouvent automatiquement

### Configuration Performance

```csharp
// Dans GlobalBadgeTracker
public float validationInterval = 0.5f; // Intervalle de validation (secondes)
public bool enableRealTimeValidation = true; // Validation en temps réel
public bool enableAutoTracking = true; // Tracking automatique
```

## Dépannage

### Problèmes Courants

**Q: Les badges ne s'attribuent pas**
R: Vérifiez que tous les composants sont présents et configurés

**Q: L'éditeur n'apparaît pas dans le menu**
R: Assurez-vous que le script est dans un dossier `Editor`

**Q: Les données ne se sauvegardent pas**
R: Vérifiez que `enablePersistence` est activé dans `GlobalPlayerBadgeStorage`

### Debug

```csharp
// Activer les logs détaillés
var badgeSystem = GlobalBadgeSystem.Instance;
badgeSystem.enableDebugLogs = true;

var badgeTracker = GlobalBadgeTracker.Instance;
badgeTracker.showTrackingLogs = true;
badgeTracker.showValidationDetails = true;
```

## Migration depuis l'Ancien Système

Si vous utilisiez l'ancien système FireflyDance spécifique :

1. **Sauvegarder** les données existantes
2. **Remplacer** les anciens composants par les nouveaux
3. **Migrer** les badges existants vers `GlobalBadgeDatabase`
4. **Adapter** le code pour utiliser `FireflyBadgeAdapter`
5. **Tester** l'intégration complète

Le nouveau système est conçu pour être rétrocompatible et plus flexible.

---

## Support et Contribution

Pour ajouter de nouveaux mini-jeux ou modifier le système :

1. Créer un nouvel adaptateur basé sur les exemples existants
2. Ajouter les badges spécifiques dans `GlobalBadgeDatabase`
3. Documenter les nouvelles métriques et badges
4. Tester l'intégration complète

Le système est conçu pour être extensible et maintenir la cohérence entre tous les mini-jeux de Motion-Party.
