# Guide de test pour les problèmes corrigés

## Problèmes identifiés et solutions

### 1. ❌ Problème de scènes inexistantes
**Problème** : `FireflyDance`, `LogParade`, `MusicNotePress` n'existent pas dans le projet
**Solution** : Configuration par défaut mise à jour avec `SampleScene` (3 mini-jeux de test)

### 2. ❌ NullReferenceException dans LoadingScreenManager
**Problème** : `loadingUI` était null, causant un crash ligne 159
**Solution** : Protections ajoutées + fallback vers chargement direct

### 3. ❌ Liste de mini-jeux vide après le premier jeu
**Problème** : Configuration perdue lors des changements de scène
**Solution** : `EnsureMiniGamesConfiguration()` recrée la liste si nécessaire

## Étapes de test

### 1. Vérifier la configuration des mini-jeux
1. Lancez le jeu
2. Regardez dans la Console Unity pour ces logs :
   ```
   [GameSessionManager] Configuration par défaut créée avec 3 mini-jeu(s)
   [GameSessionManager] Mini-jeu 0: SampleScene (Demo Game 1)
   [GameSessionManager] Mini-jeu 1: SampleScene (Demo Game 2)
   [GameSessionManager] Mini-jeu 2: SampleScene (Demo Game 3)
   ```

### 2. Tester la transition entre mini-jeux
1. Terminez le premier mini-jeu
2. Vérifiez ces logs :
   ```
   [GameSessionManager] Index actuel AVANT incrément: 0, Total mini-jeux: 3
   [GameSessionManager] Index actuel APRÈS incrément: 1
   [GameSessionManager] Chargement du mini-jeu 2/3: SampleScene
   ```

### 3. Vérifier le LoadingScreenManager
1. Si l'écran de chargement se bloque, regardez les logs :
   ```
   [LoadingScreenManager] Début chargement de SampleScene
   [LoadingScreenManager] loadingUI est null - impossible d'afficher l'écran de chargement
   ```
2. Dans ce cas, il y aura un chargement direct (fallback)

### 4. Tester les joueurs analytics
1. Sélectionnez 2 joueurs avant de commencer
2. Dans le dashboard Analytics, vérifiez :
   - "GameSessionManager Instance: ✅"
   - "Analytics Session Active: ✅"
   - "Joueurs actifs: 2" (au lieu de 0)

## Configuration recommandée pour tests

### Dans l'Inspector du GameSessionManager :
```
Mini Games (3 éléments) :
├── Element 0: Scene Name = "SampleScene", Display Name = "Demo Game 1"
├── Element 1: Scene Name = "SampleScene", Display Name = "Demo Game 2"
└── Element 2: Scene Name = "SampleScene", Display Name = "Demo Game 3"

Main Menu Scene Name = "MiniGameManager"
Enable Analytics = ✓ true
Enable Detailed Logs = ✓ true (pour debug)
```

## Si les problèmes persistent

### Pour les scènes manquantes :
1. Allez dans File → Build Settings
2. Ajoutez `SampleScene` si elle n'y est pas
3. Ou modifiez les noms de scènes dans la liste pour correspondre à vos vraies scènes

### Pour LoadingScreenManager :
1. Vérifiez qu'il y a un objet `LoadingScreenCanvas` dans la scène
2. Ou placez le prefab `LoadingScreenCanvas` dans le dossier `Resources/`

### Pour les analytics :
1. Utilisez le bouton "🔍 Diagnostiquer le problème" dans le dashboard
2. Vérifiez que les joueurs sont bien sélectionnés avant de lancer les mini-jeux

Le système devrait maintenant fonctionner avec ces 3 mini-jeux de test utilisant la même scène !
