# 🏠 Section Équipe - Guide d'implémentation

## 📋 Vue d'ensemble

Cette section gère la création et la gestion de l'équipe (maison de repos) et de ses joueurs directement depuis le menu principal.

## 🎯 Fonctionnalités

### ✅ Gestion d'équipe
- **Créat   ├── Content (Vertical Layout Group)
   │   ├── TeamNameText (TextMeshPro) - "Équipe : ..."
   │   ├── GamesPlayedText (TextMeshPro) - "Parties jouées : ..."
   │   ├── TotalScoreText (TextMeshPro) - "Score total : ..."
   │   └── CreatedDateText (TextMeshPro) - "Créé le : ..."'équipe** : Premier lancement → Saisie du nom de la maison de repos
- **Stockage persistant** : Nom d'équipe sauvegardé via PlayerPrefs
- **Affichage** : Nom de l'équipe visible en permanence

### ✅ Gestion des joueurs
- **Ajout rapide** : Bouton "Ajouter un joueur" → Saisie du prénom → Création automatique
- **Liste dynamique** : Affichage scrollable des joueurs de l'équipe
- **Actions** : Édition et suppression par joueur (boutons dédiés)
- **Statistiques** : Nombre de parties jouées visible par joueur

### ✅ Interface utilisateur
- **Responsive** : Adaptée aux personnes âgées (gros boutons, texte lisible)
- **Modulaire** : Composants réutilisables et extensibles
- **Intuitive** : Workflow logique avec confirmations/annulations

## 🏗️ Architecture

### Structure des fichiers
```
Assets/Scripts/
├── Systems/
│   └── TeamSetupManager.cs          # Gestionnaire principal d'équipe
└── UI/
    └── Team/
        ├── TeamManagementUI.cs       # Interface principale
        └── TeamPlayerItem.cs         # Composant item joueur
```

### Composants principaux

#### 1. **TeamSetupManager** - Gestionnaire d'équipe
```csharp
// Singleton pour la gestion d'équipe
- SetTeamName(string name)           # Définir le nom d'équipe
- CreatePlayer(string name)          # Créer un joueur
- RemovePlayer(string id)            # Supprimer un joueur
- GetTeamPlayers()                   # Récupérer les joueurs de l'équipe
```

#### 2. **TeamManagementUI** - Interface principale
```csharp
// Gestion de l'interface utilisateur
- Configuration d'équipe (première fois)
- Ajout/suppression de joueurs
- Affichage de la liste
- Navigation (retour menu)
```

#### 3. **TeamPlayerItem** - Item de joueur
```csharp
// Composant pour chaque joueur dans la liste
- Affichage nom + statistiques
- Boutons Éditer/Supprimer
- Animations hover
- Callbacks vers l'interface parent
```

## 🛠️ Implémentation Unity

### 1. Hiérarchie UI recommandée

```
PlayerManagementPanel (existant)
├── 📁 Header
│   ├── TeamNameText (TextMeshPro)
│   └── PlayerCountText (TextMeshPro)
├── 📁 PlayerList
│   ├── ScrollView
│   │   └── Content (avec TeamPlayerItem prefabs)
│   └── AddPlayerButton
├── 📁 Controls
│   └── BackButton
├── 📁 TeamSetupPanel (popup)
│   ├── Background (Image semi-transparente)
│   ├── Dialog
│   │   ├── TitleText ("Nom de votre équipe")
│   │   ├── TeamNameInput (TMP_InputField)
│   │   ├── ConfirmButton
│   │   └── CancelButton
└── 📁 PlayerCreationPanel (popup)
    ├── Background (Image semi-transparente)
    ├── Dialog
    │   ├── TitleText ("Ajouter un joueur")
    │   ├── PlayerNameInput (TMP_InputField)
    │   ├── ConfirmButton
    │   └── CancelButton
```

### 2. Configuration des scripts

#### TeamSetupManager
```csharp
// Créer GameObject vide "TeamSetupManager"
[Header("Configuration")]
✅ debugMode = false (true pour tests)
✅ defaultTeamName = "Ma Maison de Repos"
```

