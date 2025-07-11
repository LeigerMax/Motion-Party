# Système de Sélection de Joueurs / Équipe Maison de Repos

## 📋 Vue d'ensemble

Ce système permet de créer, gérer et sélectionner des joueurs pour les parties de Motion Party. Il est conçu spécifiquement pour les équipes de maisons de repos, permettant une gestion fluide des profils de joueurs et une sélection intuitive avant chaque partie.

## 🧩 Fonctionnalités

### ✅ Gestion des joueurs
- **Création de profils** : Ajout de joueurs avec prénom/pseudo uniquement
- **Équipes maison de repos** : Chaque joueur appartient à une équipe
- **Stockage persistant** : Données sauvegardées localement (JSON ou PlayerPrefs)
- **Interface scrollable** : Gestion fluide de 20+ joueurs
- **Statistiques** : Suivi des parties jouées et scores

### ✅ Sélection des joueurs
- **Interface intuitive** : Sélection avant le lancement des parties
- **Modes flexibles** : 1, 2 ou plus de joueurs par partie
- **Workflow complet** : Menu Principal → Équipe → Jouer → Sélection → Lancement

### ✅ Intégration
- **Architecture modulaire** : Respect des conventions du projet
- **Système d'événements** : Communication entre composants
- **MiniGameBase** : Compatible avec tous les mini-jeux existants

## 🔧 Architecture

### Structure des dossiers
```
Assets/Scripts/
├── Systems/
│   ├── PlayerData.cs              # Données d'un joueur
│   ├── Team.cs                    # Données d'une équipe
│   ├── PlayerProfileManager.cs    # Gestionnaire principal
│   ├── TeamManager.cs             # Gestionnaire d'équipes
│   └── GamePlayerSelector.cs      # Sélection pour les parties
└── UI/
    ├── PlayerSelection/
    │   ├── PlayerSelectionUI.cs       # Interface de sélection
    │   └── PlayerSelectionItem.cs     # Élément de liste
    └── PlayerManagement/
        ├── PlayerManagementUI.cs      # Interface de gestion
        └── PlayerManagementItem.cs    # Élément de gestion
```

### Composants principaux

#### 1. **PlayerData** - Données d'un joueur
```csharp
public class PlayerData
{
    public string Id { get; }           // ID unique
    public string Nickname { get; set; } // Prénom/pseudo
    public string TeamName { get; set; } // Équipe
    public int GamesPlayed { get; set; } // Nombre de parties
    public int TotalScore { get; set; }  // Score total
    public bool IsActive { get; set; }   // Actif/inactif
}
```

#### 2. **PlayerProfileManager** - Gestionnaire principal
```csharp
public class PlayerProfileManager : MonoBehaviour
{
    // Gestion des joueurs
    public PlayerData CreatePlayer(string nickname, string teamName = "");
    public bool RemovePlayer(string playerId);
    public List<PlayerData> GetAllPlayers();
    
    // Gestion des équipes
    public Team CreateTeam(string teamName);
    public List<Team> GetAllTeams();
    
    // Persistance
    private void SaveData();
    private void LoadData();
}
```

#### 3. **GamePlayerSelector** - Sélection pour les parties
```csharp
public class GamePlayerSelector : MonoBehaviour
{
    // Sélection des joueurs
    public void SelectPlayers(List<PlayerData> players);
    public PlayerData CurrentPlayer { get; }
    
    // Contrôle de partie
    public bool StartGame();
    public void EndGame();
    public PlayerData NextPlayer();
    
    // Scoring
    public void AddScoreToCurrentPlayer(int points);
}
```

## 📱 Interfaces utilisateur

### Interface de gestion des joueurs
- **Création** : Formulaire simple (prénom + équipe)
- **Édition** : Modification des informations
- **Suppression** : Avec confirmation
- **Recherche** : Filtrage par nom ou équipe
- **Vue liste** : Affichage scrollable avec statistiques

### Interface de sélection
- **Sélection multiple** : Coches pour chaque joueur
- **Compteur** : Nombre de joueurs sélectionnés
- **Validation** : Nombre min/max configurable
- **Recherche** : Filtrage rapide

