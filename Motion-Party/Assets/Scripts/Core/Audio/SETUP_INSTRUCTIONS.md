# Instructions de Configuration Audio

## Problème Résolu

Le problème des AudioClips perdus lors des changements de scène a été résolu avec un système de configuration persistante basé sur ScriptableObject.

## Étapes de Configuration

### 1. Créer la Configuration Audio

1. Dans Unity, faites **clic droit** dans la fenêtre Project
2. Allez dans **Create > Motion Party > Audio Configuration**
3. Nommez le fichier `AudioConfiguration`
4. **Important** : Placez ce fichier dans le dossier `Assets/Resources/`

### 2. Assigner les AudioClips

Ouvrez votre fichier `AudioConfiguration.asset` et assignez :

#### Menu Audio
- **Menu Background Music** : Votre musique de menu principal
- **Menu Music Volume** : Volume de la musique de menu (0.0 à 1.0)

#### Mini-Game Music  
- **Music Note Press Music** : Musique pour le mini-jeu MusicNote
- **Log Parade Music** : Musique pour le mini-jeu LogParade
- **Firefly Dance Music** : Musique pour le mini-jeu FireflyDance
- **Mini Game Music Volume** : Volume des musiques de mini-jeux (0.0 à 1.0)

#### Sound Effects
- **Water Splash Sound** : Son d'éclaboussure pour LogParade
- **Firefly Capture Sound** : Son de capture pour FireflyDance
- **SFX Volume** : Volume des effets sonores (0.0 à 1.0)

#### Audio Settings
- **Fade Speed** : Vitesse des transitions audio (recommandé : 1.0)
- **Enable Debug Logs** : Cochez pour voir les logs audio

### 3. Vérification

1. Placez votre `AudioConfiguration.asset` dans `Assets/Resources/`
2. L'AudioManager se configurera automatiquement
3. Testez en changeant de scène - les sons doivent persister

## Structure des Dossiers

```
Assets/
├── Resources/
│   └── AudioConfiguration.asset    ← PLACEZ VOTRE CONFIG ICI
├── Scripts/
│   └── Core/
│       └── Audio/
│           ├── AudioManager.cs
│           ├── AudioConfiguration.cs
│           └── AudioInitializer.cs
└── Audio/                          ← VOS FICHIERS AUDIO
    ├── Music/
    │   ├── MenuMusic.wav
    │   ├── MusicNoteMusic.wav
    │   ├── LogParadeMusic.wav
    │   └── FireflyDanceMusic.wav
    └── SFX/
        ├── WaterSplash.wav
        └── FireflyCapture.wav
```

## Test et Diagnostic

### 1. Utilisation de AudioDebugHelper

1. Ajoutez le script `AudioDebugHelper` sur un GameObject dans votre scène
2. Dans l'Inspector, utilisez les boutons de test pour vérifier chaque élément audio
3. Utilisez "Debug Audio State" pour voir quels clips sont assignés

### 2. Vérifications Manuelles

1. **Configurez** votre AudioConfiguration avec tous les clips audio
2. **Lancez** le jeu depuis n'importe quelle scène
3. **Changez** de scène - la musique doit continuer
4. **Testez** chaque mini-jeu pour vérifier leur musique spécifique

## Problèmes Courants et Solutions

### LogParade ne joue pas de musique
- **Vérifiez** que `LogParade Music` est assigné dans votre AudioConfiguration
- **Activez** "Enable Debug Logs" et regardez la console pour les messages `[AudioManager]`
- **Utilisez** AudioDebugHelper pour tester spécifiquement LogParade

### Musique de menu se lance dans les mini-jeux
- **Solution appliquée** : La musique de menu ne se lance maintenant que dans les scènes de menu
- **Vérification** : La scène doit contenir "Menu", "Main", "Manager" dans son nom

### Musique ne revient pas au menu après un mini-jeu
- **Solution appliquée** : Système de redémarrage automatique avec délai
- **Vérification** : Le GameSessionManager doit être présent dans le menu

## En cas de problème

1. **Vérifiez** que votre `AudioConfiguration.asset` est dans `Assets/Resources/`
2. **Activez** "Enable Debug Logs" pour voir les messages de diagnostic
3. **Utilisez** AudioDebugHelper pour tester chaque élément individuellement
4. **Regardez** la Console Unity pour les messages `[AudioManager]`
5. **Utilisez** le menu contextuel "Debug Audio State" sur l'AudioManager

Le système fonctionne maintenant automatiquement avec les corrections suivantes :
- ✅ Musique de menu uniquement dans les bonnes scènes
- ✅ Redémarrage automatique de la musique de menu
- ✅ Debug amélioré pour LogParade et tous les mini-jeux