#### TeamManagementUI  
```csharp
// Sur le GameObject PlayerManagementPanel
[Header("UI References")]
✅ addPlayerButton → AddPlayerButton
✅ backButton → BackButton
✅ playerListContent → ScrollView/Content
✅ teamNameText → TeamNameText
✅ playerCountText → PlayerCountText

[Header("Player Item")]
✅ playerItemPrefab → TeamPlayerItem Prefab (fallback)
✅ simplePlayerButtonPrefab → SimplePlayerButton Prefab

[Header("Player Details")]
✅ playerDetailsPopupPrefab → PlayerDetailsPopup Prefab (instanciation dynamique)

[Header("Team Setup")]
✅ teamSetupPanel → TeamSetupPanel
✅ teamNameInput → TeamNameInput
✅ confirmTeamButton → ConfirmButton
✅ cancelTeamButton → CancelButton

[Header("Player Creation")]
✅ playerCreationPanel → PlayerCreationPanel
✅ playerNameInput → PlayerNameInput
✅ confirmPlayerButton → ConfirmButton
✅ cancelPlayerButton → CancelButton

[Header("Popup Animation")]
✅ teamSetupCanvasGroup → CanvasGroup du TeamSetupPanel
✅ playerCreationCanvasGroup → CanvasGroup du PlayerCreationPanel
✅ animationDuration = 0.3f
```

### 3. Configuration détaillée des popups

#### A. Création du TeamSetupPanel
```
1. Clic droit sur PlayerManagementPanel → UI → Panel
2. Renommer en "TeamSetupPanel"
3. RectTransform : Anchor = Stretch, Offset = 0,0,0,0 (plein écran)
4. Image : Color = Noir rgba(0,0,0,150) - fond semi-transparent
5. Add Component → Canvas Group (pour animations)

Enfant PopupDialog :
6. Clic droit sur TeamSetupPanel → UI → Panel
7. Renommer en "PopupDialog"
8. RectTransform : Anchor = Middle Center, Width = 400, Height = 200
9. Image : Color = Blanc, Add Component → Shadow (optionnel)
10. Add Component → Vertical Layout Group
11. Add Component → Content Size Fitter (Vertical = Preferred Size)

Contenu du PopupDialog :
├── TitleText (TextMeshPro) : "Nom de votre équipe"
├── TeamNameInput (TMP_InputField) : Placeholder "Ma Maison de Repos"
└── ButtonGroup (Panel avec Horizontal Layout Group)
    ├── ConfirmButton (Button) : "Confirmer"
    └── CancelButton (Button) : "Annuler"
```

#### B. Création du PlayerCreationPanel
```
Même structure que TeamSetupPanel mais avec :
├── TitleText : "Ajouter un joueur"
├── PlayerNameInput : Placeholder "Prénom du joueur"
└── ButtonGroup
    ├── ConfirmButton : "Ajouter"
    └── CancelButton : "Annuler"
```

#### C. Configuration des CanvasGroup
```
Sur TeamSetupPanel :
✅ Add Component → Canvas Group
✅ Alpha = 1, Interactable = true, Blocks Raycasts = true

Sur PlayerCreationPanel :
✅ Add Component → Canvas Group  
✅ Alpha = 1, Interactable = true, Blocks Raycasts = true
```

### 3. Création du prefab TeamPlayerItem

```
TeamPlayerItem (Panel)
├── 📁 PlayerInfo
│   ├── PlayerNameText (TextMeshPro) - Nom du joueur
│   └── PlayerStatsText (TextMeshPro) - "X parties • Y pts"
├── 📁 Actions
│   ├── EditButton (Button) - "Modifier"
│   └── RemoveButton (Button) - "Supprimer"
└── Background (Image) - Fond de l'item
```

**Configuration TeamPlayerItem :**
```csharp
[Header("UI References")]
✅ playerNameText → PlayerNameText
✅ playerStatsText → PlayerStatsText  
✅ editButton → EditButton
✅ removeButton → RemoveButton
✅ backgroundImage → Background
```

## 🎯 **Solution optimisée : Boutons simples + Popup détaillé**

### **Problème résolu :**
Les `PlayerManagementItem` étaient invisibles dans le ScrollView. La nouvelle solution utilise :
- **Boutons simples** dans la liste (juste nom + stats)
- **Popup détaillé** quand on clique sur un joueur

### **Nouveaux composants :**

#### **1. SimplePlayerButton.cs**
```csharp
// Bouton simple et visible pour chaque joueur
- Affichage : Nom + statistiques de base
- Clic → Ouvre les détails du joueur
- Couleurs hover/selection automatiques
- Compatible avec tous les prefabs existants
```

#### **2. PlayerDetailsPopup.cs**
```csharp
// Popup détaillé pour chaque joueur
- Affichage : Toutes les informations du joueur
- Actions : Éditer, Supprimer, Fermer
- Animations : Fade in/out
- Confirmation de suppression
```

### **Création du prefab SimplePlayerButton :**

