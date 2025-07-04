# 🦋 FireflyDance - Guide d'intégration Unity

## 📋 Vue d'ensemble
Ce guide détaille **étape par étape** comment configurer le mini-jeu FireflyDance dans Unity, de la création des GameObjects jusqu'aux tests finaux.

---

## 🛠️ **ÉTAPE 1 : Préparation de la scène**

### 1.1 Créer la scène
1. Créer une nouvelle scène : `MiniGame_FireflyDance.unity`
2. Sauvegarder dans : `Assets/Scenes/MiniGames/`

### 1.2 Configuration de base
1. **Camera** : Position `(0, 0, -10)`, Projection `Orthographic`, Size `5`
2. **Lighting** : Réduire l'intensité pour ambiance nocturne
3. **Background** : Couleur sombre (bleu nuit : `#1a1a2e`)

---

## 🎮 **ÉTAPE 2 : GameManager principal**

### 2.1 Créer le GameObject GameManager
```
Hierarchy:
└── FireflyDanceGameManager (Empty GameObject)
    ├── FireflyDanceGameManager.cs
    ├── FireflyDanceStateController.cs
    └── FireflyDanceLogger.cs
```

### 2.2 Configuration du GameManager
1. **Créer GameObject vide** : `FireflyDanceGameManager`
2. **Ajouter scripts** :
   - `FireflyDanceGameManager.cs`
   - `FireflyDanceStateController.cs`
   - `FireflyDanceLogger.cs`

3. **Assigner dans l'inspecteur** :
   - `Config` → Glisser `FireflyDanceConfig.asset` depuis le dossier Core
   - `State Controller` → Référence automatique (même GameObject)
   - `Spawner` → Sera assigné à l'étape 4
   - `Hand Tracker` → Sera assigné à l'étape 3
   - `Hand Interactor` → Sera assigné à l'étape 3
   - `Score Manager` → Sera assigné à l'étape 5

---

## 🖐️ **ÉTAPE 3 : Système de suivi de la main**

### 3.1 Créer le GameObject Hand
```
Hierarchy:
└── HandSystem (Empty GameObject)
    ├── HandTracker.cs
    ├── HandInteractor.cs
    └── HandVisual (Child GameObject)
        ├── Sphere (Primitive)
        ├── SphereCollider (Trigger)
        └── Material HandMaterial
```

### 3.2 Configuration détaillée

#### 3.2.1 GameObject principal "HandSystem"
1. **Créer GameObject vide** : `HandSystem`
2. **Position** : `(0, 0, 0)`
3. **Ajouter scripts** :
   - `HandTracker.cs`
   - `HandInteractor.cs`

**Note** : `HandPositionConverter.cs` est une classe statique et ne s'attache pas sur un GameObject.

#### 3.2.2 Sous-objet "HandVisual" 
1. **Créer enfant** : `HandVisual` (GameObject vide)
2. **Créer petit-enfant** : `HandSphere` (Primitive Sphere)
3. **Configuration HandSphere** :
   - **Scale** : `(0.2, 0.2, 0.2)`
   - **Material** : Créer nouveau matériau `HandMaterial`
   - **Couleur par défaut** : Bleu `#0080ff` (main ouverte)

#### 3.2.3 Collider pour interactions
1. **Sur HandSphere, ajouter** : `SphereCollider`
2. **Configuration** :
   - ✅ `Is Trigger` = True
   - `Radius` = `1.0` (pour zone de capture)
   - `Center` = `(0, 0, 0)`

#### 3.2.4 Configuration des scripts

**HandTracker.cs** :
- `Hand Visual` → Référence vers `HandVisual`
- `UDP Port` → `5005` (doit correspondre au script Python)
- `Smooth Speed` → `5.0`

**HandInteractor.cs** :
- `Hand Tracker` → Référence vers `HandTracker`
- `Hand Collider` → Référence vers le `SphereCollider`
- `Capture Radius` → `1.0`

**Note** : `HandPositionConverter` est automatiquement utilisé par `HandTracker` (classe statique).

---

## 🦋 **ÉTAPE 4 : Système de lucioles**

### 4.1 Créer le prefab Firefly

#### 4.1.1 GameObject Firefly de base
```
Prefab: FireflyPrefab
└── Firefly (Empty GameObject)
    ├── FireflyController.cs
    ├── VisualEffect (Child)
    │   ├── Sphere (Primitive, Scale 0.1)
    │   ├── Light (Point Light)
    │   └── ParticleSystem (Optionnel)
    ├── SphereCollider (Trigger)
    └── Rigidbody (Kinematic)
```

#### 4.1.2 Configuration détaillée

