# 📦 LogParadeLogGenerator

Générateur de rondins en descente pour le mini-jeu LogParade avec logique de chemin jouable et vitesse adaptée aux seniors.

## 🔧 Fonctionnalités principales

- **Génération contrôlée de rondins sur 4 voies fixes** : Spawn automatique avec positions X prédéfinies (-3, -1, +1, +3)
- **Maintien d'un chemin jouable pour le joueur** : Algorithme de validation qui garantit qu'il y a toujours des rondins adjacents accessibles
- **Vitesse adaptée à un public senior** : Descente lente à 1.2 unités/seconde pour une expérience confortable
- **Suppression des superpositions et collisions indésirables** : Espacement vertical minimum de 2.5 unités entre rondins
- **Gestion par buffer avec anticipation et validation** : Système de queue pour préparer et valider chaque vague de rondins

## 🏗️ Structure du dossier

```
/Assets/Scripts/Gameplay/LogParade/LogParadeLogGenerator/
├── LogParadeLogGenerator.cs    # Générateur principal
├── LogParadeLog.cs            # Composant des rondins individuels
└── README.md                  # Documentation du module
```

## 🎮 Configuration requise

### Prefabs de rondins à créer manuellement

Vous devez créer **3 prefabs** dans votre projet Unity :

1. **`LogPrefab_Short`** - Rondin court (taille recommandée : 1x1x1 unités)
2. **`LogPrefab_Medium`** - Rondin moyen (taille recommandée : 1x1x1.5 unités)  
3. **`LogPrefab_Long`** - Rondin long (taille recommandée : 1x1x2 unités)

**Important :** 
- Chaque prefab doit avoir un **Collider** (BoxCollider recommandé)
- Ajoutez un **Rigidbody** si vous souhaitez des interactions physiques
- Utilisez des matériaux visuellement distincts pour chaque type

### Voies (Lanes) à créer dans la scène

Créez **4 GameObjects vides** dans votre scène pour représenter les voies :

1. **Lane_1** - Position X sera automatiquement fixée à -3
2. **Lane_2** - Position X sera automatiquement fixée à -1
3. **Lane_3** - Position X sera automatiquement fixée à +1
4. **Lane_4** - Position X sera automatiquement fixée à +3

## 🧪 Instructions pour tester

1. **Créer une scène de test** :
   - Créez une nouvelle scène Unity
   - Ajoutez un GameObject vide nommé "LogParadeGenerator"

2. **Configurer le générateur** :
   - Attachez le script `LogParadeLogGenerator` au GameObject
   - Créez 4 GameObjects enfants pour les voies (Lane_1 à Lane_4)
   - Assignez les 4 voies dans le champ "Lanes" de l'inspecteur
   - Assignez vos 3 prefabs de rondins dans le champ "Log Prefabs"

3. **Paramètres par défaut** (déjà configurés) :
   - ✅ Vitesse : 1.2 unités/seconde
   - ✅ Espacement vertical : 2.5 unités
   - ✅ Intervalle de génération : 2.5 secondes
   - ✅ Hauteur de spawn : Y = 10
   - ✅ Hauteur de destruction : Y = -5

4. **Lancer le test** :
   - Démarrez la scène en mode Play
   - Les rondins doivent apparaître au sommet et descendre lentement
   - Vérifiez qu'il n'y a pas de superpositions
   - Vérifiez que chaque rangée a toujours un chemin jouable

## 🚀 Instructions pour utiliser / intégrer

### Intégration dans votre mini-jeu

1. **Ajout du générateur à la scène** :
   ```csharp
   // Le générateur hérite de MiniGameBase
   // Il sera automatiquement géré par votre MiniGameManager
   ```

2. **Configuration dans l'inspecteur** :
   - **Lanes** : Assignez les 4 transforms des voies (obligatoire)
   - **Log Prefabs** : Assignez les 3 prefabs LogPrefab_Short, LogPrefab_Medium, LogPrefab_Long (obligatoire)
   - **Enable Generation** : Cochez pour activer la génération automatique
   - **Show Debug Info** : Cochez pour afficher les informations de debug

3. **Contrôle par script** :
   ```csharp
   LogParadeLogGenerator generator = GetComponent<LogParadeLogGenerator>();
   
   // Arrêter la génération
   generator.StopGeneration();
   
   // Reprendre la génération
   generator.ResumeGeneration();
   
   // Nettoyer tous les rondins
   generator.ClearAllLogs();
   ```

### Événements et interactions

Le composant `LogParadeLog` sur chaque rondin fournit :

```csharp
// Événement de destruction
logComponent.OnDestroyed += () => {
    Debug.Log("Rondin détruit !");
};

// Informations utiles
int laneIndex = logComponent.GetLaneIndex(); // 0-3
bool isMoving = logComponent.IsMoving();
float currentY = logComponent.GetCurrentY();
```

## 🛠️ Paramètres personnalisables

### Dans l'inspecteur Unity :

| Paramètre | Description | Valeur par défaut |
|-----------|-------------|-------------------|
| `Log Speed` | Vitesse de descente (unités/sec) | 1.2 |
| `Vertical Spacing` | Espacement vertical minimum | 2.5 |
| `Generation Interval` | Fréquence de génération (sec) | 2.5 |
| `Spawn Height` | Hauteur d'apparition (Y) | 10 |
| `Destroy Height` | Hauteur de destruction (Y) | -5 |
| `Enable Generation` | Active/désactive la génération | True |
| `Show Debug Info` | Affiche les infos de debug | False |

### Debug et test :

- **Gizmos dans Scene View** : Activez "Show Debug Info" pour voir les lignes de voies
- **Logs en console** : Informations sur chaque rangée générée
- **Touches de debug** : Appuyez sur `Espace` en mode Play pour voir les statistiques

## ⚠️ Points d'attention

- **Dépendance MiniGameBase** : Le module hérite de `MiniGameBase` comme les autres mini-jeux
- **Pas de génération automatique de voies** : Les 4 voies doivent être créées manuellement dans la scène
- **Validation automatique des chemins** : L'algorithme garantit qu'il y a toujours un chemin jouable
- **Gestion mémoire** : Les rondins sont automatiquement détruits en bas de l'écran
- **Optimisation** : Maximum 50 rondins actifs simultanément (ajustable si besoin)

## 🔧 Personnalisation avancée

### Modifier l'algorithme de placement :

Dans `LogParadeLogGenerator.cs`, la méthode `CreateValidLogRow()` peut être personnalisée pour :
- Changer le nombre de rondins par rangée (actuellement 1-3)
- Modifier la logique de connectivité
- Ajouter des patterns spécifiques

### Ajouter des interactions :

Dans `LogParadeLog.cs`, la méthode `OnPlayerContact()` peut être étendue pour :
- Système de points
- Effets visuels et sonores
- Mécaniques de gameplay spécifiques

### Performance :

Le système est optimisé pour :
- ✅ Faible consommation CPU (coroutines au lieu d'Update intensif)
- ✅ Gestion automatique de la mémoire
- ✅ Validation efficace des chemins
- ✅ Debug tools intégrés

## 👤 Compatibilité

- **Unity 2021.3+** : Compatible
- **URP/HDRP** : Compatible  
- **Mobile** : Optimisé pour mobile
- **VR** : Compatible (testé en mode desktop)

---

*Module créé pour Motion-Party - LogParade Mini-Game*  
*Optimisé pour un public senior avec vitesse réduite et validation de chemins*
