# Menu Principal InteractifFireflyDance

📦 **Nom et description du module**
Menu principal interactif pour démarrer une partie dans le mini-jeu FireflyDance. Ce système fournit une interface utilisateur complète avec navigation entre différents panneaux UI et prise en charge future du contrôle vocal/gestuel.

## 🔧 Fonctionnalités principales

### Navigation entre panneaux UI et caméras
- **MainMenuPanel** : Panneau d'accueil avec le menu principal
- **GameSelectionPanel** : Panneau orienté vers les choix de jeu et options de lancement
- Système d'activation/désactivation des panneaux via `SetActive()`
- **Gestion optionnelle des caméras** : 
  - `MainMenuCamera` pour la vue du menu principal
  - `GameSelectionCamera` pour la vue de sélection de jeu
  - Basculement automatique entre les caméras lors de la navigation

### Lancement d'une partie en local via MiniGameBase
- Intégration avec le système `MiniGameBase` existant
- Gestion automatique des callbacks de fin de partie
- Retour automatique au menu principal après une partie

### Préparation du multijoueur et des niveaux à venir
- Boutons pré-configurés mais désactivés pour les fonctionnalités futures
- Structure extensible pour l'ajout de nouvelles fonctionnalités

### Support futur du contrôle vocal/gestuel
- Méthodes `SimulateClick()` pour chaque bouton
- Architecture préparée pour l'intégration de contrôles alternatifs

## 🏗️ Structure du dossier

```
/UI/Menu/
├── MainMenuController.cs          # Gère les interactions des boutons principaux
├── GameSelectionController.cs     # Gère la navigation et les actions de la caméra secondaire
└── README.md                     # Documentation du module
```

### MainMenuController.cs
- Gestion des boutons : Jouer, Options, Équipe, Quitter
- Navigation entre les panneaux UI (MainMenuPanel ↔ GameSelectionPanel)
- Gestion optionnelle des caméras (MainMenuCamera ↔ GameSelectionCamera)
- Méthodes de simulation pour contrôle vocal/gestuel futur

### GameSelectionController.cs
- Gestion des options de lancement de partie
- Boutons : Nouvelle partie locale, Multijoueur (grisé), Choisir un niveau (inactif), Retour
- Intégration avec le système MiniGameBase pour lancer le jeu

## 🚀 Instructions d'utilisation

### 1. Structure du Canvas et des panneaux
- Créer un Canvas principal pour tout le menu
- Ajouter deux enfants : `MainMenuPanel` et `GameSelectionPanel`
- Structure recommandée :
  ```
  Canvas
  ├── MainMenuPanel
  │   ├── Bouton Jouer
  │   ├── Bouton Options  
  │   ├── Bouton Équipe
  │   └── Bouton Quitter
  └── GameSelectionPanel
      ├── Bouton Nouvelle partie locale
      ├── Bouton Multijoueur (désactivé)
      ├── Bouton Choisir un niveau (désactivé)
      └── Bouton Retour
  ```

### 2. Configuration des références dans Unity
- Assigner `MainMenuController` à un GameObject parent (peut être le Canvas lui-même)
- Assigner `GameSelectionController` à un GameObject dans GameSelectionPanel
- Dans l'inspecteur du `MainMenuController` :
  - **UI Panels** : Connecter `mainMenuPanel` et `gameSelectionPanel` aux GameObjects correspondants
  - **Cameras (Optionnel)** : Connecter `mainMenuCamera` et `gameSelectionCamera` si vous voulez différentes vues
  - **UI Buttons** : Connecter tous les boutons (Jouer, Options, Équipe, Quitter)
  - **Game Selection Controller** : Connecter la référence au `GameSelectionController`

### 4. Lien avec le système MiniGameBase et GameSessionManager
- **GameSessionManager** : Le système détectera automatiquement le GameSessionManager et lancera une session complète de mini-jeux
- **Fallback MiniGameBase** : Si aucun GameSessionManager n'est trouvé, il cherchera un mini-jeu individuel
- Le GameSessionManager gère automatiquement la progression entre les mini-jeux
- Retour automatique au menu principal après la session complète

## 📌 Notes importantes

### Extensibilité pour contrôles alternatifs
- Chaque bouton dispose d'une méthode `SimulateClick()` publique
- Structure prête pour l'intégration du contrôle vocal ou gestuel
- Exemple d'utilisation : `mainMenuController.SimulatePlayClick()` pour déclencher le bouton Jouer

### Boutons inactifs et futurs
- **Multijoueur** : Bouton visible mais désactivé (`interactable = false`)
- **Sélection de niveau** : Bouton préparé pour l'ajout de niveaux futurs
- **Options** : Structure prête pour paramètres de jeu (son, langue, accessibilité)

### Indépendance du menu
- Le menu fonctionne indépendamment du mini-jeu spécifique
- Utilise l'interface `MiniGameBase` pour une compatibilité maximale
- Aucune dépendance directe sur l'implémentation du mini-jeu

## ⚙️ Configuration recommandée

1. **Canvas Setup** : Utiliser un Canvas en mode "Screen Space - Overlay"
2. **Event System** : S'assurer qu'un EventSystem est présent dans la scène
3. **Panneaux** : Organiser les éléments UI dans les panneaux MainMenuPanel et GameSelectionPanel
4. **Caméras (Optionnel)** : Créer des caméras séparées pour différentes vues si souhaité
5. **Hierarchy** : Structure claire avec Canvas parent et panneaux enfants
6. **Références** : Toutes les références assignées dans l'inspecteur du MainMenuController

## 🔄 Flux de navigation

```
MainMenuPanel (Menu principal)
    ↓ [Bouton Jouer]
GameSelectionPanel (Sélection de jeu)
    ↓ [Nouvelle partie locale]
Mini-jeu FireflyDance
    ↓ [Fin de partie]
MainMenuPanel (Retour au menu)
```

Ce système offre une base solide et extensible pour le menu principal du jeu, avec une architecture préparée pour les évolutions futures.
