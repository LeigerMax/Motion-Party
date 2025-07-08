# CameraTransitionManager - Transitions Fluides de Caméra

📦 **Nom et description du module**
Gestionnaire de transitions fluides entre caméras pour le mini-jeu FireflyDance. Ce système remplace les switches instantanés par des mouvements fluides et cinématographiques entre différents points de vue.

**⚠️ Note importante** : Ce module utilise le namespace `CameraTransitions` pour éviter les conflits avec le type `UnityEngine.Camera`.

## 🎬 Fonctionnalités principales

### Transitions fluides
- **Interpolation Lerp** : Mouvement fluide de position et rotation
- **Courbe d'animation** : Contrôle de l'easing via AnimationCurve dans l'inspecteur
- **Durée configurable** : Chaque transition peut avoir sa propre durée
- **Switch désactivable** : Option pour revenir au comportement instantané

### Gestion non-bloquante
- **Coroutines** : Transitions asynchrones qui n'impactent pas le gameplay
- **Interruption sécurisée** : Possibilité d'arrêter une transition en cours
- **État de transition** : Vérification si une transition est active

### Intégration simple
- **Pattern Singleton** : Accès facile depuis n'importe où dans le code
- **Méthodes prédéfinies** : `TransitionToMainMenu()` et `TransitionToGameSelection()`
- **Méthode générique** : `StartTransition(Transform target)` pour d'autres caméras

## 🏗️ Architecture

```
/Camera/
├── CameraTransitionManager.cs         # Gestionnaire de base (Lerp + Coroutines)
├── CameraTransitionManagerAdvanced.cs # Version avancée avec support DOTween
├── DOTweenSetup.cs                    # Configuration automatique DOTween
└── README.md                         # Documentation du module
```

### Composants principaux
- **CameraTransitionManager** : Version de base utilisant Lerp et Coroutines
- **CameraTransitionManagerAdvanced** : Version avancée avec support DOTween optionnel
- **DOTweenSetup** : Configuration automatique des symboles de compilation
- **Main Camera** : La caméra qui se déplace physiquement (généralement Camera.main)
- **Caméras de destination** : Les caméras qui définissent les positions/rotations cibles
- **Transition Curve** : AnimationCurve pour l'easing du mouvement

## 🚀 Instructions d'utilisation

### 1. Setup initial dans Unity

#### Version de base (CameraTransitionManager)
1. **Créer un GameObject vide** nommé "CameraTransitionManager"
2. **Ajouter le script** `CameraTransitionManager.cs`
3. **Configurer les références** dans l'inspecteur

#### Version avancée (CameraTransitionManagerAdvanced)
1. **Créer un GameObject vide** nommé "CameraTransitionManagerAdvanced"
2. **Ajouter le script** `CameraTransitionManagerAdvanced.cs`
3. **Installer DOTween** (optionnel, voir section Installation)
4. **Configurer les références** dans l'inspecteur

#### Configuration commune des références :
   - `Main Camera` : Assigner la caméra principale de la scène (souvent Camera.main)
   - `Main Menu Camera` : Caméra représentant la position/rotation du menu principal
   - `Game Selection Camera` : Caméra représentant la position/rotation de sélection

### 2. Création des caméras de destination

```
Scene Hierarchy:
├── Main Camera (la caméra qui bouge, généralement Camera.main)
├── CameraTransitionManager
├── MainMenuCamera (caméra désactivée, utilisée comme position cible)
└── GameSelectionCamera (caméra désactivée, utilisée comme position cible)
```

**Configuration des caméras** :
1. **Main Camera** : Caméra active qui effectuera physiquement les transitions
2. **MainMenuCamera** : Positionnée à l'endroit désiré pour le menu principal
3. **GameSelectionCamera** : Positionnée à l'endroit désiré pour la sélection de jeu
4. Les caméras de destination seront automatiquement désactivées par le système
5. Assigner ces caméras dans le `CameraTransitionManager`

### 3. Intégration avec le MainMenuController