## 🚀 Intégration dans le projet

### 1. Ajout au menu principal
Le bouton "Équipe" du menu principal ouvre l'interface de gestion des joueurs :

```csharp
// Dans MainMenuController.cs
private void OnTeamButtonClicked()
{
    ShowPlayerManagement();
}
```

### 2. Workflow de partie
```
Menu Principal → Jouer → Sélection des joueurs → Lancement
```

### 3. Intégration avec MiniGameBase
```csharp
// Dans GameSelectionController.cs
private void LaunchMiniGame()
{
    var gamePlayerSelector = GamePlayerSelector.Instance;
    gamePlayerSelector.StartGame();
    
    // Lancer le mini-jeu existant
    miniGame.StartMiniGame(() => {
        gamePlayerSelector.EndGame();
        // Retour au menu
    });
}
```

## 🛠️ Guide d'implémentation Unity

### Étape 1 : Configuration des GameObjects principaux

#### A. Dans la scène Menu Principal
Créer la hiérarchie suivante :

```
Main Menu Scene
├── 📁 Canvas
│   ├── 📁 MainMenuPanel
│   │   ├── PlayButton
│   │   ├── TeamButton ← IMPORTANT : Connecter à OnTeamButtonClicked()
│   │   ├── OptionsButton
│   │   └── QuitButton
│   ├── 📁 GameSelectionPanel
│   │   ├── NewLocalGameButton ← IMPORTANT : Connecter à OnNewLocalGameClicked()
│   │   ├── MultiplayerButton
│   │   ├── LevelSelectButton
│   │   └── BackButton
│   └── 📁 PlayerManagementPanel (nouveau)
│       ├── 📁 Header
│       │   ├── TitleText ("Gestion des Joueurs")
│       │   └── CloseButton
│       ├── 📁 PlayerList
│       │   ├── ScrollView
│       │   │   └── Content (Transform + ContentSizeFitter)
│       │   └── NoPlayersText ("Aucun joueur créé")
│       └── 📁 Controls
│           ├── AddPlayerButton
│           ├── SearchInputField
│           └── TeamFilter (Dropdown)
├── 📁 Managers
│   ├── 🎮 MainMenuController ← Script existant, modifier
│   ├── 🎮 GameSelectionController ← Script existant, modifier
│   ├── 🎮 PlayerProfileManager ← NOUVEAU script singleton
│   ├── 🎮 TeamManager ← NOUVEAU script singleton
│   └── 🎮 GamePlayerSelector ← NOUVEAU script singleton
└── 📁 PlayerSelectionDialog (nouveau)
    ├── 📁 Background (Image semi-transparente)
    ├── 📁 Dialog
    │   ├── 📁 Header
    │   │   ├── TitleText ("Sélection des Joueurs")
    │   │   └── CloseButton
    │   ├── 📁 PlayerList
    │   │   ├── ScrollView
    │   │   │   └── Content
    │   │   └── SearchInputField
    │   └── 📁 Footer
    │       ├── SelectedCountText ("0/4 joueurs sélectionnés")
    │       ├── CancelButton
    │       └── StartGameButton
```

#### B. Scripts à assigner

**🎮 MainMenuController.cs**
```csharp
[Header("Player Management")]
[SerializeField] private GameObject playerManagementPanel;
[SerializeField] private UI.PlayerManagement.PlayerManagementUI playerManagementUI;
```

**🎮 GameSelectionController.cs**
```csharp
[Header("Player Selection")]
[SerializeField] private GameObject playerSelectionDialog;
[SerializeField] private UI.PlayerSelection.PlayerSelectionUI playerSelectionUI;
```

### Étape 2 : Création des Prefabs

#### A. Prefab PlayerManagementItem
```
PlayerManagementItem (Prefab)
├── 📁 Background (Image)
├── 📁 PlayerInfo
│   ├── 📁 Avatar (Image - optionnel)
│   ├── 📁 TextInfo
│   │   ├── NameText
│   │   ├── TeamText
│   │   └── StatsText
│   └── 📁 StatusIndicator (Image)
└── 📁 Actions
    ├── EditButton
    └── DeleteButton
```

