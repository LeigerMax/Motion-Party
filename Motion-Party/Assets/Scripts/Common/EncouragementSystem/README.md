# Système d'Encouragement pour Mini-Jeux Motion Party

## Vue d'ensemble

Le système d'encouragement affiche des messages motivants aléatoires pendant la partie des mini-jeux LogParade, Firefly et MusicNote. Chaque message peut être accompagné d'un fichier audio pour une expérience plus immersive.

## Fonctionnalités principales

- ✅ Messages aléatoires toutes les 15-20 secondes
- ✅ Support audio pour chaque message
- ✅ Évite la répétition de messages consécutifs
- ✅ Interface UI animée avec fondu d'entrée/sortie
- ✅ Configuration via ScriptableObject
- ✅ Intégration automatique avec les mini-jeux
- ✅ Système modulaire et réutilisable

## Structure des fichiers

```
Scripts/Common/EncouragementSystem/
├── EncouragementMessage.cs          # Structure de données pour un message
├── EncouragementManager.cs          # Gestionnaire principal du système
├── EncouragementDisplay.cs          # Affichage UI des messages
├── EncouragementConfiguration.cs    # ScriptableObject de configuration
└── MiniGameEncouragementIntegrator.cs # Intégration automatique
```

## Messages par défaut

Le système inclut 23 messages d'encouragement thématiques :

**Messages génériques :**
- "Bien joué !"
- "Continue comme ça !"
- "Super, tu y es presque !"
- "Bravo, tu progresses !"
- "C'est parti, tu gères !"
- "Excellent !"
- "Tu es dans le rythme !"
- "Parfait !"
- "Tu assures !"
- "Encore un peu !"
- "Doucement mais sûrement !"
- "Tu fais ça très bien !"
- "C'est fluide, continue !"
- "Tu tiens le bon rythme !"
- "Belle précision !"
- "Tout en maîtrise !"

**Messages thématiques colonie :**
- "Les copains vont être fiers !"
- "Le camp entier t'applaudit !"
- "Le chef de colo te félicite !"
- "T'es la star de la veillée !"
- "Les animateurs sont bluffés !"
- "On t'offre la première part de marshmallow !"
- "Le feu de camp est à toi !"

**Message custom :**
1- Tu les attrapes comme un pro !
2- Une de plus dans le filet ! 
3- Vise bien, elle ne t’échappera pas
4- C’était précis, bravo !
5- Attention, celle-ci est rapide !
6- Elles ne peuvent plus t’échapper ! 
7- Bien joué, elle n’a rien vu venir ! 
8- Encore une et c’est la gloire du camp !

9- Hop, un rondin de plus !
10- Belle traversée, continue !
11- Tu es agile comme un écureuil !
12- Les copains sur la berge t’applaudissent !
13- Cette rivière est à toi ! 


14- Parfait, tu es dans le tempo ! 
15- Bravo, quelle oreille ! 
16- Parfaitement en rythme
17- J'espère que tu ne triche pas 
18- Chaque note est un succès ! 
19- Tu tiens le rythme du début à la fin !

## Utilisation

### 1. Intégration automatique

Ajoutez le composant `MiniGameEncouragementIntegrator` à votre mini-jeu :

```csharp
// Le composant détecte automatiquement le type de jeu et s'abonne aux événements
// Démarre automatiquement quand le jeu commence
// S'arrête automatiquement quand le jeu se termine
```

### 2. Intégration manuelle

```csharp
// Démarrer le système
encouragementManager.StartEncouragement();

// Arrêter le système
encouragementManager.StopEncouragement();

// Afficher un message immédiatement
encouragementManager.ShowRandomMessage();
```

### 3. Configuration via ScriptableObject

1. Créez une configuration : `Create > Motion Party > Encouragement > Configuration`
2. Assignez-la au `EncouragementManager`
3. Personnalisez les messages, timing et audio

## Configuration audio

### Préparation des fichiers audio