**GameObject principal "Firefly"** :
1. **Créer GameObject vide** : `Firefly`
2. **Ajouter script** : `FireflyController.cs`
3. **Ajouter** : `SphereCollider` + `Rigidbody`

**SphereCollider** :
- ✅ `Is Trigger` = True
- `Radius` = `0.5`

**Rigidbody** :
- ✅ `Is Kinematic` = True (mouvement contrôlé par script)

**Enfant "VisualEffect"** :
1. **Créer enfant** : `VisualEffect`
2. **Ajouter Sphere primitive**
3. **Configuration Sphere** :
   - **Scale** : `(0.1, 0.1, 0.1)`
   - **Material** : Matériau émissif jaune
   - **Couleur** : `#ffff00` avec émission

**Light enfant** :
1. **Ajouter enfant** : `Light` (Point Light)
2. **Configuration** :
   - `Color` = Jaune `#ffff00`
   - `Intensity` = `2.0`
   - `Range` = `3.0`

#### 4.1.3 Sauvegarder le prefab
1. **Glisser vers** : `Assets/Prefabs/FireflyDance/`
2. **Nom** : `FireflyPrefab`

### 4.2 Créer le FireflySpawner

#### 4.2.1 GameObject Spawner
```
Hierarchy:
└── FireflySpawner (Empty GameObject)
    └── FireflySpawner.cs
```

1. **Créer GameObject vide** : `FireflySpawner`
2. **Ajouter script** : `FireflySpawner.cs`
3. **Configuration** :
   - `Config` → Glisser `FireflyDanceConfig.asset`
   - `Firefly Prefab` → Glisser `FireflyPrefab`
   - `Spawn Parent` → Créer GameObject vide "ActiveFireflies"

#### 4.2.2 Zone de spawn (optionnel visuel)
1. **Créer enfant** : `SpawnZone` (Cube wireframe)
2. **Configuration** :
   - **Scale** : Selon la zone définie dans `FireflyDanceConfig`
   - **Material** : Wireframe transparent
   - **Disable Mesh Renderer** en mode release

---

## 🏆 **ÉTAPE 5 : Système de score**

### 5.1 Créer le ScoreManager
```
Hierarchy:
└── ScoreManager (Empty GameObject)
    └── FireflyScoreManager.cs
```

1. **Créer GameObject vide** : `ScoreManager`
2. **Ajouter script** : `FireflyScoreManager.cs`
3. **Configuration** :
   - `Config` → Glisser `FireflyDanceConfig.asset`

### 5.2 UI Score (optionnel)
```
Canvas:
└── ScoreUI
    ├── ScoreText (Text - Legacy ou TextMeshPro)
    ├── ComboText
    └── EncouragementText
```

Si vous voulez l'UI :
1. **Créer Canvas** : `ScoreCanvas`
2. **Ajouter Text** pour afficher le score
3. **Lier dans ScoreManager** si nécessaire

---

## 🔗 **ÉTAPE 6 : Liaisons finales**

### 6.1 Connecter tout dans GameManager
Retour au `FireflyDanceGameManager`, assigner :

- `Spawner` → Référence vers `FireflySpawner`
- `Hand Tracker` → Référence vers `HandTracker`
- `Hand Interactor` → Référence vers `HandInteractor`
- `Score Manager` → Référence vers `FireflyScoreManager`

### 6.2 Vérifications
- ✅ Tous les scripts ont leurs références
- ✅ Le prefab Firefly est bien assigné
- ✅ La configuration est partagée partout
- ✅ Les colliders sont en mode Trigger
- ✅ Les layers sont corrects (si utilisés)

---

## 🎯 **ÉTAPE 7 : Configuration UDP**

### 7.1 Script Python externe
1. **Démarrer** : `python-tracker/main.py`
2. **Vérifier port** : `5005` (même que dans HandTracker)
3. **Tester la connexion** avant Unity

### 7.2 UDPReceive dans Unity
1. **Trouver ou créer** : GameObject avec `UDPReceive.cs`
2. **Configuration** :
   - `Port` = `5005`
   - `Auto-start` = True

---

## 🧪 **ÉTAPE 8 : Tests**

### 8.1 Test sans mouvement
1. **Play** dans Unity
2. **Vérifier Console** : Logs de démarrage
3. **Observer** : Lucioles qui apparaissent automatiquement

### 8.2 Test avec mouvement
1. **Démarrer Python tracker**
2. **Play** dans Unity
3. **Bouger la main** devant la caméra
4. **Observer** : 
   - Sphere bleue qui suit la main
   - Devient rouge quand main fermée
   - Capture les lucioles au contact

### 8.3 Test scoring
1. **Capturer quelques lucioles**
2. **Vérifier Console** : Messages de score
3. **Observer** : Points qui s'accumulent