```
1. Créer un nouveau prefab "SimplePlayerButton"
2. Structure recommandée :
   
SimplePlayerButton (Panel + Button)
├── Background (Image) - Fond du bouton
├── PlayerInfo (Panel + Horizontal Layout Group)
│   ├── PlayerNameText (TextMeshPro) - Nom du joueur
│   └── PlayerStatsText (TextMeshPro) - "X parties • Y pts"
└── Arrow (Image) - Icône ">" optionnelle

3. Configuration du script SimplePlayerButton :
   ✅ playerNameText → PlayerNameText
   ✅ playerStatsText → PlayerStatsText
   ✅ mainButton → Button component
   ✅ backgroundImage → Background Image
```

### **Création du popup PlayerDetailsPopup :**

**Étapes détaillées :**

```
1. Créer un nouveau GameObject "PlayerDetailsPopup"
2. Ajouter RectTransform → Anchor = Stretch, Offset = 0,0,0,0 (plein écran)
3. Ajouter Image → Color = Noir rgba(0,0,0,150) - fond semi-transparent
4. Ajouter le script PlayerDetailsPopup.cs
5. Ajouter CanvasGroup (pour animations)

Structure hiérarchique :
PlayerDetailsPopup (GameObject racine)
├── Background (Image semi-transparente)
├── PopupDialog (Panel centré)
│   ├── Header
│   │   ├── PlayerNameTitle (TextMeshPro) - Nom du joueur
│   │   └── CloseButton (Button) - "X"
│   ├── Content (Vertical Layout Group)
│   │   ├── TeamNameText (TextMeshPro) - "Équipe : ..."
│   │   ├── GamesPlayedText (TextMeshPro) - "Parties jouées : ..."
│   │   ├── TotalScoreText (TextMeshPro) - "Score total : ..."
│   │   └── CreatedDateText (TextMeshPro) - "Créé le : ..."
│   └── Actions (Horizontal Layout Group)
│       ├── EditButton (Button) - "Modifier"
│       └── DeleteButton (Button) - "Supprimer"
└── CanvasGroup (pour animations)

6. Sauvegarder comme PREFAB dans Assets/Prefabs/UI/
7. Désactiver ou supprimer le GameObject de la scène
```

**Configuration du script PlayerDetailsPopup :**

```csharp
[Header("UI References")]
✅ popupPanel → PopupDialog
✅ playerNameTitle → PlayerNameTitle
✅ teamNameText → TeamNameText
✅ gamesPlayedText → GamesPlayedText
✅ totalScoreText → TotalScoreText
✅ createdDateText → CreatedDateText

[Header("Action Buttons")]
✅ editButton → EditButton
✅ deleteButton → DeleteButton
✅ closeButton → CloseButton

[Header("Animation")]
✅ popupCanvasGroup → CanvasGroup du GameObject racine
✅ animationDuration = 0.3f
```

### **Configuration dans TeamManagementUI :**

```csharp
[Header("Player Item")]
✅ playerItemPrefab → Ancien prefab (fallback)
✅ simplePlayerButtonPrefab → Nouveau prefab SimplePlayerButton

[Header("Player Details")]
✅ playerDetailsPopupPrefab → Prefab PlayerDetailsPopup (pas GameObject de la scène)
```

### **⚠️ Important : Instanciation dynamique du popup**

Le popup `PlayerDetailsPopup` est maintenant instancié dynamiquement au lieu d'être référencé dans la scène. Ceci résout le problème des prefabs inactifs.

**Avantages :**
- ✅ Le prefab peut être inactif dans la scène
- ✅ Pas besoin d'assigner un GameObject existant
- ✅ Chaque popup est créé à la demande
- ✅ Nettoyage automatique après fermeture

**Configuration :**
```csharp
// Dans TeamManagementUI Inspector
[Header("Player Details")]
✅ playerDetailsPopupPrefab → Glisser le PREFAB PlayerDetailsPopup (pas un GameObject de la scène)
```

**Workflow automatique :**
1. Clic sur joueur → Instanciation du prefab
2. Affichage des détails → Animation fade-in
3. Fermeture → Animation fade-out + destruction automatique
4. Nouveau clic → Nouvelle instance fraîche

### **Workflow utilisateur amélioré :**

```
1. Liste des joueurs → Boutons simples et visibles
2. Clic sur un joueur → Popup détaillé avec toutes les infos
3. Actions disponibles → Éditer, Supprimer, Fermer
4. Interface claire → Séparation liste/détails
```

### **Avantages de cette solution :**

