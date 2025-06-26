# Motion-Party
Animation game project for retirement homes in order to obtain my thesis in computer science

# 🎮 Configuration LogParade - Guide Développeur Unity

Ce guide explique comment configurer manuellement tous les scripts du mini-jeu **LogParade** dans une scène Unity propre.

## 🚨 **CORRECTION RAPIDE D'ERREURS**

Si vous voyez des erreurs dans la console Unity comme "La voie 1 n'est pas assignée" ou "Configuration invalide", utilisez ces outils de correction automatique :

### 🔧 LogParadeAutoFixer
1. Ajoutez le script `LogParadeAutoFixer` à n'importe quel GameObject dans votre scène
2. Dans l'inspecteur, cochez "Fix On Start" 
3. Lancez la scène - les erreurs seront corrigées automatiquement
4. **OU** utilisez le menu contextuel : `Clic droit > Fix All LogParade Errors`

### 🎨 LogParadeUIFixer  
1. Ajoutez le script `LogParadeUIFixer` pour corriger spécifiquement les erreurs d'UI
2. Corrige automatiquement les sliders manquants, textes non assignés, etc.
3. **OU** utilisez le menu contextuel : `Clic droit > Fix All UI Errors`

### 🎯 Correction Manuelle Rapide
Si vous préférez corriger manuellement :
- **Lanes manquantes** : Créez 4 GameObjects nommés "Lane_1" à "Lane_4" 
- **Prefabs manquants** : Placez 3 prefabs de rondins dans le dossier Resources/
- **UI manquante** : Le LogParadeUIFixer créera automatiquement l'UI de base

---

## � **MISE À JOUR IMPORTANTE**
**✅ Système Lane Indicators et Lane Highlights SUPPRIMÉ**
- Plus besoin de configurer les arrays `laneIndicators` et `laneHighlights`
- Configuration simplifiée pour améliorer l'expérience développeur
- Le système fonctionne désormais sans ces éléments UI complexes

## �📋 Pré-requis de la scène

✅ Un **PlayerAvatar** déjà placé dans la scène  
✅ Les **4 lanes** sont bien en place  
✅ **3 prefabs de rondins** : Log_Short, Log_Medium, Log_Long  
✅ Un **prefab de calibration** (CalibrationPrefab)

---

## 🎯 Scripts Principaux du LogParade

### 🎮 LogParadeGameTimer.cs
**But :** Contrôle le cycle de vie d'une partie (minuterie, démarrage/arrêt du jeu)

**Où l'ajouter :** GameObject principal "LogParadeGameManager" ou similaire

**Variables à assigner :**
- `gameDurationInSeconds` : Durée de la partie (60 secondes par défaut)
- `startDelay` : Délai avant démarrage (2 secondes par défaut)
- `scoreManager` : Référence vers LogParadeScoreManager
- `logGenerator` : Référence vers LogParadeLogGenerator
- `gameController` : Référence vers LogParadeGameController

**Conditions / Particularités :**
- Hérite de `MiniGameBase`
- Peut être démarré automatiquement ou via MiniGameBase
- Gère les événements OnGameStart/OnGameEnd

---

### 🎮 LogParadeGameController.cs
**But :** Contrôleur principal qui orchestre le jeu et gère le tracking latéral

**Où l'ajouter :** GameObject principal "LogParadeGameManager"

**Variables à assigner :**
- `udpReceive` : Référence vers le composant UDPReceive
- `uiManager` : Référence vers LogParadeUIManager
- `lateralTracker` : Référence vers LogParadeLateralTracker
- `playerAvatar` : Référence vers LogParadePlayerAvatar
- `scoreManager` : Référence vers LogParadeScoreManager
- `laneMarkers[4]` : Array des 4 Transforms représentant les positions des lanes
- `laneMaterials[4]` : Array des 4 Materials pour visualiser les lanes

**Conditions / Particularités :**
- Hérite de `MiniGameBase`
- Script principal qui coordonne tous les autres
- Trouve automatiquement les composants si pas assignés

---

