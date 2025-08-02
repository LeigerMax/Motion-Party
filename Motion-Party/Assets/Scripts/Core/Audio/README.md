# 🎵 Système Audio Motion Party

## Vue d'ensemble

Le système audio de Motion Party offre une gestion centralisée de tous les éléments sonores du jeu :
- Musique de fond du menu principal
- Musiques spécifiques pour chaque mini-jeu 
- Effets sonores (éclaboussure, capture de lucioles, etc.)
- Transitions audio fluides avec fondus

## Architecture

### AudioManager (Core.Audio.AudioManager)
**Gestionnaire principal audio avec pattern Singleton**
- Gestion de la musique de fond (menu + mini-jeux)
- Gestion des effets sonores
- Transitions audio automatiques avec fondus
- Persistance entre les scènes

### AudioInitializer (Core.Systems.AudioInitializer)
**Initialisation automatique dans chaque scène**
- S'assure que l'AudioManager est présent
- Configuration automatique des AudioSources

## 🎮 Intégration dans les Mini-Jeux

### LogParade
```csharp
// Démarrage de la musique
AudioManager.Instance.PlayMiniGameMusic("LogParade");

// Son d'éclaboussure lors de la chute
AudioManager.Instance.PlayWaterSplashSound();

// Arrêt en fin de partie
AudioManager.Instance.StopMusic();
```

### FireflyDance
```csharp
// Démarrage de la musique
AudioManager.Instance.PlayMiniGameMusic("FireflyDance");

// Son de capture de luciole
AudioManager.Instance.PlayFireflyCaptureSound();

// Arrêt en fin de partie
AudioManager.Instance.StopMusic();
```

### MusicNotePress
```csharp
// Démarrage de la musique
AudioManager.Instance.PlayMiniGameMusic("MusicNote");

// Sons des notes (système existant maintenu)
// Géré par NoteSequenceManager.cs

// Arrêt en fin de partie
AudioManager.Instance.StopMusic();
```

## 🎵 Configuration Audio

### Fichiers Audio Requis

**Mini-Games Music:**
- `musicNotePressMusic` - Musique pour Music Note Press
- `logParadeMusic` - Musique pour Log Parade  
- `fireflyDanceMusic` - Musique pour Firefly Dance

**Menu Music:**
- `menuBackgroundMusic` - Musique de fond du menu principal

**Sound Effects:**
- `waterSplashSound` - Son d'éclaboussure (chute dans l'eau)
- `fireflyCaptureSound` - Son de capture de luciole

### Assignment dans l'Inspector

1. Créer un GameObject `AudioManager` dans la scène principale
2. Assigner le script `AudioManager.cs`
3. Configurer les AudioClips dans l'Inspector :
   ```
   Audio Sources:
   ├── Music Source (AudioSource pour la musique)
   └── SFX Source (AudioSource pour les effets)
   
   Audio Clips:
   ├── Menu Background Music
   ├── Music Note Press Music  
   ├── Log Parade Music
   ├── Firefly Dance Music
   ├── Water Splash Sound
   └── Firefly Capture Sound
   ```

## 🔧 Utilisation

### API Publique Principal

```csharp
// Musique
AudioManager.Instance.PlayMenuMusic();
AudioManager.Instance.PlayMiniGameMusic(string miniGameName);
AudioManager.Instance.StopMusic();

// Effets sonores
AudioManager.Instance.PlayWaterSplashSound();
AudioManager.Instance.PlayFireflyCaptureSound();
AudioManager.Instance.PlaySoundEffect(AudioClip clip);

// Configuration
AudioManager.Instance.SetMusicVolume(float volume);
AudioManager.Instance.SetSFXVolume(float volume);
AudioManager.Instance.SetDebugLogging(bool enabled);
```

### Noms des Mini-Jeux Supportés

- `"MusicNote"` ou `"MusicNotePress"` → musicNotePressMusic
- `"LogParade"` → logParadeMusic  
- `"FireflyDance"` ou `"Firefly"` → fireflyDanceMusic

## ✅ Fonctionnalités Implémentées

### ✅ Musique du Menu Principal
- Démarrage automatique au lancement de GameSessionManager
- Reprise lors du retour au menu depuis un mini-jeu

### ✅ Musiques des Mini-Jeux
- **MusicNotePress**: Musique démarrée en `Launch()`, arrêtée en `HandleGameFinished()`
- **LogParade**: Musique démarrée en `Launch()`, arrêtée en `HandleGameFinished()`  
- **FireflyDance**: Musique démarrée en `Launch()`, arrêtée en `OnGameEnded()`

### ✅ Effets Sonores
- **Son d'éclaboussure**: Joué automatiquement lors de la chute dans l'eau (LogParade)
- **Son de capture**: Joué automatiquement lors de la capture d'une luciole (FireflyDance)

### ✅ Transitions Audio
- Fondus automatiques entre les musiques
- Pas de coupures brutales
- Gestion des volumes séparés (musique/SFX)

## 🎯 Intégration Automatique

Le système est entièrement intégré dans le code existant :

1. **GameSessionManager** : Démarre la musique de menu au `Start()`
2. **Mini-Game Managers** : Démarrent/arrêtent automatiquement leur musique
3. **LogParade**: Son d'éclaboussure intégré dans `PlayerLogCollisionChecker`
4. **FireflyDance**: Son de capture intégré dans `FireflyCapture`

## 🛠️ Debug & Configuration

### Logs Debug
```csharp
AudioManager.Instance.SetDebugLogging(true);
```

### Menu Contextuel
- Clic droit sur AudioManager → `Force Initialize AudioManager`
- Vérification automatique des AudioSources manquantes

### Gestion d'Erreurs
- Vérifications de nullité pour tous les AudioClips
- Messages d'avertissement si les sons ne sont pas assignés
- Fallback gracieux si AudioManager n'est pas disponible

## 📁 Structure des Fichiers

```
Assets/Scripts/Core/Audio/
├── AudioManager.cs          # Gestionnaire principal
├── AudioInitializer.cs      # Initialisation automatique
└── README.md               # Cette documentation

Mini-Games Integration:
├── LogParade/Systems/PlayerLogCollisionChecker.cs    # Son éclaboussure
├── FireflyDance/Fireflies/FireflyCapture.cs         # Son capture
├── LogParade/Core/LogParadeGameManager.cs            # Musique LogParade
├── FireflyDance/Core/FireflyDanceGameManager.cs      # Musique FireflyDance
├── MusicNotePress/MusicNoteGameManager.cs            # Musique MusicNote
└── Systems/GameSessionManager.cs                     # Musique menu
```

Le système audio est maintenant entièrement fonctionnel et intégré dans Motion Party ! 🎵