#### B. Prefab PlayerSelectionItem
```
PlayerSelectionItem (Prefab)
├── 📁 Background (Image)
├── 📁 Toggle
│   └── Checkmark
├── 📁 PlayerInfo
│   ├── 📁 Avatar (Image - optionnel)
│   └── 📁 TextInfo
│       ├── NameText
│       └── TeamText
└── 📁 SelectionIndicator (Image)
```

### Étape 3 : Configuration des Managers

#### A. PlayerProfileManager
```csharp
// Créer un GameObject vide "PlayerProfileManager"
// Assigner le script PlayerProfileManager.cs
[Header("Settings")]
[SerializeField] private bool usePlayerPrefs = false;
[SerializeField] private bool debugMode = false;
[SerializeField] private int maxPlayers = 50;
```

#### B. TeamManager
```csharp
// Créer un GameObject vide "TeamManager"
// Assigner le script TeamManager.cs
[Header("Default Teams")]
[SerializeField] private string[] defaultTeamNames = {
    "Les Roses",
    "Les Violettes", 
    "Les Marguerites",
    "Les Tulipes"
};
```

#### C. GamePlayerSelector
```csharp
// Créer un GameObject vide "GamePlayerSelector"
// Assigner le script GamePlayerSelector.cs
[Header("Game Settings")]
[SerializeField] private bool debugMode = false;
[SerializeField] private int maxSelectedPlayers = 4;
[SerializeField] private int minSelectedPlayers = 1;
```

### Étape 4 : Configuration des UI

#### A. PlayerManagementUI
```csharp
// Sur le GameObject PlayerManagementPanel
[Header("UI References")]
[SerializeField] private Transform playerListContent;
[SerializeField] private GameObject playerItemPrefab;
[SerializeField] private Button addPlayerButton;
[SerializeField] private InputField searchInputField;
[SerializeField] private Dropdown teamFilterDropdown;
[SerializeField] private Text noPlayersText;
```

#### B. PlayerSelectionUI
```csharp
// Sur le GameObject PlayerSelectionDialog
[Header("UI References")]
[SerializeField] private Transform playerListContent;
[SerializeField] private GameObject playerSelectionItemPrefab;
[SerializeField] private InputField searchInputField;
[SerializeField] private Text selectedCountText;
[SerializeField] private Button startGameButton;
[SerializeField] private Button cancelButton;
[SerializeField] private int maxSelectedPlayers = 4;
```

### Étape 5 : Connexion des événements

#### A. Boutons du Menu Principal
```csharp
// Dans MainMenuController.cs - méthode InitializeButtons()
if (teamButton != null)
    teamButton.onClick.AddListener(OnTeamButtonClicked);
```

#### B. Boutons de sélection de jeu
```csharp
// Dans GameSelectionController.cs - méthode InitializeButtons()
if (newLocalGameButton != null)
    newLocalGameButton.onClick.AddListener(OnNewLocalGameClicked);
```

### Étape 6 : Test et validation

#### A. Vérifications essentielles
1. **Managers présents** : PlayerProfileManager, TeamManager, GamePlayerSelector
2. **Références UI** : Tous les champs [SerializeField] assignés
3. **Prefabs** : PlayerManagementItem et PlayerSelectionItem créés
4. **Événements** : Boutons connectés aux bonnes méthodes

#### B. Tests fonctionnels
1. **Création de joueur** : Menu → Équipe → Ajouter un joueur
2. **Sélection** : Menu → Jouer → Sélectionner des joueurs
3. **Persistence** : Redémarrer Unity, vérifier que les données persistent

### Étape 7 : Personnalisation (optionnel)

#### A. Couleurs et thèmes
```csharp
// Dans PlayerManagementItem.cs
[Header("Visual Configuration")]
[SerializeField] private Color activeColor = Color.green;
[SerializeField] private Color inactiveColor = Color.red;
[SerializeField] private Color normalBackgroundColor = Color.white;
[SerializeField] private Color hoverBackgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);
```

#### B. Animations
Les animations sont déjà intégrées avec des coroutines natives Unity, aucune configuration supplémentaire nécessaire.

### ⚠️ Points importants