### 🎮 LogParadeScoreManager.cs
**But :** Gère le système de score basé sur la position du joueur (sur rondin ou dans l'eau)

**Où l'ajouter :** GameObject dédié "ScoreManager" ou sur le GameManager principal

**Variables à assigner :**
- `pointsPerSecond` : Points gagnés par seconde sur rondin (1 par défaut)
- `penaltyPoints` : Points perdus en tombant à l'eau (5 par défaut)
- `initialScore` : Score de départ (0 par défaut)
- `PlayerLogCollisionChecker` : Référence vers PlayerLogCollisionChecker
- `scoreDisplayText` : (Optionnel) TextMeshProUGUI pour afficher le score

**Conditions / Particularités :**
- Trouve automatiquement PlayerLogCollisionChecker si non assigné
- Ne démarre le score qu'après la calibration
- Système tolérant pour public senior

---

### � PlayerLogCollisionChecker.cs
**But :** Détecte si le joueur est sur un rondin ou dans l'eau en temps réel

**Où l'ajouter :** Sur le GameObject PlayerAvatar

**Variables à assigner :**
- `detectionRadius` : Rayon de détection sous le joueur (0.8f par défaut)
- `maxLogGapDistance` : Distance max entre rondins (0.5f par défaut)
- `laneChangeToleranceTime` : Tolérance après changement de lane (0.3f par défaut)

**Conditions / Particularités :**
- Nécessite un SphereCollider (créé automatiquement)
- Utilise les tags "Log" sur les rondins
- Système de tolérance après changement de voie

---

### 🎮 LogParadeLogGenerator.cs
**But :** Génère et gère le cycle de vie des rondins qui descendent

**Où l'ajouter :** GameObject dédié "LogGenerator"

**Variables à assigner :**
- `lanes[4]` : Array des 4 Transforms des lanes
- `logPrefabs[3]` : Array des 3 prefabs (Log_Short, Log_Medium, Log_Long)
- `logSpeed` : Vitesse de descente des rondins (1.2f par défaut)
- `verticalSpacing` : Espacement vertical entre rondins (2.5f par défaut)
- `generationInterval` : Fréquence de génération (2.5f par défaut)
- `spawnHeight` : Hauteur de spawn (10f par défaut)
- `destroyHeight` : Hauteur de destruction (-5f par défaut)

**Conditions / Particularités :**
- Hérite de `MiniGameBase`
- Créé automatiquement LogParadeLogConfiguration
- Contrôlé par LogParadeGameTimer

---

### 🎮 LogParadeLogConfiguration.cs
**But :** Configuration centralisée du système de génération de rondins

**Où l'ajouter :** Ajouté automatiquement par LogParadeLogGenerator

**Variables à assigner :**
- Configuration automatique depuis LogParadeLogGenerator
- Valide automatiquement les références des lanes et prefabs

**Conditions / Particularités :**
- Créé automatiquement, ne pas ajouter manuellement
- Gère la validation des lanes aux positions X fixes (-3f, -1f, 1f, 3f)

---

### 🎮 LogParadePlayerAvatar.cs
**But :** Gère le mouvement de l'avatar du joueur sur les 4 voies

**Où l'ajouter :** Sur le GameObject PlayerAvatar existant

**Variables à assigner :**
- `moveSpeed` : Vitesse de déplacement (5f par défaut)
- `laneWidth` : Largeur d'une lane (2f par défaut)
- `basePosition` : Position de base (Vector3.zero par défaut)
- `avatarModel` : GameObject du modèle d'avatar (lui-même si null)

**Conditions / Particularités :**
- Réagit aux événements du LogParadeLateralTracker
- Gère les animations de déplacement
- Ajoute automatiquement AudioSource si manquant

---

### 🎮 LogParadeLateralTracker.cs
**But :** Suit les mouvements latéraux du joueur via MediaPipe et mappe sur les 4 voies

**Où l'ajouter :** GameObject dédié "LateralTracker" ou sur PlayerAvatar

**Variables à assigner :**
- `udpReceive` : Référence vers UDPReceive
- `smoothingFactor` : Facteur de lissage (0.8f par défaut)
- `leftBoundary` : Limite gauche (-1.5f par défaut)
- `rightBoundary` : Limite droite (1.5f par défaut)

**Conditions / Particularités :**
- Dépend d'UDPReceive pour les données MediaPipe
- Peut fonctionner sans calibration automatique
- Envoie des événements OnLaneChanged et OnPositionUpdated

---

### 🎮 LogParadeUIManager.cs
**But :** Orchestre tous les éléments d'interface utilisateur du jeu

**Où l'ajouter :** GameObject dédié "UIManager" ou Canvas principal

**Variables à assigner :**
- `currentLaneText` : TextMeshProUGUI pour afficher la lane actuelle
- `positionText` : TextMeshProUGUI pour la position
- `gameStatusText` : TextMeshProUGUI pour le statut du jeu
- `debugInfoText` : TextMeshProUGUI pour les infos debug
- `calibrationPanel` : GameObject du panel de calibration
- ~~`laneIndicators` et `laneHighlights` : SUPPRIMÉS - plus nécessaires~~

**Conditions / Particularités :**
- Utilise des modules UI spécialisés
- Gère l'affichage de calibration et de gameplay

---

### 🎮 LogParadeCalibrationInteractive.cs
**But :** Système de calibration interactive où le joueur doit se déplacer vers les lanes 1 et 4

**Où l'ajouter :** GameObject dédié "CalibrationManager" ou sur GameManager

**Variables à assigner :**
- `timeoutDuration` : Durée max de calibration (15f par défaut)
- `playerAvatar` : Référence vers LogParadePlayerAvatar
- `lateralTracker` : Référence vers LogParadeLateralTracker
- `laneTransforms[4]` : Array des 4 Transforms des lanes
- `logPrefab` : Prefab utilisé pour la calibration
- `calibrationTextUI` : Référence vers CalibrationTextUI (voir ci-dessous)

**Conditions / Particularités :**
- Lance les événements OnCalibrationCompleted
- Utilise LogParadeGameStateController pour les verrous
- Optionnel si calibration automatique suffisante
- **Important :** Nécessite un GameObject avec le composant `CalibrationTextUI`

---

### 🎮 CalibrationTextUI.cs
**But :** Interface utilisateur dédiée à la calibration interactive (textes, indicateurs, timer)

**Où l'ajouter :** GameObject dédié "CalibrationUI" (souvent enfant d'un Canvas)