- ✅ **Visibilité garantie** : Boutons simples toujours visibles
- ✅ **Performance** : Liste légère, détails à la demande
- ✅ **UX moderne** : Pattern liste + popup détaillé
- ✅ **Compatibilité** : Fonctionne avec tous les prefabs existants
- ✅ **Extensible** : Facile d'ajouter de nouvelles actions

### **Logs attendus :**

```
[TeamManagementUI] Item configuré avec SimplePlayerButton pour TestPlayer
[TeamManagementUI] Joueur sélectionné : TestPlayer
[TeamManagementUI] Popup de détails ouvert pour TestPlayer
[PlayerDetailsPopup] Affichage des détails pour TestPlayer
[TeamManagementUI] Popup de détails fermé
```

### **Gestion des erreurs :**

Si le prefab n'est pas assigné :
```
[TeamManagementUI] playerDetailsPopupPrefab non assigné
```

Si le prefab n'a pas le bon composant :
```
[TeamManagementUI] Le prefab PlayerDetailsPopup n'a pas de composant PlayerDetailsPopup
```

## 🔧 Workflow utilisateur

### 1. Premier lancement
```
Menu Principal → Équipe → Popup "Nom de votre équipe"
└── Saisie nom → Confirmation → Affichage liste vide
```

### 2. Ajout de joueur
```
Section Équipe → "Ajouter un joueur" → Popup saisie
└── Saisie prénom → Confirmation → Rafraîchissement liste
```

### 3. Gestion d'un joueur
```
Liste des joueurs → Item joueur → Boutons "Modifier"/"Supprimer"
└── Action → Confirmation → Mise à jour liste
```

### 4. Retour menu
```
Section Équipe → "Retour" → Menu Principal
```

## 📊 Données persistantes

### PlayerPrefs utilisés
```csharp
"MotionParty_TeamName"          # Nom de l'équipe
"MotionParty_TeamSetupDone"     # Configuration terminée (0/1)
```

### Intégration avec PlayerProfileManager
- Les joueurs créés sont automatiquement ajoutés via `PlayerProfileManager`
- Sauvegarde JSON automatique
- Données synchronisées entre tous les systèmes

## 🎨 Personnalisation

### Apparence
```csharp
// Dans TeamPlayerItem.cs
[Header("Visual Configuration")]
✅ normalColor = Color.white
✅ hoverColor = Color.lightGray
```

### Textes
```csharp
// Dans TeamSetupManager.cs
✅ defaultTeamName = "Ma Maison de Repos"
```

### Validation
```csharp
// Logique de validation simple
- Nom d'équipe non vide
- Nom de joueur non vide
- Pas de doublons (géré par PlayerProfileManager)
```

## 🚀 Extensions futures

### Fonctionnalités prévues
- **Modification du nom d'équipe** : Bouton "Modifier" à côté du nom
- **Photos des joueurs** : Ajout d'avatars dans TeamPlayerItem
- **Statistiques avancées** : Graphiques de progression
- **Import/Export** : Sauvegarde d'équipe complète

### Points d'extension
- **TeamPlayerItem** : Espace prévu pour photo/avatar
- **TeamSetupManager** : Méthodes pour modification d'équipe
- **TeamManagementUI** : Panels extensibles pour nouvelles fonctionnalités

## ⚠️ Points importants

1. **Singleton** : TeamSetupManager est un singleton, une seule instance
2. **Persistance** : Utilise PlayerPrefs pour la config, JSON pour les joueurs
3. **Événements** : Communication via événements Unity (OnTeamNameChanged, OnPlayerAdded, etc.)
4. **Modularité** : Chaque composant peut être activé/désactivé indépendamment
5. **Compatibilité** : Fonctionne avec l'architecture PlayerProfileManager existante

## 🧪 Tests

### Checklist de validation
- [ ] Premier lancement → Popup création d'équipe
- [ ] Nom d'équipe sauvegardé et affiché
- [ ] Bouton "Ajouter un joueur" fonctionnel
- [ ] Liste des joueurs mise à jour dynamiquement
- [ ] Bouton "Supprimer" fonctionnel avec confirmation
- [ ] Bouton "Retour" ramène au menu principal
- [ ] Persistance après redémarrage Unity
- [ ] Compteur de joueurs mis à jour
- [ ] Interface responsive sur différentes tailles d'écran

---

*Cette implémentation respecte les contraintes du projet : modulaire, extensible, compatible avec l'architecture existante et adaptée aux utilisateurs seniors.*

## 🐛 Débogage et résolution des problèmes

### Problèmes courants

#### 1. **Les joueurs ne s'affichent pas dans la liste**

**Causes possibles :**
- PlayerProfileManager.Instance non initialisé
- Nom d'équipe ne correspond pas
- Prefab TeamPlayerItem non assigné
- PlayerListContent non assigné