1. **Une seule scène** : Tout le système fonctionne dans la scène Menu Principal
2. **Persistance automatique** : Les données sont sauvegardées automatiquement
3. **Singleton pattern** : Les managers sont des singletons, pas besoin de multiples instances
4. **Événements** : Le système utilise des événements Unity pour la communication
5. **Modularité** : Chaque composant peut être activé/désactivé indépendamment

---

## 💾 Persistance des données

### Options de stockage
- **Fichiers JSON** (par défaut) : `Application.persistentDataPath/players.json`
- **PlayerPrefs** : Alternative simple
- **Configuration** : Changeable via l'inspecteur

### Structure des données
```json
{
  "players": [
    {
      "id": "guid-unique",
      "nickname": "Marie",
      "teamName": "Maison des Roses",
      "gamesPlayed": 5,
      "totalScore": 150,
      "isActive": true,
      "createdAt": "2025-01-11T10:00:00"
    }
  ],
  "teams": [
    {
      "id": "team-guid",
      "name": "Maison des Roses",
      "playerIds": ["guid-unique"],
      "isActive": true
    }
  ]
}
```

## 🎮 Utilisation dans les mini-jeux

### Accès au joueur actuel
```csharp
public class MonMiniJeu : MiniGameBase
{
    protected override void Launch()
    {
        var currentPlayer = GamePlayerSelector.Instance.CurrentPlayer;
        Debug.Log($"Partie de {currentPlayer.Nickname}");
    }
    
    private void OnScoreEarned(int points)
    {
        GamePlayerSelector.Instance.AddScoreToCurrentPlayer(points);
    }
}
```

### Gestion des tours multiples
```csharp
// Passer au joueur suivant
var nextPlayer = GamePlayerSelector.Instance.NextPlayer();
Debug.Log($"Au tour de {nextPlayer.Nickname}");
```

## 🔧 Configuration

### Dans Unity Inspector

#### PlayerProfileManager
- **Use Player Prefs** : Utiliser PlayerPrefs au lieu de JSON
- **Debug Mode** : Afficher les logs détaillés
- **Max Players** : Nombre maximum de joueurs (50 par défaut)

#### PlayerSelectionUI
- **Max Selected Players** : Nombre maximum de joueurs sélectionnables
- **Min Selected Players** : Nombre minimum requis
- **Allow Multiple Selection** : Autoriser la sélection multiple

#### GamePlayerSelector
- **Debug Mode** : Afficher les informations de debug

## 🎯 Workflow d'utilisation

### 1. Création d'équipe et de joueurs
```
Menu Principal → Équipe → Ajouter un joueur
- Saisir le prénom (ex: "Marie")
- Sélectionner/créer une équipe (ex: "Maison des Roses")
- Confirmer
```

### 2. Lancement d'une partie
```
Menu Principal → Jouer → Partie locale
- Sélectionner les joueurs participants
- Confirmer la sélection
- Le jeu se lance automatiquement
```

### 3. Pendant la partie
- Le système gère automatiquement les tours
- Les scores sont enregistrés par joueur
- Les statistiques sont mises à jour

## 🛠️ Personnalisation

### Ajout de nouveaux champs
Pour ajouter des informations aux joueurs :

```csharp
// Dans PlayerData.cs
[SerializeField] private int age;
[SerializeField] private string favoriteColor;

public int Age { get => age; set => age = value; }
public string FavoriteColor { get => favoriteColor; set => favoriteColor = value; }
```

### Nouveaux types d'équipes
```csharp
// Dans Team.cs
public enum TeamType { MaisonRepos, Club, Famille }
[SerializeField] private TeamType teamType;
```

### Interface personnalisée
Tous les composants UI peuvent être étendus ou remplacés en héritant des classes de base.

## 🔍 Dépannage

### Problèmes courants

**Aucun joueur affiché**
- Vérifier que PlayerProfileManager est présent dans la scène
- Contrôler les logs pour les erreurs de chargement

**Sauvegarde ne fonctionne pas**
- Vérifier les permissions d'écriture
- Essayer PlayerPrefs comme alternative

**Sélection ne fonctionne pas**
- Vérifier que GamePlayerSelector est initialisé
- Contrôler les événements OnPlayersSelected