---

## 🏗️ **Structure finale dans Hierarchy**

```
Hierarchy (MiniGame_FireflyDance):
├── Main Camera
├── Directional Light
├── FireflyDanceGameManager
│   ├── FireflyDanceGameManager.cs
│   ├── FireflyDanceStateController.cs
│   └── FireflyDanceLogger.cs
├── HandSystem
│   ├── HandTracker.cs
│   ├── HandInteractor.cs
│   └── HandVisual
│       └── HandSphere (Sphere + SphereCollider)
├── FireflySpawner
│   ├── FireflySpawner.cs
│   └── ActiveFireflies (Empty, parent des lucioles)
├── ScoreManager
│   └── FireflyScoreManager.cs
├── UDPReceiver (si pas déjà présent)
│   └── UDPReceive.cs
└── UI (optionnel)
    └── Canvas avec textes de score
```

---

## 📁 **Assets requis**

### Prefabs nécessaires :
- `Assets/Prefabs/FireflyDance/FireflyPrefab.prefab`

### ScriptableObjects :
- `Assets/Scripts/Gameplay/FireflyDance/Core/FireflyDanceConfig.asset`

### Matériaux :
- `HandMaterial` (bleu/rouge pour la main)
- `FireflyMaterial` (émissif jaune)

---

## 🎮 **Test final**

1. ✅ **Python tracker démarré**
2. ✅ **Play dans Unity**
3. ✅ **Bouger la main → Sphere suit**
4. ✅ **Fermer/ouvrir main → Couleur change**
5. ✅ **Lucioles apparaissent automatiquement**
6. ✅ **Contact main fermée + luciole → Capture**
7. ✅ **Score augmente dans Console**

---

## 🐛 **Dépannage courant**

### ⚠️ Erreurs Unity communes :

**"Can't add script behaviour 'HandPositionConverter'. The script class can't be abstract!"**
- ✅ **Solution** : Ne pas attacher `HandPositionConverter` - c'est une classe statique
- ✅ **Utilisation** : Automatiquement utilisée par `HandTracker`

### Main ne bouge pas :
- Vérifier que Python tracker fonctionne
- Port UDP correct (5005)
- UDPReceive présent et actif
- **NOUVEAU** : Utilisez `HandSystemDiagnostic` → "Run Full Diagnostic"

### Pas de lucioles :
- Vérifier FireflyDanceConfig assigné
- Spawner.StartSpawning() appelé ?
- Prefab Firefly correct ?

### Pas de capture :
- Colliders en mode Trigger ?
- HandInteractor correctement configuré ?
- Events bien connectés ?

### Hand visual ne s'affiche pas :
- **NOUVEAU** : Ajoutez `HandSystemDiagnostic` pour diagnostic automatique
- **NOUVEAU** : Utilisez `HandVisualCreator` → "Create Test Hand Visual"
- **NOUVEAU** : Testez avec `HandVisualTester` → "Show Hand Visual"
- Vérifiez que HandVisual est assigné dans HandTracker
- Position du visual dans le champ de vision de la caméra

### Erreurs de compilation :
- Tous les using statements présents ?
- References bien assignées ?
- Namespaces corrects ?

---

## 🛠️ **Outils de debug avancés**

### Scripts de debug disponibles :

1. **HandSystemDiagnostic** - Diagnostic automatique complet
   ```csharp
   // Ajoutez à un GameObject, puis :
   // Clic droit → "Run Full Diagnostic"
   // Clic droit → "Quick Fix Attempt"
   ```

2. **HandTrackerDebugger** - Debug en temps réel avec GUI
   ```csharp
   // Active showDebugGUI pour interface de debug
   ```

3. **HandVisualTester** - Test indépendant du hand visual
   ```csharp
   // Clic droit → "Show Hand Visual"
   // Clic droit → "Check Hierarchy"
   ```

4. **HandVisualCreator** - Création automatique d'un hand visual
   ```csharp
   // Clic droit → "Create Test Hand Visual"
   ```

### Commandes de debug HandTracker :
- "Test Show Hand Visual" - Force l'affichage
- "Test Simulate Hand Detection" - Simule une détection
- "Debug Hand System" - Debug complet
- "Debug UDP Data" - Affiche données UDP

### Guide de résolution détaillé :
Consultez `DEBUG_HAND_VISUAL_GUIDE.md` pour un guide complet de résolution des problèmes de hand visual.

---

🎉 **Voilà ! Votre mini-jeu FireflyDance est prêt !** 

Le système est maintenant entièrement fonctionnel et modulaire. Vous pouvez facilement ajuster les paramètres dans `FireflyDanceConfig` pour équilibrer le gameplay.