**Solutions :**
```csharp
// Vérifier dans TeamManagementUI.cs
[Header("Configuration")]
✅ debugMode = true; // Activer pour voir les logs

// Vérifier dans l'Inspector Unity
✅ playerItemPrefab → Assigner le prefab TeamPlayerItem
✅ playerListContent → Assigner le Transform du ScrollView/Content
```

#### 2. **Le bouton "Retour" ne fonctionne plus**

**Causes possibles :**
- MainMenuController introuvable
- Hiérarchie UI modifiée

**Solutions :**
```csharp
// Code corrigé dans TeamManagementUI.cs avec fallback
private void OnBackClicked()
{
    // Tentative principale
    var mainMenuController = FindFirstObjectByType<UI.Menu.MainMenuController>();
    if (mainMenuController != null)
    {
        mainMenuController.ShowMainMenu();
    }
    else
    {
        // Fallback : méthode alternative
        gameObject.SetActive(false);
        var mainMenuPanel = GameObject.Find("MainMenuPanel");
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
}
```

#### 3. **Les popups ne s'animent pas correctement**

**Causes possibles :**
- CanvasGroup manquant sur les panels
- Références incorrectes dans l'Inspector

**Solutions :**
```
TeamSetupPanel → Add Component → Canvas Group
PlayerCreationPanel → Add Component → Canvas Group

Dans TeamManagementUI Inspector :
✅ teamSetupCanvasGroup → CanvasGroup du TeamSetupPanel
✅ playerCreationCanvasGroup → CanvasGroup du PlayerCreationPanel
```

### Script de débogage

Un script `TeamSystemDebugger.cs` est fourni pour diagnostiquer les problèmes :

```csharp
// Utilisation :
1. Créer un GameObject vide "TeamDebugger"
2. Attacher TeamSystemDebugger.cs
3. Utiliser les boutons dans l'Inspector ou les Context Menu :
   - Test - Create Player
   - Test - List Players
   - Test - Reset Team
   - Test - Check UI References
```

### Logs de débogage

Activez `debugMode = true` dans TeamManagementUI pour voir les logs détaillés :

```
[TeamManagementUI] Tentative de création du joueur 'TestPlayer'
[TeamManagementUI] Joueur créé avec succès : TestPlayer
[TeamManagementUI] Récupération de 1 joueurs pour l'équipe 'Ma Maison de Repos'
[TeamManagementUI] Création item pour joueur TestPlayer
[TeamManagementUI] Item configuré pour TestPlayer
```

### Vérifications étape par étape

1. **Vérifier les instances** :
   ```csharp
   Debug.Log(Systems.TeamSetupManager.Instance != null);
   Debug.Log(Systems.PlayerProfileManager.Instance != null);
   ```

2. **Vérifier les données** :
   ```csharp
   Debug.Log($"Nom d'équipe: {Systems.TeamSetupManager.Instance.GetTeamName()}");
   Debug.Log($"Nombre de joueurs: {Systems.TeamSetupManager.Instance.GetTeamPlayerCount()}");
   ```

3. **Vérifier l'UI** :
   ```csharp
   Debug.Log($"PlayerItemPrefab: {playerItemPrefab != null}");
   Debug.Log($"PlayerListContent: {playerListContent != null}");
   ```

4. **Vérifier le prefab SimplePlayerButton** :
   ```
   ✅ playerNameText assigné et unique
   ✅ playerStatsText assigné et unique
   ✅ mainButton assigné et unique
   ✅ Pas de références partagées entre instances
   ```

5. **Test de validation** :
   ```csharp
   // Dans la console Unity, exécuter :
   var teamUI = FindFirstObjectByType<TeamManagementUI>();
   teamUI.ForceRefreshPlayerList();
   ```

**Si le problème persiste :**
- Vérifier qu'il n'y a pas de singleton ou de référence statique partagée
- Confirmer que chaque `Instantiate()` crée bien un nouvel objet
- Vérifier que les composants TextMeshPro ne sont pas partagés entre instances

#### 4. **Prefab PlayerSelectionItem existant détecté**

**Problème :** Le prefab `PlayerSelectionItem` n'a pas le composant `TeamPlayerItem`

**Solutions :**

**Option A - Ajouter TeamPlayerItem au prefab :**
```
1. Ouvrir le prefab PlayerSelectionItem en mode édition
2. Sélectionner le GameObject racine
3. Add Component → TeamPlayerItem
4. Configurer les références :
   ✅ playerNameText → NicknameText
   ✅ playerStatsText → StatsText (créer si nécessaire)
   ✅ editButton → SelectionToggle (réutilisé)
   ✅ removeButton → Nouveau bouton ou Context Menu
   ✅ backgroundImage → Background
```