### Logs de debug
Activer `debugMode = true` dans les composants pour voir les logs détaillés.

## 📋 Évolutions futures

### Corrections apportées
- **✅ Corrections Unity 2023.x** : Mise à jour des appels `FindObjectOfType` vers `FindFirstObjectByType` et `FindObjectsByType`
- **✅ Suppression des dépendances LeanTween** : Remplacement par des animations natives avec coroutines
- **✅ Correction des couleurs** : Utilisation de `new Color(0.8f, 0.8f, 0.8f, 1f)` au lieu de `Color.lightGray`
- **✅ Optimisation des performances** : Animations plus fluides et moins gourmandes

### Fonctionnalités prévues
- **Photos des joueurs** : Ajout d'avatars
- **Statistiques avancées** : Graphiques et historiques
- **Import/Export** : Sauvegarde vers fichier externe
- **Modes de jeu** : Tournois, championnats
- **Badges** : Système de récompenses

### Améliorations possibles
- **Notifications** : Alertes pour les anniversaires
- **Préférences** : Jeux favoris par joueur
- **Accessibilité** : Support des technologies d'assistance
- **Synchronisation** : Backup cloud

## 🤝 Contribution

Ce système respecte les conventions du projet Motion Party :
- **Architecture modulaire** : Composants indépendants
- **Événements** : Communication découplée
- **Configuration** : Paramètres visibles dans l'Inspector
- **Documentation** : Code commenté et README complet

Pour contribuer :
1. Respecter les conventions de nommage
2. Ajouter des commentaires XML
3. Tester avec différents nombres de joueurs
4. Mettre à jour ce README si nécessaire

---

*Ce système a été conçu pour faciliter la gestion des joueurs dans les maisons de repos, avec une interface simple et intuitive adaptée à tous les niveaux d'utilisateurs.*

## 🛠️ Guide d'implémentation Unity

### 🎯 Vue d'ensemble de l'implémentation

Le système se compose de **3 scènes Unity** avec des GameObjects spécifiques :

1. **Menu Principal** - Gestion des joueurs et navigation
2. **Sélection de Joueurs** - Interface avant partie
3. **Scènes de Mini-jeux** - Intégration avec le système existant

---

### 🏗️ Étape 1 : Configuration de la scène Menu Principal

#### A. Créer la hiérarchie UI de base

```
📁 Canvas (si n'existe pas déjà)
├── 📁 MenuPrincipal_Panel
│   ├── 🎮 PlayButton
│   ├── ⚙️ OptionsButton  
│   ├── 👥 TeamButton
│   └── 🚪 QuitButton
├── 📁 GameSelection_Panel
│   ├── 🎯 NewLocalGameButton
│   ├── 🌐 MultiplayerButton
│   ├── 📊 LevelSelectButton
│   └── ⬅️ BackButton
└── 📁 PlayerManagement_Panel
    ├── 📋 PlayerList (ScrollView)
    ├── ➕ AddPlayerButton
    ├── 🔍 SearchInputField
    └── ⬅️ BackButton
```

#### B. GameObjects à créer dans la scène

**1. Managers (GameObjects vides)**
```
📁 === MANAGERS ===
├── 🎮 MainMenuController
├── 👥 PlayerProfileManager  
├── 🎯 GameSelectionController
└── 🏆 GamePlayerSelector
```

**2. UI Panels (UI GameObjects)**
```
📁 === UI PANELS ===
├── 📱 PlayerManagementPanel
│   └── Script: PlayerManagementUI
├── 🎮 GameSelectionPanel
│   └── Script: GameSelectionController
└── 🏠 MainMenuPanel
    └── Script: MainMenuController
```

#### C. Scripts à attacher

| GameObject | Script | Fonction |
|------------|--------|----------|
| **MainMenuController** | `MainMenuController.cs` | Navigation principale |
| **PlayerProfileManager** | `PlayerProfileManager.cs` | Gestion des données |
| **GameSelectionController** | `GameSelectionController.cs` | Lancement de parties |
| **GamePlayerSelector** | `GamePlayerSelector.cs` | Sélection pour parties |
| **PlayerManagementPanel** | `PlayerManagementUI.cs` | Interface de gestion |

