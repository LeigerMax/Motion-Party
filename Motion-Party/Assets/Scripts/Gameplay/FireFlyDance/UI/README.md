# Interface Utilisateur FireflyDance

## 📦 Description

Ce module gère l'interface utilisateur du mini-jeu "Danse des Lucioles" (FireflyDance). Il fournit un système d'affichage simple et réactif pour les informations essentielles du jeu.

## 🔧 Fonctionnalités principales

### Affichage du Score
- Score actuel en temps réel

### Affichage du Timer
- Timer avec format MM:SS

### Gestion des États du Jeu
- Affichage du statut du jeu (En attente, En cours, Terminé, etc.)

## 🏗️ Structure du dossier

```
Assets/Scripts/Gameplay/FireFlyDance/UI/
├── FireflyDanceUIManager.cs        # Gestionnaire principal de l'UI
├── FireflyDanceScoreDisplay.cs     # Affichage spécialisé du score
├── FireflyDanceTimerDisplay.cs     # Affichage spécialisé du timer
└── README.md                       # Ce fichier
```

## 🚀 Instructions d'intégration

### 1. Placement du Canvas

Créez un Canvas dans votre scène avec les paramètres suivants :
- **Render Mode**: Screen Space - Overlay
- **Canvas Scaler**: Scale With Screen Size
- **Reference Resolution**: 1920x1080

### 2. Configuration manuelle des composants

**Important** : Tous les éléments UI doivent être configurés manuellement dans l'inspecteur Unity.

1. Ajoutez le script `FireflyDanceUIManager` à un GameObject dans votre scène
2. Créez manuellement vos éléments UI (Canvas, TextMeshPro, etc.)
3. Assignez les composants UI dans l'inspecteur du UIManager

### 3. Configuration du FireflyDanceUIManager

1. Ajoutez le script `FireflyDanceUIManager` à un GameObject dans votre scène
2. Assignez les composants UI dans l'inspecteur :

#### Configuration des références UI

Assignez manuellement dans l'inspecteur :
**Score UI**
- `currentScoreText`: TextMeshPro pour le score actuel

**Timer UI**
- `timerText`: TextMeshPro pour l'affichage du temps

**Game Status UI**
- `gameStatusText`: TextMeshPro pour le statut du jeu

### 4. Configuration des options UI

Dans l'inspecteur du `FireflyDanceUIManager` :

#### Configuration
- `config`: Référence vers le FireflyDanceConfig

### 5. Composants spécialisés (Optionnel)

Pour un contrôle plus fin, vous pouvez utiliser les composants spécialisés :

#### FireflyDanceScoreDisplay
- Permet la gestion indépendante de l'affichage du score

#### FireflyDanceTimerDisplay
- Gère l'affichage du timer de manière autonome

### 6. Connexions avec le GameManager

Le `FireflyDanceGameManager` détecte automatiquement le UIManager. Assurez-vous simplement que les deux sont dans la même scène.

## 📋 Scripts dépendants

### Requis
- `FireflyDanceEvents.cs` - Système d'événements
- `FireflyDanceConfig.cs` - Configuration du jeu
- `FireflyDanceLogger.cs` - Système de logging
- `FireflyDanceGameManager.cs` - Gestionnaire principal

### Optionnels
- `FireflyScoreManager.cs` - Pour la gestion des scores
- `FireflyDanceTimer.cs` - Pour le timer du jeu

## 🎨 Personnalisation

### Formats d'affichage
Les préfixes des textes peuvent être personnalisés :
- Score: `"Score: "` → `"Points: "`

## 🔧 API Publique

### FireflyDanceUIManager

#### Méthodes principales
- `ResetUI()`: Réinitialise l'interface

#### Mises à jour manuelles
- `UpdateScoreDisplay(int score)`: Met à jour le score
- `UpdateTimerDisplay(float timeRemaining)`: Met à jour le timer
- `UpdateGameStatusDisplay(string status)`: Met à jour le statut

### FireflyDanceScoreDisplay

#### Méthodes utiles
- `ForceUpdateDisplay(int score)`: Mise à jour forcée
- `ResetDisplay()`: Réinitialise l'affichage

### FireflyDanceTimerDisplay

#### Méthodes utiles
- `ForceUpdateTime(float time)`: Met à jour le temps
- `ResetTimer()`: Réinitialise le timer

## 🐛 Dépannage

### Problèmes courants

1. **L'UI ne se met pas à jour**
   - Vérifiez que les événements FireflyDance sont correctement déclenchés
   - Assurez-vous que les TextMeshPro sont bien assignés

2. **Le timer ne s'affiche pas correctement**
   - Vérifiez que FireflyDanceConfig est bien assigné
   - Assurez-vous que l'événement OnTimerTick est déclenché

3. **Erreurs de compilation**
   - Vérifiez que tous les scripts dépendants sont présents
   - Assurez-vous que les namespaces sont corrects

### ✅ Corrections récentes

- **FireflyDanceLogger.LogUI** : Méthode ajoutée pour les logs UI
- **FireflyDanceConfig.GameDuration** : Utilisation de la propriété correcte
- **GameState enums** : Utilisation des énumérations correctes (Idle, Playing, Finished)
- **Variables manquantes** : Toutes les références ont été corrigées

## 📝 Notes importantes

- L'UI s'initialise automatiquement au démarrage
- Les événements sont nettoyés automatiquement à la destruction
- Le système est conçu pour être modulaire et réutilisable
- Toute configuration se fait manuellement dans l'inspecteur Unity

## 🔄 Intégration avec d'autres systèmes

L'UI Manager s'intègre automatiquement avec :
- **GameManager**: Détection automatique et initialisation
- **ScoreManager**: Écoute des événements de score
- **Timer**: Écoute des événements de temps
- **Events System**: Utilise le système d'événements centralisé

Aucune configuration manuelle n'est requise pour ces intégrations.