**Variables à assigner :**
- `mainInstructionText` : TextMeshProUGUI pour les instructions principales
- `statusText` : TextMeshProUGUI pour le statut de calibration
- `timerText` : TextMeshProUGUI pour afficher le timer
- `progressBar` : Image pour la barre de progression (optionnelle)
- `successPanel` : GameObject affiché en cas de succès (optionnel)
- `timeoutPanel` : GameObject affiché en cas d'échec (optionnel)
- ~~`laneIndicators` : SUPPRIMÉ - plus nécessaire~~

**Conditions / Particularités :**
- Utilisé par LogParadeCalibrationInteractive
- Peut fonctionner avec des références partielles
- Ajoute automatiquement un CanvasGroup si manquant
- Gère les animations et transitions automatiquement

---

### 🎮 LogParadeGameStateController.cs
**But :** Contrôleur centralisé de l'état global du jeu (calibration, gameplay)

**Où l'ajouter :** GameObject dédié "GameStateController" ou GameManager

**Variables à assigner :**
- Aucune variable publique à assigner
- Configuration automatique

**Conditions / Particularités :**
- Singleton pattern
- Gère les verrous globaux (IsCalibrationInProgress, IsGameplayAllowed)
- Accès statique depuis tous les autres scripts

---

## 🔧 Configuration de la Scène - Récapitulatif

### ✅ GameObjects et Composants Attendus

#### 📦 **LogParadeGameManager** (GameObject principal)
**Composants :**
- `LogParadeGameTimer`
- `LogParadeGameController`
- `LogParadeGameStateController`

**Liens :**
- GameTimer → ScoreManager, LogGenerator, GameController
- GameController → UDPReceive, UIManager, LateralTracker, PlayerAvatar, ScoreManager

---

#### 📦 **PlayerAvatar** (GameObject existant)
**Composants :**
- `LogParadePlayerAvatar`
- `PlayerLogCollisionChecker`
- `LogParadeLateralTracker` (ou sur objet séparé)

**Liens :**
- PlayerAvatar ← référencé par GameController, ScoreManager
- LateralTracker → UDPReceive

---

#### 📦 **LogGenerator**
**Composants :**
- `LogParadeLogGenerator`
- `LogParadeLogConfiguration` (ajouté automatiquement)

**Liens :**
- LogGenerator → Lanes[4], LogPrefabs[3]
- LogGenerator ← référencé par GameTimer

---

#### 📦 **ScoreManager**
**Composants :**
- `LogParadeScoreManager`

**Liens :**
- ScoreManager → PlayerLogCollisionChecker, ScoreDisplayText
- ScoreManager ← référencé par GameTimer, GameController

---

#### 📦 **UIManager** (ou Canvas)
**Composants :**
- `LogParadeUIManager`

**Liens :**
- UIManager → Tous les éléments UI (textes, panels, indicateurs)
- UIManager ← référencé par GameController

---

#### 📦 **CalibrationManager** (optionnel)
**Composants :**
- `LogParadeCalibrationInteractive`

**Liens :**
- CalibrationManager → PlayerAvatar, LateralTracker, Lanes[4], CalibrationTextUI

---

#### 📦 **CalibrationUI** (requis si calibration interactive)
**Composants :**
- `CalibrationTextUI`

**Liens :**
- CalibrationUI → TextMeshProUGUI elements (instructions, status, timer)
- CalibrationUI ← référencé par LogParadeCalibrationInteractive