```csharp        // Dans MainMenuController.cs, modifier les méthodes ShowMainMenu() et ShowGameSelection()
        public void ShowMainMenu()
        {
            // Gestion des panneaux UI
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
            if (gameSelectionPanel != null)
                gameSelectionPanel.SetActive(false);

            // Transition de caméra fluide
            if (CameraTransitions.CameraTransitionManager.Instance != null)
                CameraTransitions.CameraTransitionManager.Instance.TransitionToMainMenu();

    // ...reste du code...
}        public void ShowGameSelection()
        {
            // Gestion des panneaux UI
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);
            if (gameSelectionPanel != null)
                gameSelectionPanel.SetActive(true);

            // Transition de caméra fluide
            if (CameraTransitions.CameraTransitionManager.Instance != null)
                CameraTransitions.CameraTransitionManager.Instance.TransitionToGameSelection();

    // ...reste du code...
}
```

## 🔧 Configuration

### Paramètres dans l'inspecteur

| Paramètre | Description | Valeur recommandée |
|-----------|-------------|-------------------|
| `Enable Camera Transition` | Active/désactive les transitions | `true` |
| `Default Transition Duration` | Durée par défaut des transitions | `2.0f` secondes |
| `Transition Curve` | Courbe d'easing pour le mouvement | `EaseInOut` |
| `Main Camera` | Caméra principale qui se déplace | `Camera.main` |
| `Main Menu Camera` | Caméra cible du menu principal | Caméra désactivée |
| `Game Selection Camera` | Caméra cible de sélection | Caméra désactivée |
| `Show Debug Logs` | Affiche les logs de debug | `true` (développement) |

### Courbes d'animation recommandées
- **EaseInOut** : Transition douce et naturelle
- **EaseIn** : Démarrage lent, accélération
- **EaseOut** : Démarrage rapide, ralentissement
- **Linear** : Vitesse constante
- **Custom** : Courbe personnalisée pour des effets spéciaux

## 📋 API publique

### Méthodes principales

```csharp
// Transition vers des positions prédéfinies
CameraTransitionManager.Instance.TransitionToMainMenu(duration);
CameraTransitionManager.Instance.TransitionToGameSelection(duration);

// Transition vers une position personnalisée avec une caméra
CameraTransitionManager.Instance.StartTransition(myCustomCamera, duration);

// Contrôle des transitions
CameraTransitionManager.Instance.StopCurrentTransition();
bool isMoving = CameraTransitionManager.Instance.IsTransitioning;

// Configuration runtime
CameraTransitionManager.Instance.SetTransitionsEnabled(false);
```

### Paramètres optionnels
- `duration` : Durée personnalisée (utilise `defaultTransitionDuration` si non spécifié)

## 🛠️ Intégration avec les systèmes existants

### Compatible avec
- **UI Canvas** : Les transitions n'affectent pas l'interface utilisateur
- **Cinemachine** : Peut coexister avec Cinemachine (éviter les conflits de caméra)
- **Timeline** : Compatible avec les séquences Timeline

### Bonnes pratiques
1. **Une seule caméra mobile** : Utiliser Camera.main comme caméra de transition
2. **Targets statiques** : Les positions cibles ne doivent pas bouger pendant la transition
3. **Gestion des interruptions** : Toujours vérifier `IsTransitioning` avant de démarrer une nouvelle transition
4. **Performance** : Désactiver les transitions sur mobiles faibles si nécessaire

## 🔧 Gestion des Audio Listeners

### Problème courant
Unity affiche souvent l'erreur : *"There are 2 audio listeners in the scene. Please ensure there is always exactly one audio listener in the scene."*

### Solution automatique
Le système de transition de caméra gère automatiquement les Audio Listeners :

1. **Désactivation automatique** : Les Audio Listeners des caméras de destination sont désactivés
2. **Activation garantie** : L'Audio Listener de la caméra principale reste actif
3. **Ajout si nécessaire** : Un Audio Listener est ajouté sur la caméra principale si manquant

### AudioListenerManager (Script utilitaire)
Un script dédié est fourni pour gérer manuellement les Audio Listeners :

```csharp
// Nettoyer automatiquement au démarrage
AudioListenerManager manager = FindObjectOfType<AudioListenerManager>();
manager.CleanupAudioListeners();

// Vérifier le nombre d'Audio Listeners actifs
int activeCount = manager.GetActiveAudioListenerCount();

// Afficher un rapport détaillé
manager.ShowAudioListenerReport();
```

