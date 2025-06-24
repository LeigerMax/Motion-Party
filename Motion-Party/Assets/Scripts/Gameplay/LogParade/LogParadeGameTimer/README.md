# 📦 LogParadeGameTimer
Gère le cycle de vie d'une partie (démarrage, minuterie, arrêt). Contrôle le score et le mouvement des rondins.

## 🔧 Fonctionnalités principales

- **Timer configurable** pour limiter la durée d'une partie
- **Démarrage manuel propre** via `LaunchLevel()`
- **Blocage du système** une fois le timer terminé
- **Intégration facile** avec le score manager et les rondins
- **Appels d'événements** pour UI ou écrans de fin
- **Compatibilité MiniGameBase** pour intégration avec le système global

## 🏗️ Structure du dossier

```
/LogParadeGameTimer/
├── LogParadeGameTimer.cs
└── README.md
```

## 🧪 Instructions pour tester

### Crée une scène avec :

1. **Le module LogParadeScoreManager** - Gestion du score
2. **Le module de génération des rondins** - LogParadeLogGenerator
3. **Le script LogParadeGameTimer** - Ce module

### Dans l'inspecteur :

1. **Fixer `gameDurationInSeconds`** à 30 (ou autre durée souhaitée)
2. **Assigner les références** aux managers (ou laisser la détection automatique)
3. **Configurer `startDelay`** pour le délai avant démarrage (défaut: 2s)

### Lancer LaunchLevel() :

- **Automatiquement** : Le système se lance via MiniGameBase
- **Manuellement** : Appeler `LaunchLevel()` via script ou interface debug
- **Interface debug** : Utiliser les boutons dans l'interface de debug (coin supérieur droit)

### Observer :

1. ✅ **Le score s'arrête** à la fin du timer
2. ✅ **Les rondins cessent de bouger** (pas de nouveaux rondins générés + arrêt du mouvement)
3. ✅ **Le joueur ne peut plus interagir** (le scoring est gelé)
4. ✅ **(Optionnel)** L'écran de fin peut s'afficher via les événements UnityEvent

## 🚀 Instructions pour utiliser / intégrer

### 1. Ajouter LogParadeGameTimer à un GameObject central
Exemple : GameController ou un GameObject dédié

### 2. Lier les références :
- **Le script de score** (`LogParadeScoreManager`) pour `PauseScoring()`
- **Le script de mouvement des rondins** (`LogParadeLogGenerator`) pour `StopLogMovement()`
- **(Optionnel)** Le `LogParadeGameController` principal

### 3. Appeler LaunchLevel() au début :
- **Via UI** : Bouton de démarrage
- **Automatiquement** : Via MiniGameBase quand le mini-jeu se lance
- **Via script** : `gameTimer.LaunchLevel();`

### 4. Gérer l'event OnGameEnd pour les écrans fin de partie :
```csharp
// Dans l'inspecteur ou via code
gameTimer.OnGameEnd.AddListener(() => {
    // Afficher écran de fin
    // Sauvegarder le score final
    // Proposer de rejouer
});
```

### 5. API Publique disponible :
```csharp
// Propriétés
bool IsGameActive { get; }      // Partie en cours ?
float TimeRemaining { get; }    // Temps restant
float GameDuration { get; }     // Durée totale

// Méthodes principales
void LaunchLevel()              // Démarre le niveau
void StopGame()                 // Arrêt manuel
void RestartGame()              // Redémarre une partie
void SetGameDuration(float)     // Change la durée (hors partie)
int GetCurrentScore()           // Score actuel

// Événements
UnityEvent OnGameStart          // Début de partie
UnityEvent OnGameEnd            // Fin de partie  
UnityEvent<float> OnTimerTick   // Chaque seconde (temps restant)
```

### 6. Interface de Debug configurable :
L'interface de debug GUI peut être repositionnée via l'inspecteur :
- **Debug GUI X** : Position horizontale en pixels
  - Valeur positive = distance depuis le bord gauche
  - Valeur négative = distance depuis le bord droit (ex: -250 = 250px du bord droit)
- **Debug GUI Y** : Position verticale en pixels depuis le haut

Utile si d'autres éléments UI sont déjà présents dans les coins de l'écran.

## 🔧 Correction du problème de démarrage automatique

**Problème résolu :** Les rondins se généraient automatiquement au démarrage, même avant que le timer ne soit lancé.

**Solutions apportées :**
1. **LogParadeLogGenerator modifié :**
   - Suppression du démarrage automatique dans `Start()`
   - Ajout de `StartLogGeneration()` - méthode publique pour contrôle externe
   - Le générateur s'initialise seulement au démarrage, sans commencer la génération

2. **LogParadeGameTimer amélioré :**
   - Utilise `StartLogGeneration()` au lieu de `ResumeGeneration()`
   - Nettoyage automatique des rondins existants à chaque nouvelle partie
   - Meilleur contrôle du cycle de vie des rondins

**Maintenant :** Les rondins ne se génèrent que quand le timer démarre explicitement via `LaunchLevel()`.

## 👤 Auteurs / liens utiles

- **Intégration MiniGameBase** : Compatible avec le système de mini-jeux global
- **Design Pattern** : Observer avec UnityEvents pour découplage maximum
- **Architecture modulaire** : Chaque responsabilité séparée (timer/score/mouvement)

---

## ✅ Bonnes pratiques respectées

✅ **Un seul responsable du timer** - LogParadeGameTimer ne gère QUE le cycle de vie

✅ **Pas de logique de score ou mouvement ici** - uniquement les ordres de démarrage/arrêt

✅ **Exposez OnGameStart et OnGameEnd** comme événements UnityEvent

✅ **Bien commenter chaque étape du cycle** - Logs explicites pour debug

✅ **Vérification que le jeu n'est pas déjà lancé** - éviter plusieurs `LaunchLevel()`

## 🛑 Important

⛔ **Ne pas calculer le score ou déplacer les rondins** en dehors de la période active.

✅ **L'état global du jeu doit être contrôlé** par ce module.

🔧 **Configuration dans l'inspecteur** pour adaptation facile aux besoins

⏰ **Timer précis** avec décompte par seconde et événements réguliers

---

*Module créé pour Motion-Party - LogParade Mini-Game*  
*Compatible avec l'architecture MiniGameBase existante*