**Option B - Utiliser l'adaptateur (automatique) :**
```
Le système détecte automatiquement le prefab PlayerSelectionItem
et ajoute un PlayerSelectionItemAdapter qui :
- Réutilise les composants existants
- Transforme le Toggle en bouton d'édition  
- Ajoute la fonctionnalité de suppression
- Maintient la compatibilité avec l'UI existante
```

**Logs attendus avec l'adaptateur :**
```
[TeamManagementUI] Item configuré avec PlayerSelectionItemAdapter pour TestPlayer
```

### **Configuration PlayerDetailsPopup :**

```csharp
[Header("UI References")]
✅ playerNameTitle → TextMeshPro du nom du joueur
✅ teamNameText → TextMeshPro de l'équipe
✅ gamesPlayedText → TextMeshPro parties jouées
✅ totalScoreText → TextMeshPro score total
✅ createdDateText → TextMeshPro de création

[Header("Action Buttons")]
✅ editButton → Bouton "Modifier"
✅ deleteButton → Bouton "Supprimer"
✅ closeButton → Bouton "Fermer"

[Header("Animation")]
✅ popupCanvasGroup → CanvasGroup pour animations
✅ animationDuration = 0.3f
```

### **Propriétés PlayerData disponibles :**

```csharp
// Propriétés accessibles dans PlayerData
player.Id              // string - Identifiant unique
player.Nickname        // string - Nom du joueur
player.TeamName        // string - Nom de l'équipe
player.CreatedAt       // DateTime - Date de création
player.GamesPlayed     // int - Nombre de parties
player.TotalScore      // int - Score total
player.IsActive        // bool - Joueur actif
```

### 🔍 **Diagnostic rapide pour le problème "tous les joueurs sur un seul item"**

**Étapes de diagnostic :**

1. **Vérifier les logs** (activer `debugMode = true` dans `TeamManagementUI`) :
   ```
   ✅ [TeamManagementUI] Récupération de X joueurs pour l'équipe...
   ✅ [TeamManagementUI] Création item pour joueur [Nom1]
   ✅ [TeamManagementUI] Item configuré avec SimplePlayerButton pour [Nom1]
   ✅ [TeamManagementUI] Création item pour joueur [Nom2]
   ✅ [TeamManagementUI] Item configuré avec SimplePlayerButton pour [Nom2]
   ```

2. **Utiliser TeamSystemDebugger** :
   ```
   - Clic droit sur TeamSystemDebugger dans l'Inspector
   - "Count Player Items" → Vérifier le nombre d'enfants
   - "Force Refresh Player List" → Forcer le rafraîchissement
   ```

3. **Vérifier la configuration ScrollView** :
   ```
   ScrollView/Content :
   ✅ Vertical Layout Group présent
   ✅ Content Size Fitter présent
   ✅ Child Force Expand Width = true
   ✅ Child Force Expand Height = false
   ```

4. **Vérifier le prefab SimplePlayerButton** :
   ```
   ✅ playerNameText assigné et unique
   ✅ playerStatsText assigné et unique
   ✅ mainButton assigné et unique
   ✅ Pas de références partagées entre instances
   ```

5. **Test de validation** :
   ```csharp
   // Dans la console Unity, exécuter :
   var teamUI = FindFirstObjectByType<TeamManagementUI>();
   teamUI.ForceRefreshPlayerList();
   ```

**Si le problème persiste :**
- Vérifier qu'il n'y a pas de singleton ou de référence statique partagée
- Confirmer que chaque `Instantiate()` crée bien un nouvel objet
- Vérifier que les composants TextMeshPro ne sont pas partagés entre instances

#### 5. **Les boutons se superposent dans le ScrollView**

**Problème :** Tous les boutons des joueurs se superposent au lieu d'être alignés verticalement.

**Cause :** Configuration incorrecte du ScrollView, Viewport, ou Content.

**Solution complète :**

**A. Configuration du ScrollView :**
```
ScrollView (GameObject principal)
├── Viewport (RectTransform)
│   └── Content (Transform avec Layout Group)
│       ├── SimplePlayerButton (Clone)
│       ├── SimplePlayerButton (Clone)
│       └── SimplePlayerButton (Clone)
└── Scrollbar Vertical (optionnel)
```

**B. Configuration étape par étape :**