---

### 🎮 Étape 2 : Configuration des références Inspector

#### A. MainMenuController
```csharp
[Header("UI Panels")]
✅ mainMenuPanel → MainMenuPanel GameObject
✅ gameSelectionPanel → GameSelectionPanel GameObject
✅ playerManagementPanel → PlayerManagementPanel GameObject

[Header("UI Buttons")]  
✅ playButton → PlayButton
✅ teamButton → TeamButton
✅ optionsButton → OptionsButton
✅ quitButton → QuitButton

[Header("Controllers")]
✅ gameSelectionController → GameSelectionController GameObject
✅ playerManagementUI → PlayerManagementUI Component
```

#### B. PlayerManagementUI
```csharp
[Header("UI References")]
✅ playerListContent → Content de la ScrollView
✅ addPlayerButton → AddPlayerButton
✅ searchInputField → SearchInputField
✅ backButton → BackButton

[Header("Player Item")]
✅ playerItemPrefab → Prefab PlayerManagementItem
```

#### C. GameSelectionController
```csharp
[Header("UI Buttons")]
✅ newLocalGameButton → NewLocalGameButton
✅ multiplayerButton → MultiplayerButton
✅ backButton → BackButton

[Header("References")]
✅ mainMenuController → MainMenuController GameObject
```

---

### 🎯 Étape 3 : Création des Prefabs

#### A. PlayerManagementItem Prefab
```
📁 PlayerManagementItem (Prefab)
├── 🖼️ Background (Image)
├── 📝 PlayerNameText (TextMeshPro)
├── 🏠 TeamNameText (TextMeshPro)
├── 📊 StatsText (TextMeshPro)
├── ✏️ EditButton (Button)
└── ❌ DeleteButton (Button)
```

**Script à attacher :** `PlayerManagementItem.cs`

#### B. PlayerSelectionItem Prefab
```
📁 PlayerSelectionItem (Prefab)
├── 🖼️ Background (Image)
├── ☑️ SelectionToggle (Toggle)
├── 📝 PlayerNameText (TextMeshPro)
├── 🏠 TeamNameText (TextMeshPro)
└── 📊 StatsText (TextMeshPro)
```

**Script à attacher :** `PlayerSelectionItem.cs`

---

### 🎮 Étape 4 : Création de la scène Sélection de Joueurs

#### A. Nouvelle scène ou Panel
```
📁 === PLAYER SELECTION SCENE ===
├── 📱 Canvas
│   └── 📋 PlayerSelectionPanel
│       ├── 📜 PlayerList (ScrollView)
│       ├── 🔍 SearchInputField
│       ├── 📊 SelectedCountText
│       ├── ✅ ConfirmButton
│       └── ❌ CancelButton
└── 🎮 GamePlayerSelector (GameObject)
```

#### B. Configuration PlayerSelectionUI
```csharp
[Header("UI References")]
✅ playerListContent → Content de la ScrollView
✅ searchInputField → SearchInputField
✅ selectedCountText → SelectedCountText
✅ confirmButton → ConfirmButton
✅ cancelButton → CancelButton

[Header("Player Item")]
✅ playerItemPrefab → PlayerSelectionItem Prefab

[Header("Selection Settings")]
✅ maxSelectedPlayers → 4 (par exemple)
✅ minSelectedPlayers → 1
✅ allowMultipleSelection → true
```

---

### 🏆 Étape 5 : Intégration dans les Mini-jeux

#### A. Dans chaque scène de mini-jeu
```
📁 === MINI-GAME SCENE ===
├── 🎮 (Objets de jeu existants)
├── 🎯 GamePlayerSelector (GameObject)
│   └── Script: GamePlayerSelector.cs
└── 📊 UI_GameSession
    ├── 👤 CurrentPlayerDisplay
    ├── 📊 ScoreDisplay
    └── ⏭️ NextPlayerButton
```