1. **Format recommandé :** MP3 ou WAV
2. **Durée :** 2-4 secondes maximum
3. **Qualité :** 44.1kHz, 16-bit minimum
4. **Compression :** Utilisez les paramètres d'import Unity optimaux

### Attribution des fichiers audio

1. Dans l'inspecteur du `EncouragementConfiguration`
2. Pour chaque message, assignez le `AudioClip` correspondant
3. Les noms de fichiers peuvent correspondre au texte pour faciliter l'organisation

### Structure recommandée des fichiers audio

```
Assets/Audio/Encouragement/
├── bien_joue.mp3
├── continue_comme_ca.mp3
├── super_tu_y_es_presque.mp3
├── bravo_tu_progresses.mp3
└── ... (autres fichiers)
```

## Paramètres de configuration

### Timing
- **minInterval** : Intervalle minimum entre messages (défaut: 15s)
- **maxInterval** : Intervalle maximum entre messages (défaut: 20s)
- **displayDuration** : Durée d'affichage du message (défaut: 3s)

### Audio
- **audioVolume** : Volume des fichiers audio (0-2, défaut: 1.5)
- **playAudioWithMessage** : Activer/désactiver l'audio

### UI
- **messagePosition** : Position du message à l'écran
- **textColor** : Couleur du texte
- **fontSize** : Taille de la police
- **backgroundColor** : Couleur de fond du panneau

## Tests et débogage

### Tests en éditeur

1. Cochez `testInEditor` dans l'`EncouragementManager`
2. Appuyez sur `E` en mode Play pour déclencher un message
3. Cochez `enableDebugLogs` pour voir les logs détaillés

### Tests avec l'intégrateur

1. Cochez `testInEditor` dans le `MiniGameEncouragementIntegrator`
2. Appuyez sur `T` en mode Play pour un message immédiat

## Intégration dans les mini-jeux

### LogParade
✅ **Intégré** - Le système démarre automatiquement quand le jeu commence

### Firefly
✅ **Intégré** - Le système démarre automatiquement avec le jeu et s'arrête à la fin

### MusicNote
✅ **Intégré** - Le système démarre automatiquement avec le jeu et s'arrête à la fin

## Personnalisation avancée

### Créer des messages spécifiques par jeu

```csharp
// Exemple pour LogParade
var logParadeMessages = new List<EncouragementMessage>
{
    new EncouragementMessage("Esquive ce rondin !", audioClip, 3f),
    new EncouragementMessage("Tu maîtrises la rivière !", audioClip, 3f)
};
```

### Conditions d'affichage personnalisées

```csharp
// Exemple : afficher un message seulement après un certain score
public void OnScoreChanged(int newScore)
{
    if (newScore > 100 && !hasShownHighScoreMessage)
    {
        encouragementManager.ShowRandomMessage();
        hasShownHighScoreMessage = true;
    }
}
```

## Troubleshooting

### Problèmes courants

1. **Les messages ne s'affichent pas**
   - Vérifiez que `StartEncouragement()` est appelé
   - Vérifiez que la liste de messages n'est pas vide
   - Activez `enableDebugLogs` pour diagnostiquer

2. **L'audio ne joue pas**
   - Vérifiez que les `AudioClip` sont assignés
   - Vérifiez le volume du `AudioSource`
   - Vérifiez que `playAudioWithMessage` est activé

3. **L'UI ne s'affiche pas correctement**
   - Vérifiez la présence d'un Canvas parent
   - Vérifiez les paramètres de positionnement
   - Vérifiez que le `CanvasGroup` est configuré

## Performance

Le système est optimisé pour :
- ✅ Minimal impact sur les performances
- ✅ Gestion mémoire efficace des AudioClips
- ✅ Animations fluides sans frame drops
- ✅ Éviter les allocations mémoire fréquentes

## Extensions futures

- [ ] Support des animations de texte personnalisées
- [ ] Messages contextuels basés sur les performances
- [ ] Support multilingue
- [ ] Statistiques d'engagement des messages
- [ ] Integration avec le système d'analytics