1. **ScrollView (GameObject racine) :**
   ```
   ✅ Add Component → Scroll Rect
   ✅ Content → Assigner le Transform "Content"
   ✅ Viewport → Assigner le RectTransform "Viewport"
   ✅ Vertical = true, Horizontal = false
   ✅ Movement Type = Elastic
   ✅ Elasticity = 0.1
   ```

2. **Viewport (enfant du ScrollView) :**
   ```
   ✅ RectTransform → Anchor = Stretch, Offset = 0,0,0,0
   ✅ Add Component → Mask
   ✅ Add Component → Image (pour le masquage)
   ✅ Image → Color = blanc transparent ou désactivé
   ```

3. **Content (enfant du Viewport) :**
   ```
   ✅ RectTransform → Anchor = Top Stretch
   ✅ RectTransform → Anchor Min = (0, 1), Anchor Max = (1, 1)
   ✅ RectTransform → Offset Left = 0, Right = 0, Top = 0
   ✅ RectTransform → Height = automatique (géré par Content Size Fitter)
   
   ✅ Add Component → Vertical Layout Group
   ✅ Vertical Layout Group → Spacing = 10
   ✅ Vertical Layout Group → Child Alignment = Upper Center
   ✅ Vertical Layout Group → Child Controls Size → Width = true, Height = false
   ✅ Vertical Layout Group → Child Force Expand → Width = true, Height = false
   
   ✅ Add Component → Content Size Fitter
   ✅ Content Size Fitter → Vertical Fit = Preferred Size
   ✅ Content Size Fitter → Horizontal Fit = Unconstrained
   ```

**C. Configuration du prefab SimplePlayerButton :**
```
SimplePlayerButton (prefab)
├── RectTransform → Height = 80 (hauteur fixe)
├── Layout Element → Min Height = 80, Preferred Height = 80
├── Horizontal Layout Group (pour organiser le contenu interne)
└── Content Size Fitter → Horizontal Fit = Unconstrained
```

**D. Vérification des tailles :**
```
Content (Transform) :
✅ Width = automatique (suit le parent)
✅ Height = calculée automatiquement par Content Size Fitter
✅ Pivot = (0.5, 1) - centré en haut

SimplePlayerButton (prefab) :
✅ Width = 100% du parent (contrôlé par Layout Group)
✅ Height = 80 pixels (fixe)
✅ Margin = 0 (géré par Layout Group Spacing)
```

**E. Test de diagnostic :**
```csharp
// Dans TeamSystemDebugger.cs
[ContextMenu("Check ScrollView Configuration")]
public void CheckScrollViewConfiguration()
{
    var teamUI = FindFirstObjectByType<TeamManagementUI>();
    if (teamUI != null)
    {
        var contentField = typeof(TeamManagementUI).GetField("playerListContent", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (contentField != null)
        {
            var content = contentField.GetValue(teamUI) as Transform;
            if (content != null)
            {
                var verticalLayout = content.GetComponent<VerticalLayoutGroup>();
                var contentSizeFitter = content.GetComponent<ContentSizeFitter>();
                
                Debug.Log($"Content: {content.name}");
                Debug.Log($"Vertical Layout Group: {verticalLayout != null}");
                Debug.Log($"Content Size Fitter: {contentSizeFitter != null}");
                Debug.Log($"Nombre d'enfants: {content.childCount}");
                
                for (int i = 0; i < content.childCount; i++)
                {
                    var child = content.GetChild(i);
                    var rect = child.GetComponent<RectTransform>();
                    Debug.Log($"Enfant {i}: {child.name} - Position: {rect.anchoredPosition}, Taille: {rect.sizeDelta}");
                }
            }
        }
    }
}
```

#### 6. **Le popup PlayerDetailsPopup reste invisible**

**Problème :** Les logs montrent que le popup s'ouvre, mais il reste invisible sur l'écran.

**Causes possibles :**
- Popup instancié derrière d'autres éléments (mauvais ordre de rendu)
- Canvas mal configuré
- Popup instancié dans le mauvais parent
- Taille ou position incorrecte

**Solutions :**