#### B. Intégration avec MiniGameBase
```csharp
public class MonMiniJeu : MiniGameBase
{
    private GamePlayerSelector playerSelector;
    
    protected override void Start()
    {
        base.Start();
        playerSelector = GamePlayerSelector.Instance;
    }
    
    protected override void OnGameEnd()
    {
        // Ajouter score au joueur actuel
        playerSelector.AddScoreToCurrentPlayer(finalScore);
        
        // Passer au joueur suivant
        var nextPlayer = playerSelector.NextPlayer();
        if (nextPlayer != null)
        {
            // Continuer avec le joueur suivant
            RestartGame();
        }
        else
        {
            // Fin de partie pour tous les joueurs
            playerSelector.EndGame();
            ReturnToMenu();
        }
    }
}
```

---

### 🔧 Étape 6 : Configuration des paramètres

#### A. PlayerProfileManager (Inspector)
```
✅ Use Player Prefs: false (utiliser JSON)
✅ Debug Mode: true (pour les tests)
✅ Max Players: 50
✅ Auto Save: true
```

#### B. GamePlayerSelector (Inspector)
```
✅ Debug Mode: true (pour les tests)
✅ Auto Next Player: false
✅ Track Scores: true
```

---

### 🎯 Étape 7 : Workflow de test

#### A. Test de création de joueurs
1. ▶️ Lancer la scène Menu Principal
2. 👥 Cliquer sur "Équipe"
3. ➕ Créer 3-4 joueurs de test
4. 💾 Vérifier la sauvegarde

#### B. Test de sélection
1. 🎮 Cliquer sur "Jouer"
2. 🎯 Cliquer sur "Nouvelle partie locale"
3. ☑️ Sélectionner 2 joueurs
4. ✅ Confirmer la sélection

#### C. Test d'intégration mini-jeu
1. 🎮 Lancer un mini-jeu
2. 📊 Vérifier l'affichage du joueur actuel
3. 🏆 Ajouter des scores
4. ⏭️ Passer au joueur suivant

---

### 📋 Checklist d'implémentation

#### ✅ Scène Menu Principal
- [ ] Hiérarchie UI créée
- [ ] GameObjects managers créés
- [ ] Scripts attachés correctement
- [ ] Références Inspector configurées
- [ ] Boutons connectés aux événements

#### ✅ Prefabs
- [ ] PlayerManagementItem créé
- [ ] PlayerSelectionItem créé
- [ ] Scripts attachés aux prefabs
- [ ] Références UI configurées

#### ✅ Sélection de joueurs
- [ ] Panel ou scène créée
- [ ] PlayerSelectionUI configurée
- [ ] Workflow de sélection testé

#### ✅ Mini-jeux
- [ ] GamePlayerSelector ajouté
- [ ] Intégration avec MiniGameBase
- [ ] Affichage joueur actuel
- [ ] Système de scoring

#### ✅ Tests
- [ ] Création de joueurs
- [ ] Sélection de joueurs
- [ ] Lancement de partie
- [ ] Scoring et tours
- [ ] Sauvegarde/chargement

---

### 🚨 Points d'attention

#### A. Ordre d'initialisation
```csharp
// Dans Awake() ou Start()
1. PlayerProfileManager.Instance (charge les données)
2. GamePlayerSelector.Instance (prêt pour sélection)
3. UI Controllers (connectent aux managers)
```

#### B. Gestion des transitions
```csharp
// Workflow recommandé
Menu → Équipe → Retour Menu → Jouer → Sélection → Mini-jeu
```

#### C. Persistance des données
```csharp
// Les données sont sauvegardées automatiquement
// Fichier : Application.persistentDataPath + "/players.json"
```

---

### 🎯 Personnalisation rapide

#### A. Ajouter des champs joueur
```csharp
// Dans PlayerData.cs
public string DateNaissance;
public string Photo;
public List<string> JeuxFavoris;
```

#### B. Modifier l'apparence
```csharp
// Dans PlayerManagementItem.cs
[Header("Visual Configuration")]
public Color teamColor = Color.blue;
public Sprite playerAvatar;
```

#### C. Ajouter des filtres
```csharp
// Dans PlayerManagementUI.cs
public void FilterByAge(int minAge, int maxAge);
public void FilterByTeam(string teamName);
```

---

**Cette implémentation est complète et prête à l'emploi !** 🎉