---

#### 📦 **UDPReceive** (existant)
**Composants :**
- `UDPReceive` (du système Core)

**Liens :**
- UDPReceive ← référencé par GameController, LateralTracker

---

### 🔗 **Hiérarchie des Dépendances**

1. **UDPReceive** (base - reçoit données MediaPipe)
2. **LogParadeLateralTracker** (dépend d'UDPReceive)
3. **LogParadePlayerAvatar** (dépend du LateralTracker)
4. **PlayerLogCollisionChecker** (sur PlayerAvatar)
5. **LogParadeScoreManager** (dépend du CollisionChecker)
6. **LogParadeLogGenerator** (indépendant, généré par Timer)
7. **LogParadeGameController** (orchestre tout)
8. **LogParadeGameTimer** (lance le jeu)

---

### ⚠️ **Points d'Attention**

- **Tags requis :** Les prefabs de rondins doivent avoir le tag `"Log"`
- **Layers :** Configurer les collisions entre joueur et rondins
- **Positions des Lanes :** X fixes à -3f, -1f, 1f, 3f
- **Ordre d'exécution :** GameStateController avant tous les autres
- **Calibration :** Le système démarre verrouillé jusqu'à calibration complète

---

### 🚀 **Démarrage Rapide**

1. Créer les GameObjects principaux selon la hiérarchie
2. Ajouter les scripts sur les bons objets
3. Assigner toutes les références dans l'inspecteur
4. Configurer les tags des prefabs de rondins
5. Tester la calibration interactive
6. Lancer le jeu via GameTimer

Le système s'auto-valide et affiche des logs détaillés pour diagnostiquer les problèmes de configuration.

---

## 🛠️ **Résolution des Problèmes Courants**

### ✅ **Lane Indicators et Lane Highlights - SUPPRIMÉS**

**Note :** Les systèmes `laneIndicators` et `laneHighlights` ont été complètement supprimés pour simplifier la configuration. Plus besoin de les configurer !

**Si vous voyez encore des erreurs :**
- Vérifiez que vous utilisez la dernière version des scripts
- Le système fonctionne maintenant sans ces éléments UI
   - Ajouter le script `LogParadeUIAutoSetup` sur un GameObject
   - Assigner le `LogParadeUIManager` dans la référence
   - Cliquer sur "Setup Basic UI" dans l'inspecteur
   - Assigner manuellement les références créées

---

### ❌ **Erreur : "IndexOutOfRangeException" dans la calibration**

**Cause :** Le système de calibration essaie d'accéder à des éléments d'array non configurés (laneTransforms, calibrationLogs)

**Solutions :**

1. **Vérifier la configuration des lanes :**
   ```
   Dans LogParadeCalibrationInteractive :
   - Assigner laneTransforms[0-3] : les 4 Transforms des lanes
   - Vérifier que tous les éléments sont non-null
   - S'assurer que l'array a exactement 4 éléments
   ```

2. **Configuration minimale pour éviter l'erreur :**
   ```
   1. Créer 4 GameObjects vides nommés "Lane1", "Lane2", "Lane3", "Lane4"
   2. Les positionner aux positions X: -3f, -1f, 1f, 3f (Y et Z à votre convenance)
   3. Assigner ces 4 Transforms dans laneTransforms[] du script LogParadeCalibrationInteractive
   4. Laisser calibrationLogs[] vide (sera rempli automatiquement)
   ```

3. **Option de contournement :**
   ```
   Si vous ne voulez pas utiliser la calibration interactive :
   1. Désactiver le GameObject avec LogParadeCalibrationInteractive
   2. Dans LogParadeLateralTracker, configurer :
      - enableAutoCalibration = true
      - bypassCalibrationForInteractiveMode = false
   ```

**Note :** La validation a été améliorée dans les dernières versions pour éviter ces crashes.

---

### ❌ **Erreur : "Composants invalides" (UI)**

**Cause :** Les références UI ne sont pas assignées dans l'inspecteur

**Solution :**
1. Ouvrir l'inspecteur du `LogParadeUIManager`
2. Assigner au minimum :
   - `gameStatusText` : Un TextMeshProUGUI pour le statut
   - `calibrationPanel` : Un GameObject pour la calibration  
   - ~~`laneIndicators` et `laneHighlights` : SUPPRIMÉS - plus nécessaires~~

**Astuce :** Le système peut fonctionner avec des références partielles, seuls les modules configurés seront actifs.

---

### ❌ **Erreur : "Aucun composant texte assigné"**

**Cause :** Aucun élément texte n'est assigné au `LogParadeGameStatusDisplay`

**Solution :**
1. Créer au moins un TextMeshProUGUI dans votre Canvas
2. L'assigner à l'une des références :
   - `currentLaneText`
   - `gameStatusText` 
   - `positionText`
   - `debugInfoText`

---

### ❌ **Erreur : "CalibrationTextUI non assigné"**

**Cause :** Le composant `CalibrationTextUI` requis par `LogParadeCalibrationInteractive` n'est pas créé ou assigné

**Solutions :**

1. **Option A - Création manuelle :**
   ```
   1. Créer un GameObject vide nommé "CalibrationUI"
   2. Ajouter le composant CalibrationTextUI
   3. Créer les éléments UI suivants en enfants :
      - TextMeshProUGUI nommé "InstructionText"      - TextMeshProUGUI nommé "StatusText" 
      - TextMeshProUGUI nommé "TimerText"
      - (Optionnel) Image nommée "ProgressBar"
      - ~~(Optionnel) 4 Images pour "LaneIndicators" : SUPPRIMÉ~~
   4. Assigner ces références dans CalibrationTextUI
   5. Assigner CalibrationUI dans LogParadeCalibrationInteractive
   ```

2. **Option B - Configuration simplifiée :**
   ```
   1. Créer un GameObject "CalibrationUI" avec CalibrationTextUI
   2. Créer seulement "InstructionText" (TextMeshProUGUI)
   3. Assigner uniquement mainInstructionText
   4. Le système fonctionnera avec ce minimum
   ```

3. **Option C - Désactiver la calibration interactive :**
   ```
   1. Ne pas utiliser LogParadeCalibrationInteractive
   2. Utiliser la calibration automatique du LateralTracker
   3. Configurer enableAutoCalibration = true dans LateralTracker
   ```

**Note :** CalibrationTextUI peut fonctionner avec des références partielles. Seul `mainInstructionText` est vraiment nécessaire.

---

### 🔧 **Configuration UI Rapide**

**Étape 1 :** Utiliser l'outil d'auto-setup
```csharp
// Sur un GameObject vide :
1. Ajouter le composant LogParadeUIAutoSetup
2. Assigner le LogParadeUIManager existant
3. Cliquer "Setup Basic UI" dans l'inspecteur
```

**Étape 2 :** Créer CalibrationTextUI (si calibration interactive requise)
```csharp
// Création manuelle rapide :
1. Créer GameObject "CalibrationUI"
2. Ajouter CalibrationTextUI
3. Créer enfant TextMeshProUGUI "InstructionText"
4. Assigner mainInstructionText dans CalibrationTextUI
5. Assigner CalibrationUI dans LogParadeCalibrationInteractive
```

**Étape 3 :** Assigner les références
- Le système créera automatiquement une UI basique
- Assigner manuellement les nouveaux éléments dans LogParadeUIManager
- Assigner CalibrationTextUI si calibration interactive utilisée
- Tester le jeu

---

### 📝 **Logs de Diagnostic**

Le système affiche des logs détaillés au démarrage :
- ✅ **Vert** : Module configuré correctement
- ⚠️ **Orange** : Module partiellement configuré (fonctionnalité limitée)
- ❌ **Rouge** : Module non configuré (désactivé)

**Console Unity :** Vérifiez la section "=== Validation de la configuration UI LogParade ===" pour identifier les problèmes.

## 🛠️ Installation et Configuration

1. Ouvrir le projet dans Unity 2022.3 LTS ou plus récent
2. Assurer que les packages nécessaires sont installés (MediaPipe, TextMeshPro)
3. Configurer les références dans l'inspecteur Unity
4. Tester avec les scripts de test fournis

## 📁 Structure du Projet

```
Motion-Party/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/                          # Scripts de base (MiniGameBase)
│   │   └── Gameplay/
│   │       └── LogParade/                 # Mini-jeu LogParade
│   │           ├── LogParadeGameController.cs
│   │           ├── LogParadeGameTimer/    # 🆕 Système de timer
│   │           ├── LogParadeScoreManager/ # Gestion du score
│   │           └── LogParadeLogGenerator/ # Génération des rondins
│   └── Scenes/                            # Scènes de jeu
├── python-tracker/                        # Système de tracking Python
└── README.md
```

## 🧪 Testing

### LogParadeGameTimer
- Utiliser `TestLogParadeGameTimer.cs` 
- Touches 1-5 pour différents tests
- Interface GUI intégrée pour contrôles visuels
- Vérification automatique des composants requis

---

*Projet développé dans le cadre d'une thèse en informatique*  
*Spécialement conçu pour les maisons de retraite*
