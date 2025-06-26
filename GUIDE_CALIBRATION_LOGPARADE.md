# 🎮 Guide d'Utilisation : Système de Calibration et Lancement LogParade

## 📋 Vue d'ensemble

Le système LogParade est maintenant organisé en séquence claire :

**1. Calibration Interactive** → **2. Lancement du Jeu**

### 🎯 Processus de Calibration

1. **Le joueur doit se rendre sur la lane 1** (3 secondes)
2. **Puis se rendre sur la lane 4** (3 secondes)
3. **Une fois terminé** → Le jeu se lance automatiquement

## 🚀 Comment Démarrer le Jeu

### Option 1: Processus Complet Automatique
```csharp
// Depuis un script
LogParadeGameLauncher launcher = FindFirstObjectByType<LogParadeGameLauncher>();
launcher.StartCompleteGameProcess();
```

### Option 2: Calibration Séparée
```csharp
// D'abord calibrer
launcher.StartCalibrationProcess();
// Le jeu se lance automatiquement après calibration
```

### Option 3: Interface Utilisateur
Utilisez le composant `LogParadeGameStarter` sur un GameObject avec des boutons UI.

## 🔧 Configuration Required

### Dans la Scène, vous devez avoir :

1. **LogParadeGameLauncher** - Orchestrateur principal
2. **LogParadeCalibrationManager** - Gestionnaire de calibration
3. **LogParadeCalibrationInteractive** - Système de calibration
4. **LogParadeGameStateController** - Contrôleur d'état
5. **CalibrationTextUI** - Interface de calibration

### Setup Minimal :
```
GameManager (GameObject)
├── LogParadeGameLauncher
├── LogParadeCalibrationManager  
└── LogParadeGameStateController

CalibrationSystem (GameObject)
├── LogParadeCalibrationInteractive
└── CalibrationTextUI

PlayerSystem (GameObject)
├── LogParadePlayerAvatar
└── LogParadeLateralTracker
```

## 📊 États du Système

Le système suit ces états :

1. **Initial** : Aucune calibration
2. **Calibration en cours** : Le joueur doit se déplacer
3. **Calibration terminée** : Prêt à jouer
4. **Jeu en cours** : LogParade actif

## 🐛 Debug et Test

### Méthodes de Debug Disponibles

Sur `LogParadeGameLauncher` (clic droit dans l'Inspector) :

- **"Debug - Start Complete Process"** : Lance tout le processus
- **"Debug - Force Launch Game"** : Bypass la calibration (test uniquement)
- **"Debug - Show System State"** : Affiche l'état actuel

### Vérification de l'État
```csharp
// Vérifier si on peut démarrer
bool canStart = LogParadeGameStateController.CanStartGameplay();

// Obtenir le statut actuel
string status = LogParadeGameStateController.GetCurrentStatusText();

// Vérifier si calibration en cours
bool calibrating = LogParadeGameStateController.IsCalibrationInProgress;
```

## ⚠️ Points d'Attention

1. **La calibration est obligatoire** avant le jeu (sauf bypass debug)
2. **Le GameStateController gère tous les verrous** d'état
3. **La calibration se lance automatiquement** au démarrage si configuré
4. **Le jeu se lance automatiquement** après calibration réussie

## 📝 Logs Utiles

Le système génère des logs détaillés :
- `🎯` Calibration
- `🚀` Lancement du jeu  
- `✅` Succès
- `❌` Erreurs
- `🔄` Redémarrages

## 🎮 Utilisation depuis l'UI

Pour créer une interface simple :

1. Ajoutez `LogParadeGameStarter` à un GameObject
2. Connectez vos boutons UI
3. Les boutons s'adaptent automatiquement à l'état du jeu

### Boutons Recommandés :
- **"Jouer"** → `StartCompleteGame()`
- **"Redémarrer"** → `RestartGame()`
- **"Calibrer"** → `StartCalibrationOnly()`
