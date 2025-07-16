# LoadingScreen

## 📦 Nom et description
**LoadingScreen** : Écran de transition entre les mini-jeux, incluant des astuces pour le joueur et des éléments visuels d'attente.

## 🔧 Fonctionnalités principales

### ✅ Écran de chargement stylisé
- Interface utilisateur moderne avec animations fluides
- Fondu d'entrée et de sortie personnalisable
- Icône de chargement animée (rotation)
- Arrière-plan personnalisable par astuce

### ✅ Système d'astuces
- Affichage d'astuces contextuelles selon le mini-jeu
- Gestion par ID pour faciliter la maintenance
- Astuces par défaut en cas de données manquantes
- Texte multi-ligne supporté

### ✅ Images de prévisualisation
- Affichage optionnel d'une image du mini-jeu suivant
- Gestion automatique si l'image n'est pas disponible
- Support des sprites Unity

### ✅ Chargement asynchrone
- Chargement de scènes en arrière-plan
- Barre de progression visuelle
- Durée minimale d'affichage configurable
- Gestion des erreurs de chargement

### ✅ Transitions fluides
- Animation d'apparition/disparition
- Synchronisation avec le chargement de scène
- Callback système pour les événements

## 🚀 Instructions pour utiliser / intégrer

### 1. Setup initial
```csharp
// 1. Ajouter le prefab LoadingScreenUI dans une scène persistante
// 2. Configurer le LoadingScreenManager avec les composants nécessaires
// 3. Créer ou configurer les données LoadingScreenData
```

### 2. Utilisation basique
```csharp
// Afficher un écran de chargement simple
LoadingScreenManager.Instance.Show("movement_game");

// Charger une scène avec écran de chargement
LoadingScreenManager.Instance.ShowAndLoadScene("SceneName", "tipId", OnSceneLoaded);

// Cacher l'écran de chargement
LoadingScreenManager.Instance.Hide();
```

### 3. Intégration avec GameSessionManager
Le système s'intègre automatiquement avec `GameSessionManager` :
```csharp
// Dans MiniGameInfo, ajoutez :
public string loadingTipId; // ID de l'astuce à afficher

// Le GameSessionManager utilisera automatiquement l'écran de chargement
```

### 4. Configuration des données
```csharp
// Créer un ScriptableObject LoadingScreenData
// Ajouter des astuces avec leurs IDs correspondants
LoadingScreenData.LoadingTip tip = new LoadingScreenData.LoadingTip
{
    tipId = "movement_game",
    tipText = "Bouge doucement les bras !",
    gamePreviewImage = mySprite,
    backgroundColor = Color.blue
};
```

## 🧩 Éléments nécessaires

### Composants Unity requis
- **Canvas** : Pour l'affichage UI
- **CanvasGroup** : Pour les animations de fondu
- **TextMeshPro** : Pour l'affichage du texte (peut être remplacé par Text)
- **Image** : Pour l'arrière-plan et la prévisualisation
- **Slider** (optionnel) : Pour la barre de progression

### Scripts système
- **LoadingScreenManager** : Gestionnaire principal (Singleton)
- **LoadingScreenUI** : Interface utilisateur
- **LoadingScreenData** : Données de configuration (ScriptableObject)
- **LoadingScreenCreator** : Utilitaire pour créer les données par défaut

### Hierarchy recommandée
```
LoadingScreenCanvas
├── Background (Image)
├── ContentPanel
│   ├── GamePreview (Image - optionnel)
│   ├── TipText (TextMeshPro)
│   ├── LoadingIcon (Image rotative)
│   └── ProgressPanel (optionnel)
│       ├── ProgressBar (Slider)
│       └── ProgressText (TextMeshPro)
```

## ✅ Bonnes pratiques respectées

### 📝 Code modulaire et lisible
- Séparation claire des responsabilités
- Commentaires détaillés pour les méthodes complexes
- Nommage explicite des variables et méthodes
- Validation des composants au démarrage

### 🎛️ Configuration flexible
- Composants optionnels désactivables
- Durées d'animation configurables
- Système de fallback si des données manquent
- Support du pattern Singleton pour un accès global

### 🔄 Intégration facile
- Compatible avec l'architecture existante
- Modification minimale du GameSessionManager
- Utilisation de ScriptableObject pour les données
- Système de callback pour les événements

### 🛡️ Gestion d'erreurs
- Validation des composants requis
- Messages d'erreur explicites
- Comportement par défaut si des données manquent
- Nettoyage automatique des ressources

## 🎨 Personnalisation

### Animations
```csharp
[Header("Configuration d'animation")]
public float fadeInDuration = 0.5f;
public float fadeOutDuration = 0.5f;
public float loadingIconRotationSpeed = 360f;
```

### Données d'astuces
```csharp
// Créer de nouvelles astuces
LoadingScreenData.LoadingTip customTip = new LoadingScreenData.LoadingTip
{
    tipId = "custom_game",
    tipText = "Astuce personnalisée !",
    gamePreviewImage = previewSprite,
    backgroundColor = customColor
};
```

### Style visuel
- Modifiez les couleurs dans LoadingScreenData
- Changez les sprites d'icônes de chargement
- Ajustez la taille et position des éléments UI
- Personnalisez les animations dans LoadingScreenUI

## 🔧 Dépannage

### Problèmes courants
1. **Écran de chargement ne s'affiche pas** : Vérifiez que LoadingScreenManager.Instance existe
2. **Astuce par défaut toujours affichée** : Vérifiez l'ID de l'astuce dans MiniGameInfo
3. **Images manquantes** : Vérifiez que les sprites sont assignés dans LoadingScreenData
4. **Performance** : Utilisez LoadingScreenCreator pour créer les données par défaut

### Debug
```csharp
// Activer les logs de debug
Debug.Log("LoadingScreenManager: " + LoadingScreenManager.Instance != null);
Debug.Log("Current tip ID: " + tipId);
```

## 📱 Exemple d'utilisation complète

```csharp
// 1. Dans l'Inspector, configurer LoadingScreenManager
// 2. Créer les données avec LoadingScreenCreator
// 3. Dans MiniGameInfo, définir loadingTipId = "movement_game"
// 4. Le système fonctionne automatiquement avec GameSessionManager !
```

Le système est prêt à être utilisé et peut être facilement étendu pour de nouvelles fonctionnalités !