### Configuration recommandée
1. **Ajouter AudioListenerManager** sur un GameObject vide dans la scène
2. **Activer autoCleanOnStart** pour un nettoyage automatique
3. **Utiliser les méthodes de context menu** pour debug manuel

## 🚀 Versions disponibles

### CameraTransitionManager (Version de base)
- **Lerp + Coroutines** : Utilise Unity Lerp pour les transitions
- **AnimationCurve** : Contrôle d'easing via courbes Unity
- **Léger et fiable** : Aucune dépendance externe
- **Compatible** : Fonctionne sur tous les projets Unity

### CameraTransitionManagerAdvanced (Version avancée)
- **Support DOTween** : Utilise DOTween si disponible, sinon fallback sur Lerp
- **Easing avancé** : Plus de 30 types d'easing DOTween
- **Performance optimisée** : DOTween est plus performant que Lerp
- **Options avancées** : Contrôle fin de la rotation et des chemins

### Quelle version choisir ?
- **Projets simples** : Utilisez `CameraTransitionManager`
- **Projets avec DOTween** : Utilisez `CameraTransitionManagerAdvanced`
- **Performance critique** : `CameraTransitionManagerAdvanced` avec DOTween
- **Mobile/WebGL** : `CameraTransitionManager` pour la taille de build

## 🎮 Installation de DOTween (Optionnel)

### Via Asset Store
1. Ouvrir l'Asset Store dans Unity
2. Chercher "DOTween Pro" ou "DOTween Free"
3. Importer le package dans le projet
4. Le script `DOTweenSetup.cs` configurera automatiquement les symboles

### Via Package Manager (Unity 2019.3+)
1. Ouvrir Window → Package Manager
2. Cliquer sur "+" → "Add package from git URL"
3. Entrer : `https://github.com/Demigiant/dotween.git`
4. Unity ajoutera DOTween automatiquement

### Configuration manuelle
Si la configuration automatique échoue :
1. Aller dans Edit → Project Settings → Player → Scripting Define Symbols
2. Ajouter `DOTWEEN_ENABLED` pour activer le support
3. Recompiler le projet (Ctrl+R)

## 🎨 Extensions futures

### Effets visuels
- **Post-processing** : Blur pendant la transition
- **Fondu** : Fade in/out pendant le mouvement
- **Particules** : Effets visuels accompagnant la transition

### Audio
- **Sons de transition** : Audio synchronisé avec le mouvement
- **Musique adaptive** : Changement de musique pendant la transition

### Animations avancées
- **Spline paths** : Chemins courbes entre les caméras
- **Multi-étapes** : Transitions avec points de passage intermédiaires
- **Caméra shake** : Légers tremblements pour plus de dynamisme

## 🧪 Test et Debug

### CameraTransitionTester
Un script utilitaire est fourni pour tester facilement les transitions :

```csharp
// Ajouter CameraTransitionTester.cs sur un GameObject vide
// Utiliser les touches 1, 2, Escape pour tester les transitions
// Interface UI optionnelle avec boutons de test
```

### Contrôles de test par défaut
- **Touche 1** : Transition vers menu principal
- **Touche 2** : Transition vers sélection de jeu  
- **Escape** : Arrêter la transition en cours
- **Touches 3-4** : Targets personnalisés (si configurés)

### Debug et vérifications
1. **Gizmos dans Scene View** : Visualiser les positions des caméras cibles
2. **Console Logs** : Messages détaillés sur l'état des transitions
3. **Interface GUI** : Affichage en temps réel de l'état du système
4. **Vérification des références** : Warnings automatiques si des éléments manquent

### Problèmes courants

| Problème | Solution |
|----------|----------|
| Transition instantanée | Vérifier que `enableCameraTransition = true` |
| Pas de mouvement | S'assurer que les caméras cibles sont assignées |
| Rotation bizarre | Activer `useShortestRotationPath` dans la version avancée |
| Performance faible | Utiliser DOTween ou réduire la durée de transition |
| Conflit avec Cinemachine | Désactiver temporairement Cinemachine ou utiliser une caméra dédiée |
| "2 audio listeners" | Utiliser `AudioListenerManager` ou vérifier la configuration automatique |