**A. Vérifier l'ordre de rendu :**
```csharp
// Dans TeamManagementUI.cs, modifier OnPlayerSelected
private void OnPlayerSelected(Systems.PlayerData player)
{
    if (debugMode)
        Debug.Log($"TeamManagementUI: Joueur sélectionné : {player.Nickname}");

    if (playerDetailsPopupPrefab != null)
    {
        // Détruire l'instance précédente
        if (currentPopupInstance != null)
        {
            Destroy(currentPopupInstance.gameObject);
        }

        // Trouver le Canvas principal pour assurer le bon rendu
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        Transform parentTransform = mainCanvas != null ? mainCanvas.transform : transform;

        // Créer une nouvelle instance dans le bon parent
        GameObject popupObject = Instantiate(playerDetailsPopupPrefab, parentTransform);
        currentPopupInstance = popupObject.GetComponent<PlayerDetailsPopup>();
        
        if (currentPopupInstance != null)
        {
            // Assurer que le popup est au premier plan
            popupObject.transform.SetAsLastSibling();
            
            // Vérifier la configuration RectTransform
            var rectTransform = popupObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
            
            currentPopupInstance.ShowPlayerDetails(player, OnEditPlayer, OnRemovePlayer);
            
            if (debugMode)
                Debug.Log($"TeamManagementUI: Popup créé pour {player.Nickname} - Parent: {parentTransform.name} - Ordre: {popupObject.transform.GetSiblingIndex()}");
        }
        else
        {
            Debug.LogError("TeamManagementUI: Le prefab PlayerDetailsPopup n'a pas de composant PlayerDetailsPopup");
        }
    }
    else
    {
        Debug.LogError("TeamManagementUI: playerDetailsPopupPrefab non assigné");
    }
}
```

**B. Configuration du prefab PlayerDetailsPopup :**
```
PlayerDetailsPopup (GameObject racine)
├── Canvas Group (Alpha = 1, Interactable = true, Blocks Raycasts = true)
├── RectTransform (Anchor = Stretch, Offset = 0,0,0,0)
├── Image (Color = Noir semi-transparent rgba(0,0,0,150))
└── PopupDialog (Panel centré)
    ├── RectTransform (Anchor = Middle Center, Width = 400, Height = 300)
    ├── Image (Color = Blanc, Material = None)
    └── [Contenu du popup...]
```

**C. Vérification des Canvas :**
```csharp
// Dans PlayerDetailsPopup.cs, ajouter en début de ShowPlayerDetails
public void ShowPlayerDetails(Systems.PlayerData player, System.Action<Systems.PlayerData> onEdit, System.Action<Systems.PlayerData> onDelete)
{
    // Vérifier la configuration du Canvas
    Canvas canvas = GetComponentInParent<Canvas>();
    if (canvas != null)
    {
        Debug.Log($"PlayerDetailsPopup: Canvas trouvé - Render Mode: {canvas.renderMode}, Sort Order: {canvas.sortingOrder}");
    }
    else
    {
        Debug.LogWarning("PlayerDetailsPopup: Aucun Canvas parent trouvé");
    }

    // Vérifier la visibilité
    var rectTransform = GetComponent<RectTransform>();
    if (rectTransform != null)
    {
        Debug.Log($"PlayerDetailsPopup: Position: {rectTransform.anchoredPosition}, Taille: {rectTransform.sizeDelta}");
    }

    // Code existant...
    currentPlayer = player;
    onEditPlayer = onEdit;
    onDeletePlayer = onDelete;

    UpdateDisplay();
    ShowPopup();

    if (debugMode)
        Debug.Log($"PlayerDetailsPopup: Affichage des détails pour {player.Nickname}");
}
```

**D. Test de diagnostic :**
```csharp
// Dans TeamSystemDebugger.cs
[ContextMenu("Check Popup Visibility")]
public void CheckPopupVisibility()
{
    var popup = FindFirstObjectByType<PlayerDetailsPopup>();
    if (popup != null)
    {
        Debug.Log($"Popup trouvé: {popup.name}");
        Debug.Log($"Popup actif: {popup.gameObject.activeInHierarchy}");
        
        var canvasGroup = popup.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            Debug.Log($"CanvasGroup Alpha: {canvasGroup.alpha}");
            Debug.Log($"CanvasGroup Interactable: {canvasGroup.interactable}");
            Debug.Log($"CanvasGroup Blocks Raycasts: {canvasGroup.blocksRaycasts}");
        }
        
        var rectTransform = popup.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Debug.Log($"RectTransform Position: {rectTransform.anchoredPosition}");
            Debug.Log($"RectTransform Size: {rectTransform.sizeDelta}");
        }
    }
    else
    {
        Debug.Log("Aucun popup PlayerDetailsPopup trouvé");
    }
}
```

**E. Solution de contournement :**
```csharp
// Dans PlayerDetailsPopup.cs, forcer la visibilité
private void ShowPopup()
{
    if (popupPanel != null)
    {
        popupPanel.SetActive(true);
        
        // Forcer la mise au premier plan
        transform.SetAsLastSibling();
        
        // Forcer la visibilité
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        StartCoroutine(FadeIn());
    }
}
```
